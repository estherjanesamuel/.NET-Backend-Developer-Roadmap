using System.Collections.Immutable;
using Orleans;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();

public interface IBubleWrap : IGrainWithStringKey
{
    Task<bool[,]?> GetSheetState();
    Task Pop(bool[,] expectedSheet);
}

public interface IDadJoke : IGrainWithIntegerKey 
{
    Task<ImmutableList<(string Id, string Url)>> GetRandomJoke(GrainCancellationToken grainCancellationToken);
}

public interface IInlineQuery : IGrainWithStringKey
{
    Task<ImmutableList<InlineQueryResults>> GetResults(string query, long userId, GrainCancellationToken grainCancellationToken);
}

public interface IOpenGraph : IGrainWithStringKey
{
    Task<OpenGraphMetadata?> GetMetadata(TimeSpan timeout);
}

public interface ITenor : IGrainWithStringKey
{
    Task<ImmutableList<(string Id, string Url, string PreviewUrl)>> SearchGifts(GrainCancellationToken grainCancellationToken);
}

public interface ITrackMessage : IGrainWithIntegerKey
{
    Task TrackMessage(string sender, string text, long? replyToMessageId);
    Task<(string? sender, string? text, long? replyToMessageId)> GetMessage();
    Task<ImmutableList<(string sender,string text)>> GetThread(int maxLines);
}

public class OpenGraphMetadata
{
}

public class InlineQueryResults
{
}