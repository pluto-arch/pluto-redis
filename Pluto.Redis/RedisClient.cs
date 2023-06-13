using System;
using System.Threading.Tasks;

#if !NET461
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Options;
#endif


using Pluto.Redis.Options;
using StackExchange.Redis;

namespace Pluto.Redis
{
#if !NET461
     public class RedisClient : IDisposable,IAsyncDisposable
#else
    public class RedisClient : IDisposable
#endif
    {
        private Lazy<ConnectionMultiplexer> connectionLazy = new Lazy<ConnectionMultiplexer>();
        
        private readonly ConfigurationOptions _options;
        private bool disposedValue;

        /// <summary>
        /// 初始化 <see cref="RedisClient"/> 类的新实例。
        /// </summary>
        public RedisClient(
#if !NET461
            IOptions<ConfigurationOptions> options
#else
            ConfigurationOptions options
#endif         
            )
        {
            
#if NETCOREAPP
             _options = options.Value;
             _ = options.Value ?? throw new ArgumentNullException("options can not be null");
#else
            _options = options;
            _ = options ?? throw new ArgumentNullException("options can not be null");
#endif 
            InitConnection();
        }
        

        void InitConnection()
        {
            connectionLazy = new Lazy<ConnectionMultiplexer>(GetConnection);
        }
        ConnectionMultiplexer GetConnection()
        {
            return ConnectionMultiplexer.Connect(_options);
        }


        public IDatabase Db => GetDatabase();

        public ISubscriber Sub => connectionLazy.Value.GetSubscriber();


        public IDatabase this[int index]
        {
            get
            {
                if (index < 0 || index > 16)
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
            return connectionLazy.Value.GetDatabase(_options.DefaultDatabase??0);
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

#if NETCOREAPP
        public ValueTask DisposeAsync()
        {
            this.Dispose(true);
            return ValueTask.CompletedTask;
        }
#endif
       

        #endregion

    }
}
