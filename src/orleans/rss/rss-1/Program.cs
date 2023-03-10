using System.Net;
using Orleans;
using Orleans.Runtime;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Xml;
using Microsoft.SyndicationFeed.Rss;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;

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
        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(FeedSourceGrain).Assembly).WithReferences())
        .AddRedisGrainStorage("redis-rss-reader",  optBuilder => optBuilder.Configure(opt => {
            opt.ConnectionString = "127.0.0.1:6379"; //192.168.1.5
            opt.UseJson = true;
            opt.DatabaseNumber = 1;
        }));
});
var app = builder.Build();

app.MapGet("/", async ctx => {
    var client = ctx.RequestServices.GetService<IGrainFactory>()!;
    var feedSourceGrain = client.GetGrain<IFeedSource>(0)!;

    await feedSourceGrain.AddAsync(new FeedSource{
        Type = FeedType.Rss,
        Url = "http://www.scripting.com/rss.xml",
        Website = "http://www.scripting.com",
        Title = "Scripting News"
    });

    await feedSourceGrain.AddAsync(new FeedSource{
        Type = FeedType.Atom,
        Url = "https://www.reddit.com/r/dotnet.rss",
        // Website = "https://www.reddit.com/r/dotnet",
        Website = "https://libreddit.teknologiumum.com/r/dotnet",
        Title = "Reddit/r/dotnet"
    });
    var sources = await feedSourceGrain.GetAllAsync();
    foreach (var s in sources)
    {
        var feedFetcherGrain = client.GetGrain<IFeedFetcher>(s.Url.ToString());
        await feedFetcherGrain.FetchAsync(s);
    }

    var feedResultsGrain = client.GetGrain<IFeedItemResults>(0);
    var feedItems = await feedResultsGrain.GetAllAsync();

    await ctx.Response.WriteAsync(@"<html>
     <head>
         <link rel=""stylesheet"", href=""https://cdn.jsdelivr.net/npm/uikit@3.5.5/dist/css/uikit.min.css"" />
         <title>Orleans RSS Reader</title>
     </head>");
     await ctx.Response.WriteAsync("<body><div class=\"uk-container\">");
     await ctx.Response.WriteAsync("<ul class=\"uk-list\">");
     foreach (var i in feedItems)
     {
         await ctx.Response.WriteAsync("<li class=\"uk-card uk-card-default uk-card-body\" >");
         if (!string.IsNullOrWhiteSpace(i.Title))
            await ctx.Response.WriteAsync($"{i.Title} <br/>");
        
        await ctx.Response.WriteAsync(i.Description ?? "");

        if (i.Url is object)
            await ctx.Response.WriteAsync($"<br/> <a href=\"{i.Url}\">link</a>");
        
        await ctx.Response.WriteAsync($"<div style=\"font-size:small;\"> published on: {i.PublishedOn} </div>");
        await ctx.Response.WriteAsync($"<div style=\"font-size:small;\"> source: <a href=\"{i.Channel?.Website}\">{i.Channel?.Title}</a> </div>");
        await ctx.Response.WriteAsync("</li>");
     }
     await ctx.Response.WriteAsync("</ul>");
     await ctx.Response.WriteAsync("</div></body></html>");
});

app.Run();
public interface IFeedSource : IGrainWithIntegerKey
{
    Task AddAsync(FeedSource source);

    Task<List<FeedSource>> GetAllAsync();
}

public interface IFeedFetcher : IGrainWithStringKey
{
    Task FetchAsync(FeedSource source);
}

public interface IFeedItemResults : IGrainWithIntegerKey
{
    Task AddAsync(List<FeedItem> items);
    Task<List<FeedItem>> GetAllAsync();
    Task ClearAsync();
}

public record FeedItem
{
    public FeedChannel? Channel { get;  set; }

    public string? Id { get;  set; }
    public string? Title { get;  set; }
    public string? Description { get; private set; }
    public Uri? Url { get; private set; }
    public DateTimeOffset PublishedOn { get; private set; }
    public FeedItem(){}
    public FeedItem(
        FeedChannel channel, 
        SyndicationItem item
        )
    {
        Channel = channel;
        Id = item.Id;
        Title = item.Title;
        Description = item.Description;
        var link = item.Links.FirstOrDefault();
        if (link is object)
        {
            Url = link.Uri;
        }
        if (item.LastUpdated == default(DateTimeOffset))
        {
            PublishedOn = item.Published;
        }
        else
        {
            PublishedOn = item.LastUpdated;
        }
    }
}

public class FeedChannel
{
    public string? Title { get; set; }
    public string? Website { get; set; }
    public Uri? Url { get; set; }
    public bool HideTitle { get; set; }
    public bool HideDescription { get; set; }


}

public class FeedSource
{
    public string Url { get;  set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string? Website { get; set; }
    public FeedType Type { get;  set; }
    public bool HideTitle { get; set; } 
    public bool HideDescription { get; set; } 

    public FeedChannel ToChannel()
    {
        return new FeedChannel{
            Title = Title,
            Website = Website,
            HideTitle = HideTitle,
            HideDescription =HideDescription
        };
    }
}

public class FeedSourceGrain: Grain, IFeedSource
{
    private readonly IPersistentState<FeedSourceStore> _storage;

    public FeedSourceGrain(
        [PersistentState("feed-source", "redis-rss-reader")]
        IPersistentState<FeedSourceStore> storage
    )
    {
        _storage = storage;
    }
    public async Task AddAsync(FeedSource source)
    {
        if (_storage.State.FeedSources.Find(x => x.Url == source.Url) is null)
        {
            _storage.State.FeedSources.Add(source);
            await _storage.WriteStateAsync();
        }
    }

    public Task<List<FeedSource>> GetAllAsync() => Task.FromResult(_storage.State.FeedSources);
}

public class FeedFetcherGrain : Grain, IFeedFetcher
{
    private IGrainFactory _grainFactory;

    public FeedFetcherGrain(
        IGrainFactory grainFactory
    )
    {
        _grainFactory = grainFactory;
    }

    public async Task FetchAsync(FeedSource source)
    {
        var storage = _grainFactory.GetGrain<IFeedItemResults>(0);
        var results = await ReadFeedAsync(source);
        await storage.AddAsync(results);
    }

    public async Task<List<FeedItem>> ReadFeedAsync(FeedSource source)
    {
        var feed = new List<FeedItem>();
        try
        {
            using var xmlReader = XmlReader.Create(source.Url.ToString(), new XmlReaderSettings(){Async = true});
            if (source.Type == FeedType.Rss)
            {
                var feedReader = new RssFeedReader(xmlReader);

                //read the feed
                while (await feedReader.Read())
                {
                    switch (feedReader.ElementType)
                    {
                        //Read Item
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
                        //Read item
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
            return feed;
        }
        catch 
        {
            
            return new List<FeedItem>();
        }
    }

   
}


public class FeedItemResultGrain : Grain, IFeedItemResults
{
    private readonly IPersistentState<FeedItemStore> _storage;

    public FeedItemResultGrain(
        [PersistentState("feed-item-results","redis-rss-reader")]
        IPersistentState<FeedItemStore> storage
    )
    {
        _storage = storage;
    }
    public async Task AddAsync(List<FeedItem> items)
    {
        //make sure there is no duplication
        foreach (var i in items.Where(x => !string.IsNullOrWhiteSpace(x.Id)))
        {
            if (!_storage.State.Result.Exists(x => x.Id?.Equals(i.Id, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                _storage.State.Result.Add(i);
            }
        }
        await _storage.WriteStateAsync();
    }

    public async Task ClearAsync()
    {
        _storage.State.Result.Clear();
        await _storage.WriteStateAsync();
    }

    public Task<List<FeedItem>> GetAllAsync()
    {
        return Task.FromResult(_storage.State.Result.OrderByDescending(x => x.PublishedOn).ToList());
    }
}

public record FeedItemStore
{
    public List<FeedItem> Result { get;  set; } = new List<FeedItem>();
}

public enum FeedType
{
    Atom,
    Rss
}

public record FeedSourceStore
{
    public List<FeedSource> FeedSources { get; set; } = new List<FeedSource>();
}