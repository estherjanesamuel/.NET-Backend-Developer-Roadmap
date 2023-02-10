namespace hello;

// example 
// what is your name ? ariefs
// hello, ariefs, nice to meet you!

public class Program
{
    static void Main()
    {
        /*
        v2
        Add chalenge, no variables.
        */ 
        Console.Write("What is your name? ");
        Console.WriteLine(GetMessage(Console.ReadLine()));
        // string? name = GetName();
        // string message = GetMessage(name);
        // WriteMessage(message);
    }

    public static string GetMessage(string? name)
    {
        // return "";
        if (string.IsNullOrEmpty(name))
        {
            return "Hello, It is nice to meet you!";
        }
        return $"Hello, {name}, nice to meet you!";
        // v3, Challenge
        // different greetings for different people

    }

    // v3, Challenge
    // different greetings for different people
    public static string GetMessageWithName(string? name)
    {
        return name switch 
        {
            "Ariefs" => $"Hello, {name}, How's it going?",
            "Juita" => $"Hello, {name}, You are awesome!",
            "Ephra" => $"Hello, {name}, You are handsome!",
            "Esther" => $"Hello, {name}, You are cute!",
            null => "Hello, It is nice to meet you",
            "" => "Hello, It is nice to meet you",
            _ => "Hello, Bro/Sist, It looks like you are new here.",
        };
    }

    // public static string? GetName()
    // {
    //     Console.Write("What is your name? ");
    //     return Console.ReadLine();
    // }

    // public static void WriteMessage(string message)
    // {
    //     Console.WriteLine(message);
    // }
}