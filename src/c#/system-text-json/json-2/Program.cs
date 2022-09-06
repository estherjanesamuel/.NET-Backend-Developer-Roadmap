using System.Text.Json;
using System.Text.Json.Serialization;

var app = WebApplication.Create();

app.MapGet("/", () => {
    var payload = new Person
    {
        Name = "Ephra",
        Age = 2,
        IsMarried = false,
        CurrentTime = DateTimeOffset.UtcNow,
        Characters = new Dictionary<string, bool>
        {
            {"Funny", true},
            {"Feisty", true},
            {"Briliant", true},
            {"FOMA", false}
        }
    };

    var opt = new JsonSerializerOptions
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
    };

    return Results.Json(payload,opt);
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