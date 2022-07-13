IResult MyDataMap(MyData data) => Results.Json(new {greetings = $"Hello {data.Name}"});
var builder = WebApplication.CreateBuilder();
builder.Services.AddSingleton<MyData>();
var app = builder.Build();
app.Map("/", MyDataMap);
app.Run();

public record MyData{
    public string Name => "Arief";
}