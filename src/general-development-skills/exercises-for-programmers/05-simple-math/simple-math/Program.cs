public class Program
{
    private static void Main(string[] args)
    {
        try
        {
            Console.Write("What is the first number? ");
            string? input1 = Console.ReadLine();
            int num1 = ConvertStringToInt(input1);
            num1 = CheckForNegative(num1);

            Console.Write("What is the second number? ");
            string? input2 = Console.ReadLine();
            int num2 = ConvertStringToInt(input2);
            num2 = CheckForNegative(num2);

            string output = $"{OutputAdd(num1,num2)}\n{OutputSubstract(num1,num2)}\n{OutputMultiply(num1,num2)}\n{OutputDivide(num1,num2)}";
            Console.WriteLine(output);
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine("Number can't be negative, Please re-run and try again.");
        }
        catch (Exception)
        {
            Console.WriteLine("Not a valid number, Please re-run and try again.");
        }
    }

    private static int CheckForNegative(int num1)
    {
        if (num1 < 0) throw new ArgumentOutOfRangeException(nameof(num1));
        return num1;
    }

    public static int Add(int x, int y)
    {
        return x + y;
    }

    public static string OutputAdd(int num1, int num2)
    {
        return $"{num1} + {num2} = {Add(num1, num2)}";
    }

    public static int Substract(int num1, int num2)
    {
        return num1 - num2;
    }

    public static string OutputSubstract(int num1, int num2)
    {
        return $"{num1} - {num2} = {Substract(num1, num2)}";
    }

    public static int Multiply(int num1, int num2)
    {
        return num1 * num2;
    }

    public static string OutputMultiply(int num1, int num2)
    {
        return $"{num1} * {num2} = {Multiply(num1, num2)}";
    }

    public static int Divide(int num1, int num2)
    {
        return num1 / num2;
    }

    public static string OutputDivide(int num1, int num2)
    {
        if (num1 == 0 || num2 == 0) return "Cannot divide by 0";
        return $"{num1} / {num2} = {Divide(num1, num2)}";
    }

    public static int ConvertStringToInt(string? input)
    {
        if (string.IsNullOrEmpty(input)) throw new ArgumentNullException(nameof(input));
        int output;
        if (!int.TryParse(input, out output))
        {
            throw new FormatException(nameof(input));
        }
        return output;
    }
}