using System.Security.Cryptography.X509Certificates;
using Contracts;
using Microsoft.Extensions.Logging;
using Orleans;


var client = new ClientBuilder()
    .UseLocalhostClustering(serviceId: "HelloWorldApp", clusterId: "dev" )
    .UseTls(
        StoreName.My,
        "fakedomain.faketld",
        allowInvalid:true,
        StoreLocation.CurrentUser,
        opt => {
            opt.OnAuthenticateAsClient = (conn, sslOpt) => {
                sslOpt.TargetHost = "fakedomain.faketld";
            };

            opt.AllowAnyRemoteCertificate();
        })
    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IHelloGrain).Assembly).WithReferences())
    .ConfigureLogging(log => log.AddConsole())
    .Build();