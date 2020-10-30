# pluto-redis
dotnet core redis 常用操作，基于StackExchange.Redis


## 使用方式
```csharp
services.AddPlutoRedis(o =>
            {
                o.DefaultDbNumber = 0; 
                o.InstanceName = "test";
                o.Password = "pluto1002";
                o.MasterName = "redis-master"; // sentinel 模式的redis master名称
                o.IsSentinelModel = true;
                o.AllowAdmin = true;
                o.KeepAlive = 180;
                o.RedisAddress=new Dictionary<int, string>
                { 
                    // sentinel 或者普通模式 redis的地址
                    {26379,"127.0.0.1"},
                    {26380,"127.0.0.1"},
                    {26381,"127.0.0.1"},
                };
            });
```
- 创建实例
```csharp
var client = _serviceProvider.GetService<IPlutoRedisClient>();
var res= client.Set("demo", "123123qweqwew",3600);
```
- 获取db
```csharp
var client = _serviceProvider.GetService<IPlutoRedisClient>();
var res= client.GetDatabase(3);
// 使用db的扩展方法
res.Set("asdasd","mkmkmkmk",200) // 和client中的基本方法一致。
```
- 分布式锁
```csharp
// 获取锁
var client = _serviceProvider.GetService<IPlutoRedisClient>();
client.Lock("admin_lock",100) // return bool 

// 解锁
client.UnLock("admin_lock")
```
- 发布订阅
```csharp
var client = _serviceProvider.GetService<IPlutoRedisClient>();
// 发布
client.Publish<User>("demo",new User{Id=111111});

// 订阅
client.Subscribe("demo",Action<string> callback);
```



