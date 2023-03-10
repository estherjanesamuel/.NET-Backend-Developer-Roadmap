namespace retirement_calculator_test;

public class Tests
{
    [Test]
    public void GetCurrentYear_ReturnsCurrentYear()
    {
        int want = DateTime.Now.Year;
        int got = Program.GetCurrentYear();
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void YearsToRetirement_OnPositive_ReturnsExpectedYears()
    {
        int want = 40;
        int currentAge = 25;
        int retirementAge = 65;
        int got = Program.CalcYearsToRetirement(currentAge, retirementAge);
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void YearsToRetirement_OnNegative_ThrowsRetiredException()
    {
        int currentAge = 70;
        int retirementAge = 65;
        Assert.Throws<RetirementException>(() => Program.CalcYearsToRetirement(currentAge, retirementAge));
    }

    [Test]
    public void RetirementYear_OnPositive_ReturnsExpectedYears()
    {
        int want = 2055;
        int currentYear = 2015;
        int yearsToRetirement = 40;
        int got = Program.GetRetirementYear(currentYear, yearsToRetirement);
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void RetirementYear_OnNegative_ThrowsRetiredException()
    {
        int currentAge = 2015;
        int retirementAge = 0;
        Assert.Throws<RetirementException>(() => Program.GetRetirementYear(currentAge, retirementAge));
    }

    [Test]
    public void ConvertStringToInt_OnPaddedNumber_ReturnsNumber()
    {
        string input = "    10  ";
        int want = 10;
        int got = Program.ConvertStringToInt(input);
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void ConvertStringToInt_OnNaN_ReturnsNumber()
    {
        string input = "hello";
        Assert.Throws<FormatException>(() => Program.ConvertStringToInt(input));
    }

     [Test]
    public void ConvertStringToInt_OnNull_ReturnsNumber()
    {
        string? input = null;
        Assert.Throws<ArgumentNullException>(() => Program.ConvertStringToInt(input));
    }

    [Test]
    public void ConvertStringToInt_OnEmpty_ReturnsNumber()
    {
        string? input = "";
        Assert.Throws<ArgumentNullException>(() => Program.ConvertStringToInt(input));
    }
}
