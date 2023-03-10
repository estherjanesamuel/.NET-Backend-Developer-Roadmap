using System.Threading.Tasks;
using GrainInterfaces;
using Microsoft.Extensions.Logging;

namespace Grains;

public class HelloGrain : IHello
{
    private readonly ILogger<HelloGrain> _logger;

    public HelloGrain(
        ILogger<HelloGrain> logger
    )
    {
        _logger = logger;
    }
    ValueTask<string> SayHello(string greeting)
    {
        _logger.LogInformation("SayHello message received: greeting = '{Greeting}'", greeting);
        string result = string.Format("client said: '{greeting}', so HelloGrain says: Hello!!",greeting);
        return ValueTask.FromResult(result);
    }
}
