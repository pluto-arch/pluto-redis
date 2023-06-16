using Microsoft.AspNetCore.Mvc;
using Pluto.Redis;
using Pluto.Redis.Extensions;
using StackExchange.Redis;
using WebTest;
using WebTest.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<DemoService>();


builder.Services.AddRedisClient(o =>
{
    o.CommandMap = CommandMap.Default;
    o.DefaultDatabase = 0;
    o.ClientName = "docker01";
    o.DefaultDatabase = 0;
    o.Password = "";
    o.KeepAlive = 180;
    o.EndPoints.Add("localhost",6379);
});

builder.Services.AddRedisClient(o =>
{
    o.CommandMap = CommandMap.Default;
    o.DefaultDatabase = 0;
    o.ClientName = "docker02";
    o.DefaultDatabase = 1;
    o.Password = "";
    o.KeepAlive = 180;
    o.EndPoints.Add("localhost",6379);
});

builder.Services.AddRedisClientFactory();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/users", async ([FromServices]RedisClientFactory redisClientFactory) =>
{
    var redis01 = redisClientFactory["docker01"];
    await redis01.Db.StringSetAsync("demoA","123123",TimeSpan.FromMinutes(3));
    return Results.Ok("demoA");

}).WithName("redsi01");


app.MapGet("/user", async ([FromServices]RedisClientFactory redisClientFactory) =>
{
    var redis01 = redisClientFactory["docker02"];
    await redis01.Db.StringSetAsync("demoB","123123",TimeSpan.FromMinutes(3));
    return Results.Ok("demoB");

}).WithName("redsi02");

app.Run();
