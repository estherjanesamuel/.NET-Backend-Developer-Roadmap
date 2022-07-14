using System.Collections.Immutable;

namespace Presence.Grains.Models;
public record class GameStatus(
    ImmutableHashSet<Guid> PlayerKeys,
    string Score
)
{
    public GameStatus WithNewScore(string newScore) => this with {Score = newScore};

    public static GameStatus Empty {get; }= new GameStatus(
        ImmutableHashSet<Guid>.Empty,
        string.Empty
    );
}