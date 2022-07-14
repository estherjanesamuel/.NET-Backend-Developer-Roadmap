// See https://aka.ms/new-console-template for more information
using Orleans;
using Orleans.Hosting;
using System.Security.Cryptography.X509Certificates;
using Contracts;

await new HostBuilder()
    .UseEnvironment(Environments.Development)
    .UseOrleans((ctx, builder) =>
    {
        var isDevelopment = ctx.HostingEnvironment.IsDevelopment();
        builder
            .UseLocalhostClustering()
            .UseTls(
                StoreName.My,
                "fakedomain.faketld",
                allowInvalid: isDevelopment,
                StoreLocation.CurrentUser,
                options =>
                {
                    // In this sample there is only one silo, however if there are multiple silos then the TargetHost must be set
                    // for each connection which is initiated.
                    options.OnAuthenticateAsClient = (connection, sslOptions) =>
                    {
                        sslOptions.TargetHost = "fakedomain.faketld";
                    };

                    if (isDevelopment)
                    {
                        // NOTE: Do not do this in a production environment
                        options.AllowAnyRemoteCertificate();
                    }
                })
            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).AddApplicationPart(typeof(IHelloGrain).Assembly));
    })
    .ConfigureLogging(logging => logging.AddConsole())
    .RunConsoleAsync();
  
public class HelloGrain : Grain, IHelloGrain
{
    private readonly ILogger<HelloGrain> _logger;

    public HelloGrain(ILogger<HelloGrain> logger)
    {
        _logger = logger;
    }
    public Task<string> SayHello(string greeting)
    {
        _logger.LogInformation("SayHello message received greeting = '{Greeting}'", greeting);
        return Task.FromResult($"You said : '{greeting}', I say: Hello!");
    }
}