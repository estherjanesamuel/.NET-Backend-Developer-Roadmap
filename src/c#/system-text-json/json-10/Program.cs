using System.Text.Json;
using System.Text.Json.Serialization;

var app = WebApplication.Create();

app.MapGet("/", () => {
    var payload = new List<Person> 
    {
        new Person
        {
            Name = "Ephra",
            Age = 2,
            IsMarried = null,
            CurrentTime = DateTimeOffset.UtcNow,
            IsWorking = false,
        },

        new Person
        {
            Name = "Ether",
            Age = 1,
            IsMarried = false,
            IsHealthy = true,
        },

        new Person
        {
            Name = "Ariefs",
            IsHealthy = false,
        },

    };

    var opt = new JsonSerializerOptions 
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
    };

    return Results.Json(payload, opt);
});



app.Run();

public class Person
{
    public string Name { get;  set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public int? Age { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool? IsMarried { get; set; }

    [JsonIgnore(Condition =JsonIgnoreCondition.WhenWritingDefault)]
    public DateTimeOffset CurrentTime { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsWorking { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsHealthy { get; set; }
}

