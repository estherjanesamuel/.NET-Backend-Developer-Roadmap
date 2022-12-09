var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapHealthChecks("/healtz");
app.Run();
