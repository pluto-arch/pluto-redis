using System;
using System.Threading.Tasks;

#if NETCOREAPP
using Microsoft.Extensions.Options;
#endif


using Pluto.Redis.Options;
using StackExchange.Redis;

namespace Pluto.Redis
{
    public class RedisClient : IDisposable,IAsyncDisposable
    {
        private Lazy<ConnectionMultiplexer> connectionLazy = new Lazy<ConnectionMultiplexer>();
        
        private readonly RedisClientOption _options;
        private bool disposedValue;

#if NETCOREAPP
        /// <summary>
        /// 初始化 <see cref="RedisClient"/> 类的新实例。
        /// </summary>
        public RedisClient(IOptions<RedisClientOption> options)
        {
            _options = options.Value;
            if (string.IsNullOrEmpty(_options.InstanceName))
            {
                throw new Exception("client name can not be null");
            }
            _ = _options.RedisAddress ?? throw new Exception("redis connect address can not be null");
            InitConnection();
        }
#else
        /// <summary>
        /// 初始化 <see cref="RedisClient"/> 类的新实例。
        /// </summary>
        public RedisClient(RedisClientOption options)
        {
            _options = options;
            if (string.IsNullOrEmpty(_options.InstanceName))
            {
                throw new Exception("client name can not be null");
            }
            _ = _options.RedisAddress ?? throw new Exception("redis connect address can not be null");
            InitConnection();
        }
#endif
        

        void InitConnection()
        {
            connectionLazy = new Lazy<ConnectionMultiplexer>(GetConnection);
        }
        ConnectionMultiplexer GetConnection()
        {
            var option = new ConfigurationOptions();
            foreach (var item in _options.RedisAddress)
            {
                option.EndPoints.Add(item.Key,item.Value);
            }

            option.CommandMap = _options.CommandMap??CommandMap.Default;
            option.AbortOnConnectFail = false;
            option.Password = _options.Password;
            option.AllowAdmin = _options.AllowAdmin;
            option.KeepAlive = _options.KeepAlive;
            option.AbortOnConnectFail = false;
            option.ClientName = _options.InstanceName;
            option.SyncTimeout = _options.SyncTimeout;
            option.ConnectTimeout = _options.ConnectTimeout;
            option.DefaultVersion = _options.Version??new Version(3, 0);
            option.ServiceName = _options.MasterName;
            return ConnectionMultiplexer.Connect(option);
        }


        public IDatabase Db => GetDatabase();

        public ISubscriber Sub => connectionLazy.Value.GetSubscriber();


        public IDatabase this[int index]
        {
            get
            {
                if (index is < 0 or > 16)
                {
                    throw new IndexOutOfRangeException(nameof(index));
                }

                return connectionLazy.Value.GetDatabase(index);
            }
        }


        /// <summary>
        /// 获取redis db
        /// </summary>
        /// <returns></returns>
        public IDatabase GetDatabase()
        {
            return connectionLazy.Value.GetDatabase(_options.DefaultDataBase);
        }

        /// <summary>
        /// 获取redis db
        /// </summary>
        /// <returns></returns>
        public IDatabase GetDatabase(int dbNumber)
        {
            return connectionLazy.Value.GetDatabase(dbNumber);
        }

        
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    connectionLazy.Value?.Close();
                    connectionLazy.Value?.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        #region dispose

        
        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~PlutoRedisClient()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            this.Dispose(true);
            return ValueTask.CompletedTask;
        }

        #endregion

    }
}
