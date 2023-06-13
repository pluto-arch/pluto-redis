using System;
using System.Collections.Generic;
using System.Linq;


#if !NET461
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
#endif

using Pluto.Redis.Options;

namespace Pluto.Redis.Extensions
{
#if !NET461
    public static class ServiceCollectionExtension
    {

        public static IServiceCollection AddRedisClient(this IServiceCollection services,Action<ConfigurationOptions> options)
        {
            services.Configure(options);
            services.AddSingleton<RedisClient>();
            return services;
        } 
    }
#endif

}