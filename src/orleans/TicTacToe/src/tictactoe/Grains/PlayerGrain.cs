using Orleans;

namespace tictactoe.Grains;
public interface IPlayerGrain : IGrainWithGuidKey
{
    // get a list of all active games
    Task<PairingSummary[]> GetAvailableGames();
    Task<List<GameSummary>> GetGameSummaries();

    // create a new game and join it
    Task<Guid> CreateGame();
    Task<GameState> JoinGame(Guid gameId);
    Task LeaveGame(Guid guid, GameOutcome gameOutcome);
    Task SetUsername(string username);
    Task<string> GetUsername();
}
public class PlayerGrain : Grain, IPlayerGrain
{
    private List<Guid> _activeGames = new();
    private List<Guid> _pastGames = new();
    private int _wins;
    private int _loses;
    private int _gamesStarted;
    private string _username = null!;

    public override Task OnActivateAsync()
    {
        _activeGames = new List<Guid>();
        _pastGames = new List<Guid>();

        _wins = 0;
        _loses = 0;
        _gamesStarted = 0;
        
        return base.OnActivateAsync();
    }
    public async Task<Guid> CreateGame()
    {
        _gamesStarted++;

        var gameId = Guid.NewGuid();
        var gameGrain = GrainFactory.GetGrain<IGameGrain>(gameId); // create a new game

        // add ourselves to the game
        var playerId = this.GetPrimaryKey(); // our player id
        await gameGrain.AddPlayerToGame(playerId);
        _activeGames.Add(gameId);
        var name = $"{_username}'s {AddOrdinalSuffix(_gamesStarted.ToString())} game";
        await gameGrain.SetName(name);

        var pairingGrain = GrainFactory.GetGrain<IPairingGrain>(0);
        await pairingGrain.AddGame(gameId, name);

        return gameId;

    }

    private static string AddOrdinalSuffix(string number)
    {
        var n = int.Parse(number);
        var nMod100 = n % 100;

        return nMod100 switch
        {
            >= 11 and <= 13 => string.Concat(number,"th"),
            _ => (n % 10) switch
            {
                1 => string.Concat(number, "st"),
                2 => string.Concat(number, "nd"),
                3 => string.Concat(number, "rd"),
                _ => string.Concat(number, "th"),
            }
        };
    }

    public async Task<PairingSummary[]> GetAvailableGames()
    {
        var grain = GrainFactory.GetGrain<IPairingGrain>(0);
        return (await grain.GetGames()).Where(x => !_activeGames.Contains(x.GameId)).ToArray();
    }

    public async Task<List<GameSummary>> GetGameSummaries()
    {
        var tasks = new List<Task<GameSummary>>();
        foreach (var gameId in _activeGames)
        {
            var game = GrainFactory.GetGrain<IGameGrain>(gameId);
            tasks.Add(game.GetSummary(this.GetPrimaryKey()));
        }

        await Task.WhenAll(tasks);
        return tasks.Select(x => x.Result).ToList();
    }

    public Task<string> GetUsername() => Task.FromResult(_username);

    // join a game that is waiting players
    public async Task<GameState> JoinGame(Guid gameId)
    {
        var gameGrain = GrainFactory.GetGrain<IGameGrain>(gameId);
        var state = await gameGrain.AddPlayerToGame(this.GetPrimaryKey());
        _activeGames.Add(gameId);

        var pairingGrain = GrainFactory.GetGrain<IPairingGrain>(0);
        await pairingGrain.RemoveGame(gameId);

        return state;
    }

    // leave game whem it is over
    public Task LeaveGame(Guid gameId, GameOutcome outcome)
    {
        // manage game list
        _activeGames.Remove(gameId);
        _pastGames.Add(gameId);

        // manage running total
        _ = outcome switch 
        {
            GameOutcome.Win => _wins++,
            GameOutcome.Lose => _loses++,
            _ => 0
        };

        return Task.CompletedTask;
    }

    public Task SetUsername(string username)
    {
        _username = username;
        return Task.CompletedTask;
    }
}