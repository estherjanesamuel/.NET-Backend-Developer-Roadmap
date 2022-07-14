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
        .UseInMemoryReminderService()
        .Configure<ClusterOptions>(opt => {
            opt.ServiceId = "HelloWorldApp";
            opt.ClusterId = "dev";
        })
        .Configure<EndpointOptions>(opt => opt.AdvertisedIPAddress = IPAddress.Loopback)
        .AddRedisGrainStorage("redis-reminder", optBuilder => optBuilder.Configure(opt => {
            opt.ConnectionString ="localhost:6379";
            opt.UseJson = true;
            opt.DatabaseNumber = 1;
        }));
});

var app = builder.Build();

app.MapGet("/", async ctx => {
    var client = ctx.RequestServices.GetService<IGrainFactory>()!;
    var grain = client.GetGrain<IHelloArchive>(0)!;
   await grain.SayHello("Hello world " + new Random().Next());
   var res = await grain.GetGreetings();
    await ctx.Response.WriteAsync(@"<html><head><link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/uikit@3.5.5/dist/css/uikit.min.css""></head>");
    await ctx.Response.WriteAsync("<body>");
    await ctx.Response.WriteAsync(@"<a href=""set-reminder"">Set Reminder</a> - <a href=""remove-reminder"">Remove reminder</a><br/>");
    await ctx.Response.WriteAsync("<ul>");
    foreach (var greeting in res)
    {
        await ctx.Response.WriteAsync($"<li>{greeting.Message} at {greeting.TimeStampUtc}</li>");
    }
    await ctx.Response.WriteAsync("</ul>");
    await ctx.Response.WriteAsync("</body></html>");
});
// WARNING - changing state using GET is a terrible terrible practice. I use it here because this is a sample and I am lazy. Don't follow my bad example.
app.MapGet("/set-reminder", async ctx => {
    var client = ctx.RequestServices.GetService<IGrainFactory>()!;
    var grain = client.GetGrain<IHelloArchive>(0)!;
    await grain.AddReminder("repeat-hell0", repeatEvery: TimeSpan.FromMinutes(1));
    ctx.Response.Redirect("/");
});
// WARNING - changing state using GET is a terrible terrible practice. I use it here because this is a sample and I am lazy. Don't follow my bad example.
app.MapGet("/remove-reminder", async ctx => {
    var client = ctx.RequestServices.GetService<IGrainFactory>()!;
    var grain = client.GetGrain<IHelloArchive>(0)!;

    await grain.RemoveReminder("repeat-hello");
    ctx.Response.Redirect("/");
});
app.Run();
public interface IHelloArchive : IGrainWithIntegerKey
{
    Task AddReminder(string reminder, TimeSpan repeatEvery);
    Task RemoveReminder(string reminder);
    Task SayHello(string greeting);
    Task<IEnumerable<Greeting>> GetGreetings();
}

public record Greeting(string Message, DateTime TimeStampUtc);

public class HelloReminderGrain : Grain, IHelloArchive, IRemindable
{
    private readonly IPersistentState<GreetingArchive> _archive;
    private readonly ILogger _log;
    private string _greeting = "hello world";

    public HelloReminderGrain(
        [PersistentState("archive", "redis-reminder")]
        IPersistentState<GreetingArchive> archive,
        ILogger<HelloReminderGrain> log
    )
    {
        _archive = archive;
        _log = log;
    }
    public async Task AddReminder(string reminder, TimeSpan repeatEvery)
    {
        if (string.IsNullOrWhiteSpace(reminder))
            throw new ArgumentNullException(nameof(reminder));
        
        var r = await GetReminder(reminder);
        if (r is object)
            await RegisterOrUpdateReminder(reminder, TimeSpan.FromSeconds(1), repeatEvery);
    }

    public Task<IEnumerable<Greeting>> GetGreetings() => Task.FromResult<IEnumerable<Greeting>>(_archive.State.Greetings);

    public async Task ReceiveReminder(
        string reminderName, 
        TickStatus status)
    {
        _log.Info($"Receive reminder {reminderName} on {DateTime.UtcNow} with status : {status} ");
        var g = new Greeting(_greeting, DateTime.UtcNow);
        _archive!.State.Greetings.Insert(0,g);
        await _archive.WriteStateAsync();

        _log.Info($"`{g.Message}` added at {g.TimeStampUtc}");
    }

    public async Task RemoveReminder(string reminder)
    {
        if (string.IsNullOrWhiteSpace(reminder))
            throw new ArgumentNullException(nameof(reminder));

        var r = await GetReminder(reminder);

        if(r is object)
            await UnregisterReminder(r);
    }

    public Task SayHello(string greeting)
    {
        _greeting = greeting;
        return Task.CompletedTask;
    }
}

public record GreetingArchive
{
    public List<Greeting> Greetings { get;  } = new List<Greeting>();
}