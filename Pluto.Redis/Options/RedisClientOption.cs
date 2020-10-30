using System.Collections.Generic;
using System.Net;
using StackExchange.Redis;

namespace Pluto.Redis.Options
{
    public class RedisClientOption
    {
        public bool IsSentinelModel { get; set; }

        public string InstanceName { get; set; }

        public int DefaultDbNumber { get; set; }

        public string Password { get; set; }

        public string MasterName { get; set; }

        public int KeepAlive { get; set; } = 180;

        public bool AllowAdmin { get; set; }

        public Dictionary<int, string> RedisAddress { get; set; }
    }
}