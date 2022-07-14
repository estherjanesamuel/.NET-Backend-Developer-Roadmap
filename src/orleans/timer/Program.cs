using System.Net;
using Orleans;
using Orleans.Runtime;
using Orleans.Configuration;
using Orleans.Hosting;

var builder = WebApplication.CreateBuilder();
builder.Logging.SetMinimumLevel(LogLevel.Information).AddConsole();
builder.Host.UseOrleans(builder => {
    builder
        .UseLocalhostClustering()
        .Configure<ClusterOptions>(opt => {
            opt.ClusterId = "dev";
            opt.ServiceId = "HelloWorldApp";
        })
        .Configure<EndpointOptions>(opt => opt.AdvertisedIPAddress = IPAddress.Loopback)
        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloTimerGrain).Assembly).WithReferences())
        .AddRedisGrainStorage("redis-timer", optBuilder => optBuilder.Configure(opt => {
            opt.ConnectionString = "192.168.1.5:6379";
            opt.UseJson = true;
            opt.DatabaseNumber = 1;
        }));
});

var app = builder.Build();

app.MapGet("/", async ctx => {
    var client = ctx.RequestServices.GetService<IGrainFactory>()!;
    var grain = client.GetGrain<IHelloArchive>(0)!;
    await grain.SayHello("Hello Ariefs " + new Random().Next(1,9));
    var greetings = await grain.GetGreetings();
    await  ctx.Response.WriteAsync(@"<html> <head> <link rel=""stylesheet"" href=""https://cdn.jsdeliver.net/npm/uikit@3.5.5/dist/css/uikit.min.css"" /> </head> ");
    await  ctx.Response.WriteAsync("<body>");
    await  ctx.Response.WriteAsync("Refresh yout browser. There's a timer that keeps adding message every 5 seconds. <br> ");
    await  ctx.Response.WriteAsync("<ul>");
    foreach (var greeting in greetings)
    {
        await  ctx.Response.WriteAsync($"<li>{greeting.Message} added at {greeting.TimeStampUtc}</li> ");
    }
    await  ctx.Response.WriteAsync("</ul> ");
    await  ctx.Response.WriteAsync("</body></html> ");
});

app.Run();


public interface IHelloArchive : IGrainWithIntegerKey
{
    Task SayHello(string greeting);
    Task<IEnumerable<Greeting>> GetGreetings();
}


public record Greeting(string Message, DateTime TimeStampUtc);

public class HelloTimerGrain : Grain, IHelloArchive
{
    private readonly IPersistentState<GreetingArchive> _archive;
    private readonly ILogger _log;
    private string _greeting = "hello world";
    private IDisposable? _timerDisposable;

    public HelloTimerGrain(
        [PersistentState("archive", "redis-timer")]
        IPersistentState<GreetingArchive> archive,
        ILogger<HelloTimerGrain> log
    )
    {
        _archive = archive;
        _log = log;
    }

    public override Task OnActivateAsync()
    {
        _timerDisposable = RegisterTimer(async (object data) => {
            var archive = data as IPersistentState<GreetingArchive>;
            var g = new Greeting(_greeting, DateTime.UtcNow);
            archive!.State.Greetings.Insert(0,g);
            await archive!.WriteStateAsync();
            _log.Info($"`{g.Message}` added at {g.TimeStampUtc}");
        },_archive, TimeSpan.FromSeconds(1),TimeSpan.FromSeconds(5));
        return Task.CompletedTask;
    }

    public override Task OnDeactivateAsync()
    {
        _timerDisposable?.Dispose();
        return Task.CompletedTask;
    }
    public Task<IEnumerable<Greeting>> GetGreetings() => Task.FromResult<IEnumerable<Greeting>>(_archive.State.Greetings);

    public Task SayHello(string greeting)
    {
        _greeting = greeting;
        return Task.CompletedTask;
    }
}

public class GreetingArchive
{
    public List<Greeting> Greetings { get;  } = new List<Greeting>();
}