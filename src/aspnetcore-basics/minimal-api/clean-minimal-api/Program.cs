
using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddHealthChecks();
builder.Services.AddFastEndpoints();
builder.Services.AddSwaggerDoc();
// builder.Services.AddSingleton<ICustomerRepository, CustomerRepository>();

var app = builder.Build();

app.MapHealthChecks("/healtz");
app.MapGet("/", () => "Hello World!");

app.Run();
