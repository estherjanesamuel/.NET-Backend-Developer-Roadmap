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

    return Results.Json(payload);
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