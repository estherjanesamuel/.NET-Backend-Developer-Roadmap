using Elsa.Extensions;
using Elsa.Workflows.Core.Activities;
using Elsa.Workflows.Core.Services;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddElsa();

var serviceProvider = services.BuildServiceProvider();
/*
    This sample demonstrates a very simple workflow Activity that writes a line to the console.
*/
var workflow = new WriteLine("Hello, ariefs"); // this is an esla activity
var runner = serviceProvider.GetRequiredService<IWorkflowRunner>();

await runner.RunAsync(workflow);