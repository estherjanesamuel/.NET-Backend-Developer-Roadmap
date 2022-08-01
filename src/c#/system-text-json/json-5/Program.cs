using System.Text.Json;
using System.Text.Json.Serialization;

var app = WebApplication.Create();
app.MapGet("/", () => {
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
        Extensions = new Dictionary<string, object>
        {
            {"SuperPowers", new {Flight = false, Humor = true, Invisibility = true}}, // ad hoc object
            {"FavouriteWords", new string[] {"Hello","Oh Dear", "Bye"}}, // an array of primitives
            {"Stats", new object[] {new {Flight=0}, new {Humor = 99}, new {Invisibility = 30, Charge = true}}}, // an array of mixed object
        }
    };

    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
    };

    return Results.Json(payload, options);
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
    public Dictionary<string, object> Extensions { get; set; }
}