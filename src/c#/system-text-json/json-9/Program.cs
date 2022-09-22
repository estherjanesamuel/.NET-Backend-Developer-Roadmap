using System.Text.Json;
using System.Text.Json.Serialization;

var app = WebApplication.Create();

app.MapGet("/", () => {
    var payload = new Person
    {
        Name = "Ephra",
        TimeWaiting = new TimeSpan(1000,0,0,0) // 1000 days
    };

    var opt = new JsonSerializerOptions 
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new TimeSpanConverter()}
    };

    return Results.Json(payload, opt);
});



app.Run();

public class Person
{
    public string Name { get;  set; } = string.Empty;
    public TimeSpan TimeWaiting { get; set; }
}

internal class TimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) => TimeSpan.Parse(reader.GetString());

    public override void Write(
        Utf8JsonWriter writer, 
        TimeSpan value, 
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}