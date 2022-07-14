using Orleans;
using Orleans.Concurrency;
using System.Runtime.Caching;

namespace tictactoe.Grains;
public interface IPairingGrain : Orleans.IGrainWithIntegerKey
{
    Task AddGame(Guid gameId, string name);
    Task RemoveGame(Guid gameId);
    Task<PairingSummary[]> GetGames();
}

[Immutable]
[Serializable]
public class PairingSummary
{
    public Guid GameId { get; set; }
    public string?  Name { get; set; }
}

public class PairingGrain : Grain, IPairingGrain
{
    private readonly MemoryCache _cache = new("pairing");
    public Task AddGame(Guid gameId, string name)
    {
        _cache.Add(gameId.ToString(), name, new DateTimeOffset(DateTime.UtcNow).AddHours(1));
        return Task.CompletedTask;
    }

    public Task<PairingSummary[]> GetGames() => Task.FromResult(_cache.Select(x => new PairingSummary {GameId = Guid.Parse(x.Key), Name = x.Value as string }).ToArray());

    public Task RemoveGame(Guid gameId) 
    {
        _cache.Remove(gameId.ToString());
        return Task.CompletedTask;
    }
}