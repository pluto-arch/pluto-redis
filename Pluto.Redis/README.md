```
builder.Services.AddRedisClient(o =>
                                  {
                                      o.CommandMap = CommandMap.Default;
                                      o.DefaultDatabase = 0;
                                      o.ServiceName = "docker";
                                      o.Password = "";
                                      o.KeepAlive = 180;
                                      o.EndPoints.Add("localhost",6379);
                                  });
```