﻿using System.Net;
using Orleans;
using Orleans.Runtime;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Text.Json.Serialization;
using Orleans.Concurrency;

var builder = WebApplication.CreateBuilder();
builder.Services.AddHttpClient();
builder.Logging.SetMinimumLevel(LogLevel.Information).AddConsole();
builder.Host.UseOrleans(builder => {
    builder
        .UseLocalhostClustering()
        .UseInMemoryReminderService()
        .Configure<ClusterOptions>(opt => {
            opt.ClusterId = "dev";
            opt.ServiceId = "http-client";
        })
        .Configure<EndpointOptions>(opt => opt.AdvertisedIPAddress = IPAddress.Loopback)
        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(TimeKeeperGrain).Assembly).WithReferences());
});

var app = builder.Build();

app.MapGet("/", async ctx => {
    var client = ctx.RequestServices.GetService<IGrainFactory>()!;
    string timezone = "Asia/Indonesia";
    var grain = client.GetGrain<ITimeKeeper>(timezone)!;
    var localTime = await grain.GetCurrentTime(timezone);
    await ctx.Response.WriteAsync(@"<html><head><link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/uikit@3.5.5/dist/css/uikit.min.css"" /></head>");
    await ctx.Response.WriteAsync("<body>");
    await ctx.Response.WriteAsync($"Local time in {localTime.timeZone} is {localTime.dateTime}");
    await ctx.Response.WriteAsync("</body></html>");
});

app.Run();
public interface ITimeKeeper : IGrainWithStringKey
{
    Task<(DateTimeOffset dateTime, string timeZone)> GetCurrentTime(string timeZone);
}

public class WorldTime
{
    [JsonPropertyName("datetime")]
    public DateTimeOffset DateTime { get; set; }
}

[StatelessWorker]
public class TimeKeeperGrain : Grain, ITimeKeeper
{
    private readonly ILogger _log;
    private readonly IHttpClientFactory _httpFactory;
    public TimeKeeperGrain(
        ILogger<TimeKeeperGrain> log,
        IHttpClientFactory httpFactory
    )
    {
        _log = log;
        _httpFactory = httpFactory;
    }
    public async Task<(DateTimeOffset dateTime, string timeZone)> GetCurrentTime(string timeZone)
    {
        var client = _httpFactory.CreateClient();

        var result = await client.GetAsync($"http://worldtimeapi.org/api/timezone/{timeZone}");
        var worldClock = await result.Content.ReadFromJsonAsync<WorldTime>();
        return (worldClock!.DateTime, timeZone);
    }
}