namespace quotes;

/*
what is the quote? these aren't the droids you're looking for.
who said it? Obi-Wan Kenobi
Obi-Wan Kenobi says, "These aren't the droids you're looking for."

v2
Constraint
Single output statement
use string concatetenation (not interpolation)

challenge
Instead of prompting for input, use a data structure
and print out all of the items in the quote format.
*/
public class Program
{
    private static void Main(string[] args)
    {
        /* V1
        Console.Write("What is the quote? ");
        string? quote = Console.ReadLine();
        Console.Write("Who said it? ");
        string? author = Console.ReadLine();
        string message = GetOutput(quote, author);
        */
        try
        {
            foreach (var (quote, author) in quotes)
            {
                string? message = GetOutput(quote,author);
                Console.WriteLine(message);
            }
        }
        catch (System.Exception)
        {
            Console.WriteLine("Sorry, I COuld not process your answers, Please try again.");
        }
    }

    public static string GetOutput(string? quote, string? author)
    {
        if (string.IsNullOrEmpty(quote) || string.IsNullOrWhiteSpace(quote)) throw new ArgumentNullException(nameof(quote));
        if (string.IsNullOrEmpty(author) || string.IsNullOrWhiteSpace(author)) throw new ArgumentNullException(nameof(author));
        
        return author + " says, \"" + quote + "\"";
    }

    public static List<(string quote, string author)> quotes = new()
    {
        ("These aren't the droids you're looking for.","Obi-Wan Kenobi"),
        ("I love you","Leia Organa"),
        ("I know","Han Solo"),
    };
}