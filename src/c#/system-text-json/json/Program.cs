var app = WebApplication.Create();

app.MapGet("/", () => {
    var payload = new Person
    {
        Name = "Ephra",
        Age = 33,
        IsMarried = false,
        CurrentTime = DateTimeOffset.UtcNow,
        Characters = new Dictionary<string, bool>
        {
            {"Funny", true},
            {"Feisty", true},
            {"Briliant", true},
            {"FOMA", true}
        },
        IsWorking = false
    };

    return Results.Json(payload);
});

app.Run();

public class Person
{
    public string Name { get;  set; }
    public int Age { get;  set; }
    public bool IsMarried { get;  set; }
    public DateTimeOffset CurrentTime { get;  set; }
    public Dictionary<string, bool> Characters { get;  set; }
    public bool IsWorking { get;  set; }
}