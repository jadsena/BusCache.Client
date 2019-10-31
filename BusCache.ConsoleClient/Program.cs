using BusCache.Client;
using BusCache.Client.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BusCache.ConsoleClient
{
    class Program : IDisposable
    {
        private IConfiguration Config { get; set; }
        private ILogger<Program> Logger { get; set; }
        private ServiceProvider Provider { get; }
        private Service1 Service { get; }

        static void Main(string[] args)
        {
            Dictionary<string, object> keys = new Dictionary<string, object>
            {
                { "teste", "teste1" },
                { "teste2", "teste1" },
                { "teste3", "teste1" },
                { "teste4", "teste1" },
                { "teste5", "teste1" },
                { "teste6", "teste1" },
                { "teste7", "teste1" }
            };

            string strjson = JsonConvert.SerializeObject(keys);

            using Program p = new Program();
            Console.WriteLine("ls");
            p.Execute("ls");
            Console.WriteLine("Set");
            p.Set("Teste", strjson);
            Console.WriteLine("Get");
            p.Get("Teste");

            Console.ReadKey();
        }
        public Program()
        {
            Configure();
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            Provider = serviceCollection.BuildServiceProvider();
            Logger = Provider.GetService<ILoggerFactory>().CreateLogger<Program>();
            Service = Provider.GetService<Service1>();
            Service.Receive += Connect_Receive;
        }

        private void Connect_Receive(object sender, string e)
        {
            Logger.LogInformation(e);
        }

        public void Set(string key, string value)
        {
            Service.Set(key, value);
        }
        public void Get(string key)
        {
            Service.Get(key);
        }
        public void Execute(string command)
        {
            Service.Execute(command);
        }
        private void Configure()
        {
            Config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .Build();
        }
        private void ConfigureServices(ServiceCollection services)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConfiguration(Config.GetSection("Logging"));
                loggingBuilder.AddConsole();
            });
            services.AddOptions();
            services.AddBusCacheClient(Config.GetSection("ServerOptions"));
            services.AddTransient<Service1>();
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
                Service.Dispose();
            }
        }
    }
}
