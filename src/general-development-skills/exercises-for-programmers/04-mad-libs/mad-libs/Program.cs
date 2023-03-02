public class Program
{
    private static void Main(string[] args)
    {
        Console.Write("Enter a noun: ");
        string? noun = Console.ReadLine();
        Console.Write("Enter a verb: ");
        string? verb = Console.ReadLine();

        Console.Write("Enter an adjective: ");
        string? adjective = Console.ReadLine();

        Console.Write("Enter a adverb: ");
        string? adverb = Console.ReadLine();

        try
        {
            string output = GetOutput(noun!,verb!, adjective!, adverb!);
            Console.WriteLine(output);
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"You did not enter a {ex.ParamName}. Please try again.");
        }
    }
    public static string GetOutput(string? noun, string? verb, string? adjective, string? adverb)
    {
        string GetAdjective()
        {
            if (string.IsNullOrEmpty(adjective))
                return "";
            return $"{adjective} ";
        }
        string GetAdverb()
        {
            if (string.IsNullOrEmpty(adverb))
                return "";
            return $" {adverb}";
        }
        if (string.IsNullOrEmpty(noun)) throw new ArgumentNullException(nameof(noun));
        if (string.IsNullOrEmpty(verb)) throw new ArgumentNullException(nameof(verb));

        return $"Do you {verb.Trim()} your {GetAdjective()}{noun.Trim()}{GetAdverb()}? That's hilarious!";
    }
}