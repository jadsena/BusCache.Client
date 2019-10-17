using BusCache.Client.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusCache.Client.Extensions
{
    public static class ClientExtensions
    {
        public static IServiceCollection AddBusCacheClient(this IServiceCollection services, IConfiguration options)
        {
            services.Configure<ServerOptions>(options);
            return services.AddSingleton<IConnect, Connect>();
        }
    }
}
