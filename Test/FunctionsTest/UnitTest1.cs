using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Pluto.Redis;
using Pluto.Redis.Extensions;
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

            //services.CreateRedisClientBuilder()
            //    .ConnectTo(new Dictionary<string, int>
            //    {
            //        {"localhost",49153},
            //    }).Build();


            //services.AddRedisClient(builder =>builder.ConnectTo(null).UseAuth("").Build());

            services.AddPlutoRedis(o =>
            {
                o.CommandMap=CommandMap.Default;
                o.DefaultDataBase = 0;
                o.InstanceName = "test";
                o.Password = "redispw";
                o.KeepAlive = 180;
                o.RedisAddress = new Dictionary<string, int>
                {
                    {"localhost",49153},
                };
            });
            _serviceProvider = services.BuildServiceProvider();
        }

        [Test]
        public async Task StringSet()
        {
            var aaa = _serviceProvider.GetRequiredService<RedisClient>();
            var res= await aaa.Db.SetAsync("demo", "123123qweqwew",3600);

            res = await aaa.Db.SetAsync("demo2",()=>JsonSerializer.Serialize<Demo>(new Demo { Name="123"}),222);

            Assert.IsTrue(res);

            res =await aaa.Db.SetAsync("demo2", () => JsonSerializer.Serialize<Demo>(new Demo { Name = "123" }), 222);

            Assert.IsTrue(res);

            var ddd = await aaa.Db.GetAsync("demo");
            var ddd3 = aaa.Db.Get<Demo>("demo2", (str) => JsonSerializer.Deserialize<Demo>(str));
            Assert.IsTrue(ddd!=null);


            await aaa[1].SetAsync("hds", "sdsd", 10);
        }

        [Test]
        public void DatabaseExtensionMethod()
        {
            var aaa = _serviceProvider.GetRequiredService<RedisClient>();
            var res = aaa.GetDatabase(3);
            Assert.IsTrue(res.Set("asdasd", "mkmkmkmk", 200)); 

            Assert.IsTrue(res.Set("asdasd", () => JsonSerializer.Serialize<Demo>(new Demo { Name = "123" }), 200)); // extension method 
        }

        class Demo
        {
            public string Name { get; set; }            
        }


        [Test]
        public void PubAndSub()
        {
            var client1 = _serviceProvider.GetRequiredService<RedisClient>();
            client1.Sub.Publish("demo", ()=>JsonSerializer.Serialize(new { a="12321"}));


            var client2 = _serviceProvider.GetRequiredService<RedisClient>();
            client2.Sub.Subscribe("demo", (cjennel, message) => {
                var model = JsonSerializer.Deserialize<dynamic>(message);
                Assert.AreEqual("12321", model.a);
            });
        }


        [Test]
        public void Lock()
        {
            var aaa = _serviceProvider.GetRequiredService<RedisClient>();
            Assert.IsTrue(aaa.Db.Lock("admin_lock",Environment.MachineName,100));
        }


        [Test]
        public void UnLock()
        {
            var aaa = _serviceProvider.GetRequiredService<RedisClient>();
            Assert.IsTrue(aaa.Db.UnLock("admin_lock", Environment.MachineName));
        }
    }
}
