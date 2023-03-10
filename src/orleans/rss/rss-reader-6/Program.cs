using System.Xml;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;
using Microsoft.SyndicationFeed.Rss;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();

public interface IFeedSource : IGrainWithIntegerKey
{
    Task Add(FeedSource source);
    Task<List<FeedSource>> GetAll();
    Task<FeedSource?> FindFeedSourceByUrl(string url);
    Task<FeedSource?> UpdateFeedSourceStatus(string url, bool activeStatus,string? message);
}
public interface IFeedFetcher : IGrainWithStringKey
{
    Task Fetch(FeedSource source);
}
public interface IFeedItemResults : IGrainWithIntegerKey
{
    Task Add(List<FeedItem> items);
    Task<List<FeedItem>> GetAll();
    Task Clear();
}
public interface IFeedFetcherReminder: IGrainWithStringKey
{
    Task AddReminder(string reminder, short repeatEveryMinute);
}
public interface IFeedStreamReader : IGrain
{
    
}

public class FeedFetcherReminder : Grain, IFeedFetcherReminder
{
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<FeedFetcherReminder> _logger;

    public FeedFetcherReminder(
        IGrainFactory grainFactory,
        ILogger<FeedFetcherReminder> logger
    )
    {
        _grainFactory = grainFactory;
        _logger = logger;
    }
    public async Task AddReminder(string reminder, short repeatEveryMinute)
    {
        if (string.IsNullOrWhiteSpace(reminder)) throw new ArgumentNullException(nameof(reminder));
        var r = await GetReminder(reminder);
        if (r is not object) await RegisterOrUpdateReminder(reminder, dueTime: TimeSpan.FromSeconds(1), period: TimeSpan.FromMinutes(repeatEveryMinute));
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        _logger.Info($"Receive {reminderName} reminder");
        var feedSourceGrain = _grainFactory.GetGrain<IFeedSource>(0)!;
        var feedSource = await feedSourceGrain.FindFeedSourceByUrl(reminderName);
        if (feedSource is object)
        {
            _logger.Info($"Fetching {feedSource.Url}");
            var feedFetcherGrain = _grainFactory.GetGrain<IFeedFetcher>(feedSource.Url);
            await feedFetcherGrain.Fetch(feedSource);
        }
    }


}
public class FeedStreamReaderGrain : Grain, IFeedStreamReader
{
    private readonly ILogger _logger;
    private readonly IGrainFactory _grainFactory;

    public FeedStreamReaderGrain(ILogger<FeedStreamReaderGrain> logger, IGrainFactory grainFactory)
    {
        _logger = logger;
        _grainFactory = grainFactory;
    }

    public override async Task OnActivateAsync()
    {
        var streamProvider = GetStreamProvider(Config.StreamProvider);
        var stream = streamProvider.GetStream<List<FeedItem>>(Config.StreamId, Config.StreamChannel);
        var feedItemResultGrain = _grainFactory.GetGrain<IFeedItemResults>(0);
        await stream.SubscribeAsync<List<FeedItem>>(async (data, token) =>
        {
            _logger.Info($"Feed Items {data.Count}");
            await feedItemResultGrain.Add(data);
        });
    }
}
public class FeedSourceGrain : Grain, IFeedSource
{
    private readonly IPersistentState<FeedSourceStore> _storage;

    public FeedSourceGrain(
        [PersistentState(stateName:"feed-source-5","")]
        IPersistentState<FeedSourceStore> storage
    )
    {
        _storage = storage;
    }
    public async Task Add(FeedSource source)
    {
        if (string.IsNullOrWhiteSpace(source.Url)) return;
        if (_storage.State.Sources.Find(x => x.Url == source.Url) is null)
        {
             _storage.State.Sources.Add(source);
             await _storage.WriteStateAsync();
        }
    }

    public Task<FeedSource?> FindFeedSourceByUrl(string url) => Task.FromResult(_storage.State.Sources.Find(x => x.Url.Equals(url, StringComparison.Ordinal)));

    public Task<List<FeedSource>> GetAll() => Task.FromResult(_storage.State.Sources);

    public async Task<FeedSource?> UpdateFeedSourceStatus(string url, bool activeStatus, string? message)
    {
        var feed = await FindFeedSourceByUrl(url);
        if (feed is object)
        {
            // feed.LogFetchAttempt(activeStatus, message);
            await _storage.WriteStateAsync();
        }

        return feed;
    }
}
public class FeedFetchGrain : Grain, IFeedFetcher
{
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public FeedFetchGrain(IGrainFactory grainFactory,
    ILogger<FeedFetchGrain> logger,
    IHttpClientFactory httpClientFactory)
    {
        _grainFactory = grainFactory;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }
    public async Task Fetch(FeedSource source)
    {
        var results = await ReadFeadAsync(source);
        var streamProvider = GetStreamProvider(Config.StreamProvider);
        var stream = streamProvider.GetStream<List<FeedItem>>(Config.StreamId, Config.StreamChannel);
        await stream.OnNextAsync(results);
    }

    public async Task<List<FeedItem>> ReadFeadAsync(FeedSource source)
    {
        if (string.IsNullOrWhiteSpace(source.Url)) return new List<FeedItem>();
        if (!source.CanFetch()) return new List<FeedItem>();
        var feed = new List<FeedItem>();
        FeedType feedType = FeedType.Rss;
        try
        {
            _logger.LogInformation($"Fetching {source.Url}");
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            var response = await client.GetAsync(source.Url.ToString());

            var memory = new MemoryStream();
            await response.Content.CopyToAsync(memory);
            memory.Seek(0, SeekOrigin.Begin);
            char[] buf = new char[400]; // we need large buffer because to skip xml metadata and comments before the root of the xml document start 
            var sr = new StreamReader(memory);
            var charRead = sr.ReadBlock(buf,0,buf.Length);

            if (!new string(buf).Contains("rss", StringComparison.OrdinalIgnoreCase)) feedType = FeedType.Atom;
            memory.Seek(0, SeekOrigin.Begin);
            using var xmlReader = XmlReader.Create(memory, new XmlReaderSettings {DtdProcessing = DtdProcessing.Ignore });
            if (feedType == FeedType.Rss)
            {
                var feedReader = new RssFeedReader(xmlReader);
                //Read the feed
                while (await feedReader.Read())
                {
                    switch (feedReader.ElementType)
                    {
                        // read item
                        case SyndicationElementType.Item:
                            var item = await feedReader.ReadItem();
                            feed.Add(new FeedItem(source.ToChannel(), new SyndicationItem(item)));
                            break;
                        default:
                            var content = await feedReader.ReadContent();
                            break;
                    }
                }
            }
            else
            {
                var feedReader = new AtomFeedReader(xmlReader);
                while (await feedReader.Read())
                {
                    switch (feedReader.ElementType)
                    {
                        // read item
                        case SyndicationElementType.Item:
                            var entry = await feedReader.ReadEntry();
                            feed.Add(new FeedItem(source.ToChannel(), new SyndicationItem(entry)));
                            break;
                        default:
                            var content = await feedReader.ReadContent();
                            break;
                    }
                }
            }

            var feedSource = _grainFactory.GetGrain<IFeedSource>(0)!;
            await feedSource.UpdateFeedSourceStatus(source.Url, true, $"{feed.Count} items fetched");

            return feed;
        }
        catch (System.Exception ex)
        {
            _logger.LogError($"({feedType}) {source.Url} Exception: {ex.Message}");
            // mark feed as invalid
            var feedSource = _grainFactory.GetGrain<IFeedSource>(0)!;
            await feedSource.UpdateFeedSourceStatus(source.Url, false, ex.Message);

            return new List<FeedItem>();
        }
    }
}
public enum FeedType
{
    Rss,
    Atom
}
public class FeedSourceStore
{
    public List<FeedSource> Sources { get; set; } = new();
}
public class FeedSource
{
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? WebSite  { get; set; }
    public bool HideTitle { get; set; }
    public bool HideDescription { get; set; }
    public short UpdateFrequencyInMinutes { get; set; } = 1;
    public List<FeedHistory> History { get;  set; } = new();
    internal bool CanFetch() => History.Take(10).Count(x => !x.IsValid) <= 3;
    public void LogFetchAttempt(bool IsValid, string? message = null) => History.Insert(0, new FeedHistory { TimeStamp = DateTimeOffset.UtcNow, IsValid = IsValid, Message = message });
    public bool IsLatesValid
    {
        get
        {
            if (History.Count == 0) return true;
            return History.First().IsValid;
        }
    }
    public FeedChannel ToChannel()
    {
        return new FeedChannel{
            Title = Title,
            WebSite = WebSite,
            HideTitle = HideTitle,
            HideDescription = HideDescription
        };

    }
}
public class FeedHistory
{
    public DateTimeOffset TimeStamp { get;  set; }
    public bool IsValid { get;  set; }
    public string? Message { get;  set; }
}
public class FeedItem
{
    public FeedChannel? Channel { get; set; }
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Uri? Url { get; set; }
    public DateTimeOffset PublishedOn { get; set; }
    public FeedItem()
    {
        
    }

    public FeedItem(FeedChannel channel, SyndicationItem item)
    {
        Channel = channel;
        Id = item.Id;
        Title = item.Title;
        Description = item.Description;
        var link = item.Links.FirstOrDefault();
        if (link is object) Url = link.Uri;
        PublishedOn = item.LastUpdated == default(DateTimeOffset) ?  item.LastUpdated : item.LastUpdated;
    }
    
}
public class FeedChannel
{
    public string? Title { get;  set; }
    public string? WebSite { get;  set; }
    public Uri? Url { get; set; }
    public bool HideTitle { get; internal set; }
    public bool HideDescription { get; internal set; }
}
public class Config
{
    public static string StreamProvider { get; internal set; }
    public static Guid StreamId { get; internal set; }
    public static string StreamChannel { get; internal set; }
    public string RedisStorage { get; set; } = string.Empty;
}