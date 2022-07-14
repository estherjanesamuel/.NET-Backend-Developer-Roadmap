using System.Net;
using Orleans;
using Orleans.Runtime;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Text.Json.Serialization;
using Orleans.Concurrency;
using System.Collections.Immutable;
using Spectre.Console;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder();
builder.Logging
    .AddFilter("Orleans.Runtime.Management.ManagementGrain", LogLevel.Warning)
    .AddFilter("Orleans.Runtime.SiloControl", LogLevel.Warning);
builder.Host.UseOrleans(builder => {
    builder
        .UseLocalhostClustering()
        .UseInMemoryReminderService()
        .Configure<ClusterOptions>(opt => {
            opt.ClusterId = "dev";
            opt.ServiceId = "http-client";
        })
        .AddMemoryGrainStorage("AccountState")
        .Configure<EndpointOptions>(opt => opt.AdvertisedIPAddress = IPAddress.Loopback)
        .ConfigureApplicationParts(parts => parts
            .AddApplicationPart(typeof(ChirperAccount).Assembly).WithReferences()
            .AddApplicationPart(typeof(IChirperAccount).Assembly).WithReferences())
        .UseDashboard();
});



var app = builder.Build();
app.Run();


public interface IChirperAccount : IChirperPublisher, IChirperSubscriber
{
    Task FollowUserIdAsync(string userNameToFollow);
    Task UnFollowUserIdAsync(string userNameToFollow);
    Task<ImmutableList<string>> GetFollowingListAsync();
    Task<ImmutableList<string>> GetFollowersListAsync();
    Task PublishMessageAsync(string chirpMessage);
    Task<ImmutableList<ChirperMessage>> GetReceivedMessagesAsync(int n = 10, int start = 0);
    Task SubscribeAsync(IChirperViewer viewer);
    Task UnSubscribeAsync(IChirperViewer viewer);
}

public interface IChirperViewer : IGrainObserver
{
    void NewChirp(ChirperMessage message);
    void SubscriptionAdded(string userName);
    void SubscriptionRemoved(string userName);
    void NewFollower(string userName);
}

public interface IChirperSubscriber : IGrainWithStringKey
{
    Task NewChirpAsync(ChirperMessage chirp);
}

public interface IChirperPublisher : IGrainWithStringKey
{
    Task<ImmutableList<ChirperMessage>> GetPublishedMessagesAsync(int n =0, int start = 0);
    Task AddFollowerAsync(string userName, IChirperSubscriber subscriber);

    Task RemoveFollowerAsync(string userName);
}

[Serializable]
public record class ChirperMessage(string Message, DateTimeOffset TimeStamp, string PublisherUserName)
{
    public Guid MessageId { get; } = Guid.NewGuid();

    public override string ToString() => $"Chirp: '{Message}' from @{PublisherUserName} at {TimeStamp}";
}

public class ChirperAccount : Grain, IChirperAccount
{
    private const int ReceivedMessagesCacheSize = 100 ;
    private const int PublishedMessagesCacheSize = 100 ;
    private const int MaxChirpLength = 280 ;
    private readonly HashSet<IChirperViewer> _viewers = new();
    private ILogger _logger;
    private readonly IPersistentState<ChirperAccountState> _state;
    private Task? _outstandingWriteStateOperation;

    public ChirperAccount(
        [PersistentState(stateName:"account", storageName:"AccountState")]
        IPersistentState<ChirperAccountState> state,
        ILogger<ChirperAccount> logger
    )
    {
        _state = state;
        _logger = logger;
    }
    private static string GrainType => nameof(ChirperAccount);
    private string GrainKey => this.GetPrimaryKeyString();

    public override Task OnActivateAsync()
    {
        _logger.LogInformation("{GrainType} {GrainKey} activated.", GrainType,GrainKey);
        return Task.CompletedTask;
    }

    public async Task PublishMessageAsync(string chirpMessage)
    {
        var chirp = CreateNewChirpMessage(chirpMessage);
        _logger.LogInformation("{GrainType} {GrainKey} publishing new chirp message `{Chirp}`.", GrainType, GrainKey,chirp);

        _state.State.MyPublishedMessages.Enqueue(chirp);
        while (_state.State.MyPublishedMessages.Count() > PublishedMessagesCacheSize)
        {
            _state.State.MyPublishedMessages.Dequeue();
        }

        await WriteStateAsync();


        _logger.LogInformation("{GrainType} {GrainKey} sending new chirp message to {ViewerCount} viewers.", GrainType, GrainKey, _viewers.Count);
        _viewers.ForEach(_ => _.NewChirp(chirp));
        _logger.LogInformation("{GrainType} {GrainKey} sending new chirp message to {FollowerCount} viewers.", GrainType, GrainKey,_state.State.Followers.Count);

        await Task.WhenAll(_state.State.Followers.Values.Select(_ => _.NewChirpAsync(chirp)).ToArray());
    }


    public Task<ImmutableList<ChirperMessage>> GetReceivedMessagesAsync(int n , int start )
    {
        if (start < 0) start = 0;
        if (start + n > _state.State.RecentReceivedMessages.Count)
        {
            n = _state.State.RecentReceivedMessages.Count - start;
        }
        return Task.FromResult(_state.State.RecentReceivedMessages.Skip(start).Take(n).ToImmutableList());
    }
   

    public async Task FollowUserIdAsync(string username)
    {
        _logger.LogInformation("{GrainType} {username} > FollowUserName({TargetUserName}).", GrainType,GrainKey,username);
        var userToFollow = GrainFactory.GetGrain<IChirperPublisher>(username);
        await userToFollow.AddFollowerAsync(GrainKey, this.AsReference<IChirperSubscriber>());
        _state.State.Subscriptions[username] = userToFollow;
        await WriteStateAsync();

        // Notify any viewers that a subscription has been added for this user
        _viewers.ForEach(cv => cv.SubscriptionAdded(username));
    }

    public async Task UnFollowUserIdAsync(string username)
    {
        _logger.LogInformation("{GrainType} {username} > UnfollowUserName({TargetUserName}).", GrainType, GrainKey, username);
        
        // Ask the publihser to remove this grain as a follower
        await GrainFactory.GetGrain<IChirperPublisher>(username).RemoveFollowerAsync(GrainKey);

        // Remove this publisher from the subscriptions list
        _state.State.Subscriptions.Remove(username);

        // Save Now
        await WriteStateAsync();

        // Notify event subscribers
        _viewers.ForEach(cv => cv.SubscriptionRemoved(username));

    }

    public Task<ImmutableList<string>> GetFollowingListAsync() => Task.FromResult(_state.State.Subscriptions.Keys.ToImmutableList());
    public Task<ImmutableList<string>> GetFollowersListAsync() => Task.FromResult(_state.State.Followers.Keys.ToImmutableList());

    public Task SubscribeAsync(IChirperViewer viewer)
    {
        _viewers.Add(viewer);
        return Task.CompletedTask;
    }

    public Task UnSubscribeAsync(IChirperViewer viewer)
    {
        _viewers.Remove(viewer);
        return Task.CompletedTask;
    }
    public Task<ImmutableList<ChirperMessage>> GetPublishedMessagesAsync(int n , int start )
    {
        if (start < 0 ) start = 0;
        if (start + n > _state.State.MyPublishedMessages.Count)
            n = _state.State.MyPublishedMessages.Count - start;
        
        return Task.FromResult(_state.State.MyPublishedMessages.Skip(start).Take(n).ToImmutableList());
    }
    public async Task AddFollowerAsync(string userName, IChirperSubscriber follower)
    {
        _state.State.Followers[userName] = follower;
        await WriteStateAsync();
        _viewers.ForEach(cv => cv.NewFollower(userName));
    }
    
    public Task RemoveFollowerAsync(string userName)
    {
        _state.State.Followers.Remove(userName);
        return WriteStateAsync();
    }

    public async Task NewChirpAsync(ChirperMessage chirp)
    {
        _logger.LogInformation("{GrainType} {username} reveived chirp message : {Chirp}).", GrainType, GrainKey, chirp);

        _state.State.RecentReceivedMessages.Enqueue(chirp);

        // only relevent when not using fixed queue
        while (_state.State.MyPublishedMessages.Count > PublishedMessagesCacheSize) // to keep not more than the max number of messages
        {
            _state.State.MyPublishedMessages.Dequeue();
        }

        await WriteStateAsync();

        // notify any viewers that a new chirp has been received
        _logger.LogInformation("{GrainType} {GrainKey} sending received chirp message to {ViewerCount} viewers", GrainType,GrainKey,_viewers.Count);
        _viewers.ForEach(_ => _.NewChirp(chirp));
    }
    private ChirperMessage CreateNewChirpMessage(string chirpMessage) => new(chirpMessage,DateTimeOffset.UtcNow, GrainKey);

     private async Task WriteStateAsync()
    {
        if (_outstandingWriteStateOperation is Task currentWriteStateOperation)
        {
            try
            {
                await currentWriteStateOperation;
            }
            catch 
            {
            }
            finally
            {
                if (_outstandingWriteStateOperation == currentWriteStateOperation)
                {
                    _outstandingWriteStateOperation = null;
                }
            }
        }
        if (_outstandingWriteStateOperation is null)
        {
            currentWriteStateOperation = _state.WriteStateAsync();
            _outstandingWriteStateOperation = currentWriteStateOperation;
        }
        else
        {
            currentWriteStateOperation = _outstandingWriteStateOperation;
        }

        try
        {
            await currentWriteStateOperation;
        }
        finally
        {
            if (_outstandingWriteStateOperation == currentWriteStateOperation)
            {
                _outstandingWriteStateOperation = null;
            }
        }
    }
}

[Serializable]
public class ChirperAccountState
{
    public Dictionary<string, IChirperPublisher> Subscriptions { get; init;} = new();
    public Dictionary<string, IChirperSubscriber> Followers { get; init;} = new();
    public Queue<ChirperMessage> RecentReceivedMessages { get; init;} = new();
    public Queue<ChirperMessage> MyPublishedMessages { get; init; } = new();
}

public static class EnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var item in enumerable)
        {
            action(item);
        }
    }
}

public class ClusterClientHostedService : IHostedService
{
    private readonly IGrainFactory _grainFactory;

    public ClusterClientHostedService(
        IGrainFactory grainFactory
    )
    {
       _grainFactory = grainFactory;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        AnsiConsole.Status().StartAsync("Connecting to server", async ctx => {
            ctx.Spinner(Spinner.Known.Dots);
            ctx.Status = "Connecting...";
        });
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        
        throw new NotImplementedException();
    }
}

public class ShellHostedService : BackgroundService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IGrainFactory _grainFactory;
    private IChirperAccount? _account;
    private ChirperConsoleViewer? _viewer;
    private IChirperViewer _viewerRef;

    public ShellHostedService(
        IHostApplicationLifetime applicationLifetime,
        IGrainFactory grainFactory
    )
    {
        _grainFactory = grainFactory;
        _applicationLifetime = applicationLifetime;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ShowHelp(true);

        while (!stoppingToken.IsCancellationRequested)
        {
            var command = Console.ReadLine();
            switch (command)
            {
                case "/help":
                    ShowHelp();
                    break;
                case "/quit":
                    _applicationLifetime.StopApplication();
                    break;
                case { } when command.StartsWith("/user"):
                    if (Regex.Match(command, @"/user (?<username\w{1,100})") is { Success: true } match)
                    {
                        await UnObserve();
                        var username = match.Groups["username"].Value;
                        _account = _grainFactory.GetGrain<IChirperAccount>(username);

                        AnsiConsole.MarkupLine("{bold grey][[[/][bold lime]V[/][bold grey]]][/] The current user is now [navy]{0}[/]",username);
                    } 
                    else
                    {
                        AnsiConsole.MarkupLine("[bold red]Invalid username[/][red].[/] Try again or type [bold fuchsia]/help[/] for a list of commands.");   
                    }
                    break;
                case { } when command.StartsWith("/follow"):
                    if (EnsureActiveAccount(_account))
                    {
                        if (Regex.Match(command, @"/follow (?<username>\w{1,100})") is {Success: true} match2)
                        {
                            var TargetUserName = match2.Groups["username"].Value;
                            await _account.FollowUserIdAsync(TargetUserName);
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[bold grey][[[/][bold red]X[bold grey]]][/] [red underline]Invalid target username[/][red].[/] Try again or type [bold fuchsia]/help[/] for a list of commands.");
                        }
                    }
                    break;
                case "/following":
                    if (EnsureActiveAccount(_account))
                    {
                        var following = await _account.GetFollowingListAsync();
                        AnsiConsole.Write(new Rule($"{_account.GetPrimaryKeyString()}'s followed accounts")
                        {
                            Alignment = Justify.Center,
                            Style =Style.Parse("blue")
                        });

                        foreach (var account in following)
                        {
                            AnsiConsole.MarkupLine("[bold yellow]{0}[/]", account);
                        }

                        AnsiConsole.Write(new Rule
                        {
                            Alignment = Justify.Center,
                            Style =Style.Parse("blue")
                        });
                    }
                    break;
                case "/followers":
                    if (EnsureActiveAccount(_account))
                    {
                        var followers = await _account.GetFollowersListAsync();
                        AnsiConsole.Write(new Rule($"{_account.GetPrimaryKeyString()}'s followed accounts")
                        {
                            Alignment = Justify.Center,
                            Style =Style.Parse("blue")
                        });

                        foreach (var account in followers)
                        {
                            AnsiConsole.MarkupLine("[bold yellow]{0}[/]", account);
                        }

                        AnsiConsole.Write(new Rule
                        {
                            Alignment = Justify.Center,
                            Style =Style.Parse("blue")
                        });
                    }
                    break;
                case "/observe":
                    if (EnsureActiveAccount(_account))
                    {
                        if (_viewerRef is null)
                        {
                            _viewer = new ChirperConsoleViewer(_account.GetPrimaryKeyString());
                            _viewerRef = await _grainFactory.CreateObjectReference<IChirperViewer>(_viewer);
                        }
                        await _account.SubscribeAsync(_viewerRef);

                        AnsiConsole.MarkupLine("[bold grey][[[/][bold lime]✓[/][bold grey]]][/] [bold olive]Now observing[/] [navy]{0}[/]", _account.GetPrimaryKeyString());
                    }
                    break;
                case "/unobserve":
                    if (EnsureActiveAccount(_account))
                    {
                        await Unobserve();
                        AnsiConsole.MarkupLine("[bold grey][[[/][bold lime]✓[/][bold grey]]][/] [bold olive]No longer observing[/] [navy]{0}[/]", _account.GetPrimaryKeyString());
                    }
                    break;
                case {} when command.StartsWith("/unfollow "):
                    if (EnsureActiveAccount(_account))
                    {
                        if (Regex.Match(command, @"/unfollow (?<usernname>\w{1,100})") is {Success: true} match3)
                        {
                            var targetUserName = match3.Groups["username"].Value;
                            await _account.UnFollowUserIdAsync(targetUserName);
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[bold grey][[[/][bold red]✗[/][bold grey]]][/] [red underline]Invalid target username[/][red].[/] Try again or type [bold fuchsia]/help[/] for a list of commands.");
                        }
                    }
                    break;
                case { } when command.StartsWith("/chirp "):
                    if (EnsureActiveAccount(_account))
                    {
                        if (Regex.Match(command, @"/chirp (?<message>.+)") is {Success: true} match4)
                        {
                            var message = match4.Groups["message"].Value;
                            await _account.PublishMessageAsync(message);
                            AnsiConsole.MarkupLine("[bold grey][[[/][bold lime]✓[/][bold grey]]][/] Published new message!");
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[bold grey][[[/][bold red]✗[/][bold grey]]][/] [red underline]Invalid chirp[/][red].[/] Try again or type [bold fuchsia]/help[/] for a list of commands.");
                        }
                    }
                    break;
                default:
                    AnsiConsole.MarkupLine(
                        "[bold grey][[[/][bold red]✗[/][bold grey]]][/] [red underline]Unknown command[/][red].[/] Type [bold fuchsia]/help[/] for a list of command."
                    );
                    break;
            }
        }
    }

    private Task Unobserve()
    {
        throw new NotImplementedException();
    }

    private static bool EnsureActiveAccount(
        [NotNullWhen(true)]
        IChirperAccount? account)
    {
        if (account is null)
        {
            AnsiConsole.MarkupLine("[bold grey][[[/][bold red]✗[/][bold grey]]][/]This command requires an [red underline]active user[/][red].[/] Set an active user using [bold fuchsia]/user[/] [aqua]username[/] or type [bold fuchsia]/help[/] for a list of command.");
            return false;
        }
        return true;
    }

    private async Task UnObserve()
    {
        if (_viewerRef is not null && _account is not null)
        {
            await _account.UnSubscribeAsync(_viewerRef);
            _viewerRef = null;
            _viewer = null;
        }
    }

    private static void ShowHelp(bool title = false)
    {
        var markup = new Markup(
            "[bold fuchsia]/help[/]: Shows this [underline green]help[/] text.\n"
            + "[bold fuchsia]/user[/]: [aqua]<username>[/]: Switches to the specified [underline green]user[/] account.\n"
            + "[bold fuchsia]/chirp[/]: [aqua]<message>[/]: [underline green]Chirps[/] a [aqua]message[/] from the active account.\n"
            + "[bold fuchsia]/follow[/]: [aqua]<username[/]: [underline green]Follows[/] the account with the specified [aqua]usename[/].\n"
            + "[bold fuchsia]/unfollow[/]: [aqua]<username[/]: [underline green]Unfollows[/] the account with the specified [aqua]usename[/].\n"
            + "[bold fuchsia]/following[/]: Lists the accounts that the active accounts is [underline green]following[/].\n"
            + "[bold fuchsia]/followers[/]: Lists the accounts [underline green]followers[/] the active account.\n"
            + "[bold fuchsia]/observe[/]: [underline green]Start observing[/] the active account."
            + "[bold fuchsia]/unobserve[/]: [underline green]Stop observing[/] the active account.\n"
            + "[bold fuchsia]/quit[/]: Closes this client.\n"
        );

        if (title)
        {
            // Add some flair for the title screen
            using var logoStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("logo.png");

                var logo = new CanvasImage(logoStream!)
                {
                    MaxWidth = 25
                };

                var table = new Table
                {
                    Border = TableBorder.None,
                    Expand = true,
                }.HideHeaders();
                table.AddColumn(new TableColumn("One"));

                var header = new FigletText("Orleans")
                {
                    Color = Color.Fuchsia
                };

                var header2 = new FigletText("Chirper")
                {
                    Color = Color.Aqua
                };

                table.AddColumn(new TableColumn("Two"));
                var rightTable = new Table().HideHeaders().Border(TableBorder.None).AddColumn(new TableColumn("Content"));

                rightTable.AddRow(header).AddRow(header2).AddEmptyRow().AddEmptyRow().AddRow(markup);
                table.AddRow(logo, rightTable);

                AnsiConsole.WriteLine();
                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();
        }
        else
        {
            AnsiConsole.Write(markup);
        }
    }
}

internal class ChirperConsoleViewer : IChirperViewer
{private readonly string _userName;

    /// <summary>
    /// Creates a new <see cref="IChirperViewer"/> that outputs notifications to the console.
    /// </summary>
    /// <param name="userName">The user name of the account being observed.</param>
    public ChirperConsoleViewer(string userName) =>
        _userName = userName ?? throw new ArgumentNullException(nameof(userName));

    /// <inheritdoc />
    public void NewChirp(ChirperMessage message) =>
        AnsiConsole.MarkupLine(
            "[[[dim]{0}[/]]] [aqua]{1}[/] [bold yellow]chirped:[/] {2}",
            message.TimeStamp.LocalDateTime, message.PublisherUserName, message.Message);

    public void NewFollower(string username) =>
        AnsiConsole.MarkupLine(
            "[bold grey][[[/][bold yellow]![/][bold grey]]][/] [aqua]{0}[/] is now following [navy]{1}[/]",
            username, _userName);

    /// <inheritdoc />
    public void SubscriptionAdded(string username) =>
        AnsiConsole.MarkupLine(
            "[bold grey][[[/][bold lime]✓[/][bold grey]]][/] [navy]{0}[/] is now following [aqua]{1}[/]",
            _userName, username);

    /// <inheritdoc />
    public void SubscriptionRemoved(string username) =>
        AnsiConsole.MarkupLine(
            "[bold grey][[[/][bold lime]✓[/][bold grey]]][/] [navy]{0}[/] is no longer following [aqua]{1}[/]",
            _userName, username);
    /*
    private readonly string _username;

    public ChirperConsoleViewer(string username) => _username = username ?? throw new ArgumentNullException(nameof(username));

    public void NewChirp(ChirperMessage message) => AnsiConsole.MarkupLine(
        "[[[dim]{0}[/]]] [aqua]{1}[/] [bold yellow]chirped:[/] {2}",message.TimeStamp.LocalDateTime, message.PublisherUserName, message.Message
    );

    public void NewFollower(string username) => AnsiConsole.MarkupLine(
        "[bold grey][[[/][bold lime]V[/][bold grey]]][/] [aqua]{0}[/] is now following [navy]{1}[/]", username, _username
    );

    public void SubscriptionAdded(string username) => AnsiConsole.MarkupLine(
        "[bold grey][[[/][bold lime]V[/][bold grey]]][/] [navy]{0}[/] is now following [aqua]{1}[/]",_username, username
    );

    public void SubscriptionRemoved(string username) => AnsiConsole.MarkupLine(
        "[bold grey][[[/][bold lime]V[/][bold grey]]][/] [navy]{0}[/] is no longer following [aqua]{1}[/]", _username, username
    );*/
}