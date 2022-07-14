using Orleans;

namespace Contracts;
  
public interface IHelloGrain : IGrainWithIntegerKey
{
    Task<string> SayHello(string greeting);
}

