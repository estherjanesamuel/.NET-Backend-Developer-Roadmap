
namespace GrainInterfaces;
public interface IHello
{
    ValueTask<string> SayHello(string greeting);
}