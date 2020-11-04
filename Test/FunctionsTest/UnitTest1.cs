using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using Pluto.Redis;
using Pluto.Redis.Extensions;
using Pluto.Redis.Interfaces;
using Pluto.Redis.Options;
using StackExchange.Redis;

namespace FunctionsTest
{
    public class Tests
    {

        internal IServiceProvider _serviceProvider;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.Configure<RedisClientOption>(o =>
            {
                o.DefaultDbNumber = 0;
                o.InstanceName = "test";
                o.Password = "pluto1002";
                o.MasterName = "redis-master";
                o.IsSentinelModel = true;
                o.AllowAdmin = true;
                o.KeepAlive = 180;
                o.RedisAddress=new Dictionary<int, string>
                {
                    // this is sentinel model endpoint
                    {26379,"0.0.0.0"},
                    {26380,"0.0.0.0"},
                    {26381,"0.0.0.0"},
                };
            });
            services.AddSingleton<IPlutoRedisClient, PlutoRedisClient>();
            _serviceProvider = services.BuildServiceProvider();
        }

        [Test]
        public void StringSet()
        {
            var aaa = _serviceProvider.GetService<IPlutoRedisClient>();
            var res= aaa.Set("demo", "123123qweqwew",3600);
            Assert.IsTrue(res);
        }

        [Test]
        public void DatabaseExtensionMethod()
        {
            var aaa = _serviceProvider.GetService<IPlutoRedisClient>();
            var res= aaa.GetDatabase(3);
            Assert.IsTrue(res.Set("asdasd","mkmkmkmk",200)); // extension method 
        }



        [Test]
        public void PubAndSub()
        {
            var aaa = _serviceProvider.GetService<IPlutoRedisClient>();
            aaa.Publish("demo",JsonConvert.SerializeObject(new {a=111111}));
            Console.WriteLine(123123);
        }


        [Test]
        public void Lock()
        {
            var aaa = _serviceProvider.GetService<IPlutoRedisClient>();
            Assert.IsTrue(aaa.Lock("admin_lock",100));
        }


        [Test]
        public void UnLock()
        {
            var aaa = _serviceProvider.GetService<IPlutoRedisClient>();
            Assert.IsTrue(aaa.UnLock("admin_lock"));
        }
    }
}