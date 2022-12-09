var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/healtz");
app.MapGet("/", () => "Hello World!");

app.Run();
