using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Pluto.Redis.Interfaces;
using Pluto.Redis.Options;

namespace Pluto.Redis.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddPlutoRedis(this IServiceCollection services,Action<RedisClientOption> options)
        {
            services.Configure(options);
            services.AddSingleton<IPlutoRedisClient, PlutoRedisClient>();
            return services;
        } 
    }
}