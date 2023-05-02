// See https://aka.ms/new-console-template for more information
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();
app.Run();

Console.WriteLine("Hello, World!");

public interface IProdNewDbContext : IDisposable
{
    Task Ping(CancellationToken cancellationToken);
}

public class ProdNewDbContext : DbContext ,IProdNewDbContext
{
    private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();
    public ProdNewDbContext(DbContextOptions<ProdNewDbContext> optionsBuilder) : base(optionsBuilder)
    {
    }

    public void Dispose()
    {
        _stoppingCts.Dispose();
    }

    public async Task Ping(CancellationToken cancellationToken)
    {
        await Database.ExecuteSqlRawAsync("SELECT 1").ConfigureAwait(false);
    }
}