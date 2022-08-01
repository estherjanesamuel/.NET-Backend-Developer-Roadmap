using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Net.Http.Headers;

var app = WebApplication.Create();
app.MapGet("/", async context => {
    var payload = new Person
    {
        Name = "Jane",
        Age = 1,
        IsMarried = false,
        CurrentTime = DateTimeOffset.Now,
        Characters = new Dictionary<string, bool>
        {
            {"funny", true},
            {"feisty", true},
            {"brilliant", true},
            {"foma", false},
        },
        IsWorking = false,
        SuperPowers = new List<SuperPower>
        {
            new SuperPower("Humor", 8),
            new SuperPower("Intelligence", 10),
            new SuperPower("Focus", 7),
        }
    };

    var options = new JsonWriterOptions
    {
        Indented = true,
    };

    context.Response.Headers.Add(HeaderNames.ContentType, "application/json");
    await using (var writer = new Utf8JsonWriter(context.Response.Body, options))
    {
        writer.WriteStartObject();
        writer.WriteString("name", payload.Name);
        writer.WriteNumber("age", payload.Age);
        writer.WriteBoolean("isMarried", payload.IsMarried);
        writer.WriteString("currentTime", payload.CurrentTime);

        writer.WriteStartObject("characters");
        foreach (var kv in payload.Characters)
        {
            writer.WriteBoolean(kv.Key, kv.Value);
        }
        writer.WriteEndObject();

        writer.WriteStartArray("superpowers");
        foreach (var kv in payload.SuperPowers)
        {
            writer.WriteStartObject();
            writer.WriteNumber(kv.Name,kv.Rating);
            writer.WriteEndObject();
        }
            writer.WriteEndArray();
            writer.WriteEndObject();

    }
});

app.Run();

public class Person
{
    [JsonPropertyName("FullName")]
    public string Name { get; set; }
    public int Age { get; set; }
    public bool IsMarried { get; set; }
    public DateTimeOffset CurrentTime { get; set; }

    // [JsonIgnore]
    public bool? IsWorking { get; set; }
    // [JsonIgnore]
    public Dictionary<string, bool> Characters { get; set; }
    public List<SuperPower> SuperPowers { get; set; }
}

public class SuperPower
{
    public string Name { get; set; }
    public short Rating { get; set; }
    public SuperPower(string name, short rating)
    {
        Name = name;
        Rating = rating;
    }
}