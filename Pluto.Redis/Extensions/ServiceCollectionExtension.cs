using System;
using System.Collections.Generic;


#if NETCOREAPP
using Microsoft.Extensions.DependencyInjection;
#endif

using Pluto.Redis.Options;

namespace Pluto.Redis.Extensions
{
#if NETCOREAPP
    
    public static class ServiceCollectionExtension
    {

        public static IServiceCollection AddPlutoRedis(this IServiceCollection services,Action<RedisClientOption> options)
        {
            services.Configure(options);
            services.AddSingleton<RedisClient>();
            return services;
        } 


        public static RedisClientBuilder CreateRedisClientBuilder(this IServiceCollection services)
        {
            return new RedisClientBuilder();
        } 


        public static void AddRedisClient(this IServiceCollection services,Func<RedisClientBuilder,RedisClient> build)
        {
            services.AddSingleton(build.Invoke(new RedisClientBuilder()));
        } 

    }
#endif


    public class RedisClientBuilder
    {
        private RedisClientOption options;

        public RedisClientBuilder ConnectTo(Dictionary<string, int> dictionary)
        {

            return this;
        }

        
        public RedisClientBuilder UseAuth(string pwd)
        {
            return this;
        }

        public RedisClientBuilder ConfigOptions(Action<RedisClientOption> optionsAction)
        {
            options = new RedisClientOption();
            optionsAction(options);
            return this;
        }

       


        public RedisClient Build()
        {
            return new RedisClient(options);
        }

    }
}