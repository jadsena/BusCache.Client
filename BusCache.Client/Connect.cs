using BusCache.Client.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BusCache.Client
{
    public class Connect : IConnect
    {
        private readonly object objSend = new object();
        private TcpClient Client { get; }
        private ILogger<Connect> Logger { get; }
        public ServerOptions ServerOptions { get; }
        public bool IsConnect { get => Client.Connected; }

        private bool disposed = false;
        public event EventHandler<string> Receive;
        public event EventHandler ConnectionClose;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        /// <summary>
        /// Cria nova instancia conectada ao servidor pré-configurado
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategoryName}"/></param>
        /// <param name="options"><see cref="IOptions{TOptions}"/></param>
        /// <exception cref="OverflowException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="EncoderFallbackException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public Connect(ILogger<Connect> logger, IOptions<ServerOptions> options)
        {
            Logger = logger;
            ServerOptions = options?.Value;
            Client = new TcpClient();
            var policy = Policy
              .Handle<Exception>()
              .WaitAndRetry(new[] {
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(15)
              });

            policy
                .Execute(() =>
                {
                    Logger.LogInformation($"Logando no servidor [{ServerOptions.IP}] na porta [{ServerOptions.Port}] com o nome [{ServerOptions.ServiceName}]");
                    Client.Connect(ServerOptions.IP, ServerOptions.Port);
                });

            Task thread = new Task(() => ReceiveData(Client), cts.Token);

            thread.Start();
            SendData($"rg {ServerOptions.ServiceName}");
            Logger.LogInformation($"Logado com sucesso no servidor [{ServerOptions.IP}] na porta [{ServerOptions.Port}] com o nome [{ServerOptions.ServiceName}]");
        }
        /// <summary>
        /// Envia dados para o servidor
        /// </summary>
        /// <param name="Data">Dados a serem enviados</param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="EncoderFallbackException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="IOException"></exception>
        public void SendData(string Data)
        {
            if (!Client.Connected) return;
            lock (objSend)
            {
                NetworkStream ns = Client.GetStream();
                byte[] buffer = Encoding.ASCII.GetBytes(string.Concat(Data, "\n"));
                ns.Write(buffer, 0, buffer.Length);
            }
            Thread.Sleep(ServerOptions.TimeoutSendComand);
        }
        public void Execute(string Command)
        {
            SendData(Command);
        }
        /// <summary>
        /// Envia dados para o servidor
        /// </summary>
        /// <param name="Data">Dados a serem enviados</param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="EncoderFallbackException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="IOException"></exception>
        public void SendDataToService(string serviceName, string Data)
        {
            string Command = $"sm {serviceName} \"{Data}\"";
            SendData(Command);
        }
        public void SetKey(string key, string value)
        {
            string Command = $"set {key} {value}";
            SendData(Command);
        }
        public void GetKey(string key)
        {
            string Command = $"get {key}";
            SendData(Command);
        }
        /// <summary>
        /// Recebe dados do servidor
        /// </summary>
        /// <param name="client"><see cref="TcpClient"/></param>
        /// <returns><see cref="Task"/></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void ReceiveData(TcpClient client)
        {
            NetworkStream stream = null;
            StreamReader sr = null;
            string Text = "";
            try
            {
                while (!cts.IsCancellationRequested && (!Text.ToLower(CultureInfo.CurrentCulture).Equals("quit",StringComparison.OrdinalIgnoreCase)))
                {
                    stream = client.GetStream();
                    try
                    {
                        sr = new StreamReader(stream);
                        Text = sr.ReadLine();
                    }
                    catch (IOException ex)
                    {
                        Logger.LogError(message: $"received ERROR from [{ServerOptions.IP}]:[{ServerOptions.Port}]");
                        Logger.LogError(ex.ToString());
                        break;
                    }

                    if (Text.ToLower(CultureInfo.CurrentCulture).Equals("quit", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.LogDebug(Text);
                        Logger.LogDebug(Environment.NewLine);
                        continue;
                    }
                    Receive?.Invoke(this, Text);
                }
            }
            catch (InvalidOperationException ex)
            {
                Logger.LogError($"received ERROR from [{ServerOptions.IP}]:[{ServerOptions.Port}]");
                Logger.LogError(ex.ToString());
            }
            catch (IOException ex)
            {
                Logger.LogError($"received ERROR from [{ServerOptions.IP}]:[{ServerOptions.Port}]");
                Logger.LogError(ex.ToString());
            }
            sr?.Dispose();
            ConnectionClose?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
                if (!disposed)
                {
                    SendData("quit");
                    if (Client != null)
                    {
                        Client.Close();
#if !NET45
                        Client?.Dispose();
#endif
                    }
                    cts?.Dispose();
                    disposed = true;
                }
        }
    }
}
