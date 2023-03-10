using System.Runtime.Serialization;

public class Program
{
    private static void Main(string[] args)
    {
        try
        {
            Console.Write("What is your current age? " );
            string? input1 = Console.ReadLine();
            int currentAge = ConvertStringToInt(input1);
            currentAge = CheckForNegative(currentAge);

            Console.Write("At what age would you like to retire? " );
            string? input2 = Console.ReadLine();
            int retirementAge = ConvertStringToInt(input2);
            retirementAge = CheckForNegative(retirementAge);

            int yearsToRetirement = CalcYearsToRetirement(currentAge, retirementAge);
            int currentYear = GetCurrentYear();
            int retirementYear = GetRetirementYear(currentYear, yearsToRetirement);

            string output =string.Format("You have {0} years left until you can retire.\n", yearsToRetirement);
            output += string.Format("It's {0}, so you can retire in {1}", currentYear, retirementYear);

            Console.WriteLine(output);
        }
        catch (RetirementException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine("number can't be zero or negative. please re-run and try again.");
        }
        catch (Exception)
        {
            Console.WriteLine("not a valid number. please re-run and try again.");
        }
    }

    private static int CheckForNegative(int input)
    {
        if (input < 0) throw new ArgumentOutOfRangeException(nameof(input));
        return input;
    }

    public static int GetCurrentYear()
    {
        return DateTime.Now.Year;
    }

    public static int CalcYearsToRetirement(int currentAge, int retirementAge)
    {
        int yearsToRetirement = retirementAge - currentAge;
        if ( retirementAge == 0) throw new ArgumentOutOfRangeException(nameof(retirementAge));
        if ( yearsToRetirement <= 0) throw new RetirementException("you are already retired");
        return yearsToRetirement;
    }

    public static int GetRetirementYear(int currentYear, int yearsToRetirement)
    {
        int retirementYear = currentYear + yearsToRetirement;
        if (yearsToRetirement <= 0) throw new RetirementException("you are already retired");
        return retirementYear;
    }

    public static int ConvertStringToInt(string? input)
    {
        int output;
        if (string.IsNullOrEmpty(input)) throw new ArgumentNullException(nameof(input));
        if (!int.TryParse(input, out output)) throw new FormatException(nameof(input));
        return output;
    }
}

[Serializable]
public class RetirementException : Exception
{
    public RetirementException() : base() {}

    public RetirementException(string? message) : base(message){ }

    public RetirementException(string? message, Exception? innerException) : base(message, innerException) { }

    protected RetirementException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}