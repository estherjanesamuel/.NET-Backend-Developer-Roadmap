using System.Text.Json;
using System.Text.Json.Serialization;

var app = WebApplication.Create();
app.MapGet("/", () => {
    var payload = new
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