[![Build Status](https://jadsena.visualstudio.com/BusCache/_apis/build/status/jadsena.BusCache.Client?branchName=master)](https://jadsena.visualstudio.com/BusCache/_build/latest?definitionId=5&branchName=master)

# BusCache.Client
Cliente de conexão .Net Standard com o server [BusCache](https://github.com/jadsena/BusCache)

## Adicionando o pacote nuget
1. Via Package Manager
```
   Install-Package BusCache.Client
```
2. Via .Net CLI
```
   dotnet add package BusCache.Client --version 0.2.2
```
3. Via PackageReference
```
   <PackageReference Include="BusCache.Client" Version="x.x.x" />
```
## Utilizando o pacote
1. Adicionando o namespace Extensions
```
   using BusCache.Client.Extensions;
```
2. Adicionando o serviço via DependencyInjection
```
        private void ConfigureServices(ServiceCollection services)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConfiguration(Config.GetSection("Logging"));
                loggingBuilder.AddConsole();
            });
            services.AddOptions();
            services.AddBusCacheClient(Config.GetSection("ServerOptions"));
        }
```
3. Adicionando no arquivo appsettings.config a configuração necessária
```
{
  "ServerOptions": {
    "IP": "127.0.0.1",
    "Port": 7289,
    "ServiceName": "ClientTeste1",
    "TimeoutSendComand": 100
  }
}
```
## Usando o Client para enviar dados para o [BusCache](https://github.com/jadsena/BusCache) Server
1. Na classe program existe todo o codigo para instanciar um serviço via DependencyInjection e o mesmo pode ser utilizado assim:
```
            using Program p = new Program();
            Console.WriteLine("ls");
            p.Execute("ls");
            Console.WriteLine("Set");
            p.Set("Teste", strjson);
            Console.WriteLine("Get");
            p.Get("Teste");

```
2. A classe Service1, está servindo como proxy para o serviço Connect que enviar e recebe dados od servidor [BusCache](https://github.com/jadsena/BusCache)
```
    public class Service1 : IDisposable
    {
        private readonly IConnect _connect;
        public event EventHandler<string> Receive;
        public Service1(IConnect connect)
        {
            _connect = connect;
            _connect.Receive += Connect_Receive;
        }

        private void Connect_Receive(object sender, string e)
        {
            Receive?.Invoke(this, e);
        }

        public void Set(string key, string value)
        {
            _connect.SetKey(key, value);
        }
        public void Get(string key)
        {
            _connect.GetKey(key);
        }
        public void Execute(string command)
        {
            _connect.Execute(command);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                _connect.Dispose();
            }
        }
    }
```
Obs.: Todos o código de teste pode ser encontrado no projeto BusCache.ConsoleClient 
