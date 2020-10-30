﻿using System;
using System.Threading.Tasks;

using Newtonsoft.Json;

using StackExchange.Redis;

namespace Pluto.Redis.Extensions
{
    public static class RedisDatabaseExtensions
    {
        #region 同步
        /// <summary>
        /// 添加一个字符串对象。
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        public static bool Set(this IDatabase db, string key, string value, TimeSpan? expiry = null)
        {
            return db.StringSet(key, value, expiry);
        }

        /// <summary>
        /// 添加一个字符串对象。
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        /// <param name="seconds">过期时间（秒）。</param>
        /// <returns>返回是否执行成功。</returns>
        public static bool Set(this IDatabase db, string key, string value, int seconds)
        {
            TimeSpan expiry = TimeSpan.FromSeconds(seconds);
            return db.StringSet(key, value, expiry);
        }

        /// <summary>
        /// 添加一个对象。
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key">键。</param>
        /// <typeparam name="T">对象的类型。</typeparam>
        /// <param name="value">值。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        public static bool Set<T>(this IDatabase db, string key, T value, TimeSpan? expiry = null)
        {
            var data = JsonConvert.SerializeObject(value);
            return db.StringSet(key, data, expiry);
        }


        /// <summary>
        /// 添加一个对象。
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key">键。</param>
        /// <typeparam name="T">对象的类型。</typeparam>
        /// <param name="value">值。</param>
        /// <param name="seconds">过期时间（秒）。</param>
        /// <returns>返回是否执行成功。</returns>
        public static bool Set<T>(this IDatabase db, string key, T value, int seconds)
        {
            TimeSpan expiry = TimeSpan.FromSeconds(seconds);
            var data = JsonConvert.SerializeObject(value);
            return db.StringSet(key, data, expiry);
        }

        /// <summary>
        /// 获取一个对象。
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key">值。</param>
        /// <returns>返回对象的值。</returns>
        public static T Get<T>(this IDatabase db, string key)
        {
            string json = db.StringGet(key);
            if (string.IsNullOrWhiteSpace(json))
            {
                return default(T);
            }
            T entity = JsonConvert.DeserializeObject<T>(json);
            return entity;
        }

        /// <summary>
        /// 获取一个字符串对象。
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key">值。</param>
        /// <returns>返回对象的值。</returns>
        public static string Get(this IDatabase db, string key)
        {
            return db.StringGet(key);
        }

        /// <summary>
        /// 删除一个对象。
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key">键。</param>
        /// <returns>返回是否执行成功。</returns>
        public static bool Delete(this IDatabase db, string key)
        {
            return db.KeyDelete(key);
        }

        /// <summary>
        /// 返回键是否存在。
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key">键。</param>
        /// <returns>返回键是否存在。</returns>
        public static bool Exists(this IDatabase db, string key)
        {
            return db.KeyExists(key);
        }

        /// <summary>
        /// 设置一个键的过期时间。
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key">键。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        public static bool SetExpire(this IDatabase db, string key, TimeSpan? expiry)
        {
            return db.KeyExpire(key, expiry);
        }

        /// <summary>
        /// 设置一个键的过期时间。
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key">键。</param>
        /// <param name="seconds">过期时间（秒）。</param>
        /// <returns>返回是否执行成功。</returns>
        public static bool SetExpire(this IDatabase db, string key, int seconds)
        {
            TimeSpan expiry = TimeSpan.FromSeconds(seconds);
            return db.KeyExpire(key, expiry);
        }


        #endregion

        #region 异步

        /// <summary>
        /// 异步添加一个字符串对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        public static async Task<bool> SetAsync(this IDatabase db, string key, string value, TimeSpan? expiry = null)
        {
            return await db.StringSetAsync(key, value, expiry);
        }

        /// <summary>
        /// 异步添加一个字符串对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        /// <param name="seconds">过期时间（秒）。</param>
        /// <returns>返回是否执行成功。</returns>
        public static async Task<bool> SetAsync(this IDatabase db, string key, string value, int seconds)
        {
            TimeSpan expiry = TimeSpan.FromSeconds(seconds);
            return await db.StringSetAsync(key, value, expiry);
        }

        /// <summary>
        /// 异步添加一个对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <typeparam name="T">对象的类型。</typeparam>
        /// <param name="value">值。</param>
        /// <returns>返回是否执行成功。</returns>
        public static async Task<bool> SetAsync<T>(this IDatabase db, string key, T value)
        {
            var data = JsonConvert.SerializeObject(value);
            return await db.StringSetAsync(key, data);
        }

        /// <summary>
        /// 异步获取一个对象。
        /// </summary>
        /// <typeparam name="T">对象的类型。</typeparam>
        /// <param name="key">值。</param>
        /// <returns>返回对象的值。</returns>
        public static async Task<T> GetAsync<T>(this IDatabase db, string key)
        {
            string json = await db.StringGetAsync(key);
            if (string.IsNullOrWhiteSpace(json))
            {
                return default(T);
            }
            T entity = JsonConvert.DeserializeObject<T>(json);
            return entity;
        }

        /// <summary>
        /// 异步获取一个字符串对象。
        /// </summary>
        /// <param name="key">值。</param>
        /// <returns>返回对象的值。</returns>
        public static async Task<string> GetAsync(this IDatabase db, string key)
        {
            return await db.StringGetAsync(key);
        }

        /// <summary>
        /// 异步删除一个对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <returns>返回是否执行成功。</returns>
        public static async Task<bool> DeleteAsync(this IDatabase db, string key)
        {
            return await db.KeyDeleteAsync(key);
        }

        /// <summary>
        /// 异步设置一个键的过期时间。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="seconds">过期时间（秒）。</param>
        /// <returns>返回是否执行成功。</returns>
        public static async Task<bool> SetExpireAsync(this IDatabase db, string key, int seconds)
        {
            TimeSpan expiry = TimeSpan.FromSeconds(seconds);
            return await db.KeyExpireAsync(key, expiry);
        }

        /// <summary>
        /// 异步设置一个键的过期时间。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        public static async Task<bool> SetExpireAsync(this IDatabase db, string key, TimeSpan? expiry)
        {
            return await db.KeyExpireAsync(key, expiry);
        }

        #endregion

        #region 锁相关操作

        /// <summary>
        /// 分布式锁 Token。
        /// </summary>
        private static readonly RedisValue LockToken = Environment.MachineName;

        /// <summary>
        /// 获取锁。
        /// </summary>
        /// <param name="key">锁名称。</param>
        /// <param name="seconds">过期时间（秒）。</param>
        /// <returns>是否已锁。</returns>
        public static bool Lock(this IDatabase db, string key, int seconds)
        {
            return db.LockTake(key, LockToken, TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// 释放锁。
        /// </summary>
        /// <param name="key">锁名称。</param>
        /// <returns>是否成功。</returns>
        public static bool UnLock(this IDatabase db, string key)
        {
            return db.LockRelease(key, LockToken);
        }

        /// <summary>
        /// 异步获取锁。
        /// </summary>
        /// <param name="key">锁名称。</param>
        /// <param name="seconds">过期时间（秒）。</param>
        /// <returns>是否成功。</returns>
        public static async Task<bool> LockAsync(this IDatabase db, string key, int seconds)
        {
            return await db.LockTakeAsync(key, LockToken, TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// 异步释放锁。
        /// </summary>
        /// <param name="key">锁名称。</param>
        /// <returns>是否成功。</returns>
        public static async Task<bool> UnLockAsync(this IDatabase db, string key)
        {
            return await db.LockReleaseAsync(key, LockToken);
        }

        #endregion
    }
}