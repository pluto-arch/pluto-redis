﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

using Pluto.Redis.Interfaces;
using Pluto.Redis.Options;
using StackExchange.Redis;

namespace Pluto.Redis
{
    public class PlutoRedisClient : IPlutoRedisClient, IDisposable
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private ConcurrentDictionary<string, ConnectionMultiplexer> _connections;
        private readonly IDatabase _database;
        /// <summary>
        /// 选项
        /// </summary>
        private readonly RedisClientOption _options;
        private string _instanceName;
        private string _password;
        private string _redisMasterName;
        private int _keepAlive;
        private bool _allowAdmin;
        private Dictionary<int, string> _addressDic;

        /// <summary>
        /// 初始化 <see cref="RedisClient"/> 类的新实例。
        /// </summary>
        public PlutoRedisClient(IOptions<RedisClientOption> options)
        {
            _options = options.Value;
            if (string.IsNullOrEmpty(_options.InstanceName))
            {
                throw new Exception("client name can not be null");
            }
            _instanceName = _options.InstanceName;
            _keepAlive = _options.KeepAlive;
            _allowAdmin = _options.AllowAdmin;
            _connections ??= new ConcurrentDictionary<string, ConnectionMultiplexer>();
            _password = _options.Password;
            _redisMasterName = _options.IsSentinelModel ? _options.MasterName ?? throw new Exception("redis master name can not be null") : _options.MasterName;
            _addressDic = _options.RedisAddress ?? throw new Exception("redis connect address can not be null");
            var redisConnection = GetConnect();
            _connectionMultiplexer = redisConnection ?? throw new Exception("redis connection failed");
            _database = redisConnection.GetDatabase(_options.DefaultDbNumber);
        }

        /// <summary>
        /// 获取ConnectionMultiplexer
        /// </summary>
        /// <returns></returns>
        private ConnectionMultiplexer GetConnect()
        {
            return _connections.GetOrAdd(_instanceName, p => GetConnection());
        }
        private ConnectionMultiplexer GetConnection()
        {
            var option = new ConfigurationOptions();
            foreach (var item in _addressDic)
            {
                option.EndPoints.Add(item.Value, item.Key);
            }
            option.CommandMap = _options.IsSentinelModel ? CommandMap.Sentinel : CommandMap.Default;
            option.AbortOnConnectFail = false;
            option.Password = _password;
            option.AllowAdmin = _allowAdmin;
            option.KeepAlive = _keepAlive;
            option.AbortOnConnectFail = false;
            option.ClientName = _instanceName;
            if (!_options.IsSentinelModel)
            {
                return ConnectionMultiplexer.Connect(option);
            }
            option.TieBreaker = "";
            option.DefaultVersion = new Version(3, 0);
            var conn = ConnectionMultiplexer.Connect(option);
            var redisServiceOptions = new ConfigurationOptions();
            redisServiceOptions.ServiceName = _redisMasterName;
            redisServiceOptions.Password = _password;
            redisServiceOptions.AllowAdmin = _allowAdmin;
            redisServiceOptions.KeepAlive = _keepAlive;
            redisServiceOptions.AbortOnConnectFail = false;
            return conn.GetSentinelMasterConnection(redisServiceOptions);
        }

        /// <summary>
        /// 获取redis db
        /// </summary>
        /// <returns></returns>
        public IDatabase GetDatabase()
        {
            return _database;
        }

        /// <summary>
        /// 获取redis db
        /// </summary>
        /// <returns></returns>
        public IDatabase GetDatabase(int dbNumber)
        {
            return _connectionMultiplexer.GetDatabase(dbNumber);
        }

        #region 同步方法

        /// <summary>
        /// 添加一个字符串对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        public bool Set(string key, string value, TimeSpan? expiry = null)
        {
            return _database.StringSet(key, value, expiry);
        }

        /// <summary>
        /// 添加一个字符串对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        /// <param name="seconds">过期时间（秒）。</param>
        /// <returns>返回是否执行成功。</returns>
        public bool Set(string key, string value, int seconds)
        {
            return _database.StringSet(key, value, TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// 添加一个对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <typeparam name="T">对象的类型。</typeparam>
        /// <param name="value">值。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        public bool Set(string key, Func<string> serializaFun, TimeSpan? expiry = null)
        {
            return _database.StringSet(key, serializaFun.Invoke(), expiry);
        }

        /// <summary>
        /// 添加一个对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <typeparam name="T">对象的类型。</typeparam>
        /// <param name="value">值。</param>
        /// <param name="seconds">过期时间（秒）。</param>
        /// <returns>返回是否执行成功。</returns>
        /// <example>
        /// client.Set<User>("key",()=>JsonConvert.Serialize<User>(new User()))
        /// </example>
        public bool Set(string key, Func<string> serializaFun, int seconds)
        {
            return _database.StringSet(key, serializaFun.Invoke(), TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// 获取一个对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        /// <example>
        /// client.Get<User>("key",(strValue)=>JsonConvert.Deserialize<User>(strValue))
        /// </example>
        public T Get<T>(string key, Func<string,T> callBack)
        {
            return callBack(_database.StringGet(key));
        }

        /// <summary>
        /// 获取一个字符串对象。
        /// </summary>
        /// <param name="key">值。</param>
        /// <returns>返回对象的值。</returns>
        public string Get(string key)
        {
            return _database.StringGet(key);
        }

        /// <summary>
        /// 删除一个对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <returns>返回是否执行成功。</returns>
        public bool Delete(string key)
        {
            return _database.KeyDelete(key);
        }

        /// <summary>
        /// 返回键是否存在。
        /// </summary>
        /// <param name="key">键。</param>
        /// <returns>返回键是否存在。</returns>
        public bool Exists(string key)
        {
            return _database.KeyExists(key);
        }

        /// <summary>
        /// 设置一个键的过期时间。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        public bool SetExpire(string key, TimeSpan? expiry)
        {
            return _database.KeyExpire(key, expiry);
        }

        /// <summary>
        /// 设置一个键的过期时间。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="seconds">过期时间（秒）。</param>
        /// <returns>返回是否执行成功。</returns>
        public bool SetExpire(string key, int seconds)
        {
            return _database.KeyExpire(key, TimeSpan.FromSeconds(seconds));
        }

        #endregion

        #region 异步方法

        /// <summary>
        /// 异步添加一个字符串对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        public async Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            return await _database.StringSetAsync(key, value, expiry);
        }

        /// <summary>
        /// 异步添加一个字符串对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        /// <param name="seconds">过期时间（秒）。</param>
        /// <returns>返回是否执行成功。</returns>
        public async Task<bool> SetAsync(string key, string value, int seconds)
        {
            return await _database.StringSetAsync(key, value, TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// 异步添加一个对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <typeparam name="T">对象的类型。</typeparam>
        /// <param name="value">值。</param>
        /// <returns>返回是否执行成功。</returns>
        /// <example>
        /// client.SetAsync<User>("key",()=>JsonConvert.Serialize<User>(new User()))
        /// </example>
        public async Task<bool> SetAsync(string key, Func<string> serializeFunc, TimeSpan? expiry = null)
        {
            return await _database.StringSetAsync(key, serializeFunc.Invoke(), expiry);
        }

        /// <summary>
        /// 异步添加一个对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <typeparam name="T">对象的类型。</typeparam>
        /// <param name="value">值。</param>
        /// <returns>返回是否执行成功。</returns>
        /// <example>
        /// client.SetAsync<User>("key",()=>JsonConvert.Serialize<User>(new User()))
        /// </example>
        public async Task<bool> SetAsync(string key, Func<string> serializeFunc, int seconds)
        {
            return await _database.StringSetAsync(key, serializeFunc.Invoke(), TimeSpan.FromSeconds(seconds));
        }


        /// <summary>
        /// 异步获取一个对象。
        /// </summary>
        /// <typeparam name="T">对象的类型。</typeparam>
        /// <param name="key">值。</param>
        /// <returns>返回对象的值。</returns>
        /// <example>
        /// client.GetAsync<User>("key",(strValue)=>JsonConvert.Deserialize<User>(strValue))
        /// </example>
        public async Task<T> GetAsync<T>(string key,Func<string,T> callback)
        {
            return callback(await _database.StringGetAsync(key));
        }

        /// <summary>
        /// 异步获取一个字符串对象。
        /// </summary>
        /// <param name="key">值。</param>
        /// <returns>返回对象的值。</returns>
        public async Task<string> GetAsync(string key)
        {
            return await _database.StringGetAsync(key);
        }

        /// <summary>
        /// 异步删除一个对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <returns>返回是否执行成功。</returns>
        public async Task<bool> DeleteAsync(string key)
        {
            return await _database.KeyDeleteAsync(key);
        }

        /// <summary>
        /// 异步设置一个键的过期时间。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="seconds">过期时间（秒）。</param>
        /// <returns>返回是否执行成功。</returns>
        public async Task<bool> SetExpireAsync(string key, int seconds)
        {
            return await _database.KeyExpireAsync(key, TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// 异步设置一个键的过期时间。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        public async Task<bool> SetExpireAsync(string key, TimeSpan? expiry)
        {
            return await _database.KeyExpireAsync(key, expiry);
        }

        #endregion

        #region 锁相关操作

        /// <summary>
        /// 获取锁。
        /// </summary>
        /// <param name="key">锁名称。</param>
        /// <param name="seconds">过期时间（秒）。</param>
        /// <returns>是否已锁。</returns>
        public bool Lock(string key,string token, int seconds)
        {
            return _database.LockTake(key, token, TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// 释放锁。
        /// </summary>
        /// <param name="key">锁名称。</param>
        /// <returns>是否成功。</returns>
        public bool UnLock(string key,string token)
        {
            return _database.LockRelease(key, token);
        }

        /// <summary>
        /// 异步获取锁。
        /// </summary>
        /// <param name="key">锁名称。</param>
        /// <param name="seconds">过期时间（秒）。</param>
        /// <returns>是否成功。</returns>
        public async Task<bool> LockAsync(string key, string token, int seconds)
        {
            return await _database.LockTakeAsync(key, token, TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// 异步释放锁。
        /// </summary>
        /// <param name="key">锁名称。</param>
        /// <returns>是否成功。</returns>
        public async Task<bool> UnLockAsync(string key, string token)
        {
            return await _database.LockReleaseAsync(key, token);
        }

        #endregion

        #region 发布订阅
        /// <summary>
        /// 获取发布者
        /// </summary>
        /// <returns></returns>
        public ISubscriber GetPublisher()
        {
            return _connectionMultiplexer.GetSubscriber();
        }

        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public long Publish(string channel, Func<string> serializaFun)
        {
            var sub = this.GetPublisher();
            return sub.Publish(channel, serializaFun.Invoke());
        }

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="subChannel"></param>
        /// <param name="callback"></param>
        public void Subscribe(string subChannel, Action<RedisChannel,string> callback)
        {
            var sub = this.GetPublisher();
            sub.Subscribe(subChannel, (channel, message) =>
            {
                callback(channel, message);
            });
        }
        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="channel"></param>
        public void Unsubscribe(string channel)
        {
            var sub = this.GetPublisher();
            sub.Unsubscribe(channel);
        }
        /// <summary>
        /// 取消所有订阅
        /// </summary>
        public void UnsubscribeAll()
        {
            var sub = this.GetPublisher();
            sub.UnsubscribeAll();
        }
        #endregion

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            if (_connections != null && _connections.Count > 0)
            {
                foreach (var item in _connections.Values)
                {
                    item.Close();
                }
            }
        }
    }
}
