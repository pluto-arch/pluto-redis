using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pluto.Redis;
using StackExchange.Redis;

namespace FrameworkTest
{
    internal class Program
    {
        private static Lazy<IRedisClient> Redis = new Lazy<IRedisClient>(InitRedis);

        private static IRedisClient InitRedis()
        {
            var opt = new ConfigurationOptions
            {
                CommandMap = CommandMap.Default,
                DefaultDatabase = 0,
                ClientName = "docker02",
                Password = "",
                KeepAlive = 180,
            };
            opt.EndPoints.Add("localhost", 6379);
            IRedisClient redis = new RedisClient(opt);
            return redis;
        }

        static async Task Main(string[] args)
        {
            var a =await  Redis.Value.Db.StringSetAsync("","",TimeSpan.Zero);
        }
    }
}
