// See https://aka.ms/new-console-template for more information
using Grpc.Net.Client;
using GrpcGreaterClient;

Console.WriteLine("Hello, World!");

// The port number must match the port of the gRPC server.
using var channel = GrpcChannel.ForAddress(" http://localhost:5256");
var client = new Greeter.GreeterClient(channel);
var reply = await client.SayHelloAsync(
    new HelloRequest { Name = "Arief" }
);

Console.WriteLine("Greeting: " + reply.Message);
Console.WriteLine("Press any key to exit...");
Console.ReadKey();

