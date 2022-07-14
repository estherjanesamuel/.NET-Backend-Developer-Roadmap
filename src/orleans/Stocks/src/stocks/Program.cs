using System.Net;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<StocksHostedService>();
builder.Logging.SetMinimumLevel(LogLevel.Information).AddConsole();
builder.Host.UseOrleans(builder => {
    builder
        .UseLocalhostClustering()
        .Configure<ClusterOptions>(options => {
            options.ClusterId = "dev";
            options.ServiceId = "StocksApp";
        })
        .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(StockGrain).Assembly).WithReferences());
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();


public interface IStockGrain : IGrainWithStringKey
{
    Task<string> GetPrice();
}
public class StockGrain : Grain, IStockGrain
{
    private string _price = null!;
    private const string ApiKey = "5NVLFTOEC34MVTDE";
    private readonly HttpClient _httpClient = new();

    public override async Task OnActivateAsync()
    {
        this.GetPrimaryKey(out var stock);
        await UpdatePrice(stock);

        RegisterTimer(
            UpdatePrice,
            stock,
            TimeSpan.FromMinutes(2),
            TimeSpan.FromMinutes(2)
        );

        await base.OnActivateAsync();
    }

    private async Task UpdatePrice(object stock)
    {
        var priceTask = GetPriceQuote((string)stock);
        _price = await priceTask;
    }

    private async Task<string> GetPriceQuote(string stock)
    {
        using var resp = await _httpClient.GetAsync($"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={stock}&apikey={ApiKey}&datatype=csv");
        return await resp.Content.ReadAsStringAsync();
    }

    public Task<string> GetPrice() => Task.FromResult(_price);
}

public class StocksHostedService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IClusterClient _client;
    private readonly List<string> _symbols = new() {"MSFT", "GOOG", "AAPL", "GME", "AMZN"};

    public StocksHostedService(ILogger<StocksHostedService> logger,IClusterClient client)
    {
        _logger = logger;
        _client = client;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Fetching stock prices");
                var tasks = new List<Task<string>>();
                // Fan out calls to each of the stock grains
                foreach (var symbol in _symbols)
                {
                    var stockGrain = _client.GetGrain<IStockGrain>(symbol);
                    tasks.Add(stockGrain.GetPrice());
                }

                // Collect the results
                await Task.WhenAll(tasks);

                // Print the results
                foreach (var task in tasks)
                {
                    var price = await task;
                    _logger.LogInformation("Price is {Price}", price);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            catch (System.Exception error) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(error, "Error fetching stock price");
            }
        }
    }
}