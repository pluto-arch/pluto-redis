using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Pluto.Redis;
using Pluto.Redis.Extensions;
using Pluto.Redis.Interfaces;
using Pluto.Redis.Options;

namespace FunctionsTest
{
    public class Tests
    {

        internal IServiceProvider _serviceProvider;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddPlutoRedis(o =>
            {
                o.DefaultDbNumber = 0;
                o.InstanceName = "test";
                o.Password = "pluto1002";
                o.MasterName = "redis-master";
                o.IsSentinelModel = true;
                o.AllowAdmin = true;
                o.KeepAlive = 180;
                o.RedisAddress = new Dictionary<int, string>
                {
                    // this is sentinel model endpoint
                    {26379,"123.57.71.234"},
                    {26380,"123.57.71.234"},
                    {26381,"123.57.71.234"},
                };
            });
            _serviceProvider = services.BuildServiceProvider();
        }

        [Test]
        public async Task StringSet()
        {
            var aaa = _serviceProvider.GetService<IPlutoRedisClient>();
            var res= aaa.Set("demo", "123123qweqwew",3600);

            res = aaa.Set("demo2",()=>JsonSerializer.Serialize<Demo>(new Demo { Name="123"}),222);

            Assert.IsTrue(res);

            res =await aaa.SetAsync("demo2", () => JsonSerializer.Serialize<Demo>(new Demo { Name = "123" }), 222);

            Assert.IsTrue(res);
        }

        [Test]
        public void DatabaseExtensionMethod()
        {
            
            var client = _serviceProvider.GetService<IPlutoRedisClient>();
            var aaa = _serviceProvider.GetService<IPlutoRedisClient>();
            var res = aaa.GetDatabase(3);
            Assert.IsTrue(res.Set("asdasd", "mkmkmkmk", 200)); // extension method 

            Assert.IsTrue(res.Set("asdasd", () => JsonSerializer.Serialize<Demo>(new Demo { Name = "123" }), 200)); // extension method 
        }

        class Demo
        {
            public string Name { get; set; }            
        }


        [Test]
        public void PubAndSub()
        {
            var client1 = _serviceProvider.GetService<IPlutoRedisClient>();
            client1.Publish("demo", ()=>JsonSerializer.Serialize(new { a="12321"}));


            var client2 = _serviceProvider.GetService<IPlutoRedisClient>();
            client2.Subscribe("demo", (cjennel, message) => {
                var model = JsonSerializer.Deserialize<dynamic>(message);
                Assert.AreEqual("12321", model.a);
            });
        }


        [Test]
        public void Lock()
        {
            var aaa = _serviceProvider.GetService<IPlutoRedisClient>();
            Assert.IsTrue(aaa.Lock("admin_lock",Environment.MachineName,100));
        }


        [Test]
        public void UnLock()
        {
            var aaa = _serviceProvider.GetService<IPlutoRedisClient>();
            Assert.IsTrue(aaa.UnLock("admin_lock", Environment.MachineName));
        }
    }
}