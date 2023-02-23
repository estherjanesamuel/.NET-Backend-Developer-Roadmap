
namespace counting
{
    public class Program
    {
        private static void Main()
        {
            //v1
            Console.Write("what is the input string? ");
            string? inputV1 = Console.ReadLine();
            int countV1 = GetCharacterCount(inputV1!);
            string messageV1 = GetOutputMessage(inputV1!, countV1);
            Console.WriteLine(messageV1);


            // v2
            string? input = null;
            do
            {
                Console.Write("what is the input string? ");
                input = Console.ReadLine();

                if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
                    Console.WriteLine("please enter a valid value.");
            } while (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input));

            int count = GetCharacterCount(input);
            string message = GetOutputMessage(input!, count);
            Console.WriteLine(message);
        }

        public static int GetCharacterCount(string input)
        {
            if (string.IsNullOrEmpty(input))
                return 0;

            return input.Length;
        }

        public static string GetOutputMessage(string input, int count)
        {
            if (string.IsNullOrEmpty(input))
                input = string.Empty;

            return $"{input} has {count} characters.";
        }
    }

}
