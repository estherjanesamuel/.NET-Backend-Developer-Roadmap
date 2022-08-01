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
        IsWorking = false
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
    public string Name { get; set; }
    public int Age { get; set; }
    public bool IsMarried { get; set; }
    public DateTimeOffset CurrentTime { get; set; }
    public bool? IsWorking { get; set; }
    public Dictionary<string, bool> Characters { get; set; }
}