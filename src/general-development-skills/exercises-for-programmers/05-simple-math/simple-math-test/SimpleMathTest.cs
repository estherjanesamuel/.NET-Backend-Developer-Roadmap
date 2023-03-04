namespace simple_math_test;

public class Tests
{
    [Test]
    public void Add_On2Numbers_ReturnSum()
    {
        int num1 = 10;
        int num2 = 5;
        int want = 15;

        int got = Program.Add(num1, num2);
        
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void OutputAdd_On2Numbers_ReturnSumString()
    {
        int num1 = 10;
        int num2 = 5;
        string want = "10 + 5 = 15";

        string got = Program.OutputAdd(num1, num2);
        
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void OutputSubstract_On2Numbers_ReturnDifference()
    {
        int num1 = 10;
        int num2 = 5;
        int want = 5;

        int got = Program.Substract(num1, num2);
        
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void Multiply_On2Numbers_ReturnProduct()
    {
        int num1 = 10;
        int num2 = 5;
        int want = 50;

        int got = Program.Multiply(num1, num2);
        
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void OutputMultiply_On2Numbers_ReturnProductString()
    {
        int num1 = 10;
        int num2 = 5;
        string want = $"10 * 5 = 50";

        string got = Program.OutputMultiply(num1, num2);
        
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void Divide_On2Numbers_ReturnResult()
    {
        int num1 = 10;
        int num2 = 5;
        int want = 2;

        int got = Program.Divide(num1, num2);
        
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void OutputDivide_On2Numbers_ReturnResultString()
    {
        int num1 = 10;
        int num2 = 5;
        string want = $"10 / 5 = 2";

        string got = Program.OutputDivide(num1, num2);
        
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void OutputDivide_On0_ReturnErrorString()
    {
        int num1 = 10;
        int num2 = 0;
        string want = $"Cannot divide by 0";

        string got = Program.OutputDivide(num1, num2);
        
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void ConvertStringToInt_OnNumber_ReturnNumber()
    {
        string input = "10";
        int want = 10;

        int got = Program.ConvertStringToInt(input);
        
        Assert.That(got, Is.EqualTo(want));
    }


    [Test]
    public void ConvertStringToInt_OnPaddedNumber_ReturnNumber()
    {
         string input = "   10   ";
        int want = 10;

        int got = Program.ConvertStringToInt(input);
        
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void ConvertStringToInt_OnNonNumber_ThrowsFormatException()
    {
        string input = "hello";
        Assert.Throws<FormatException>(() => Program.ConvertStringToInt(input));
    }

    [Test]
    public void ConvertStringToInt_OnNull_ThrowsArgumentNullException()
    {
        string? input = null;
        Assert.Throws<ArgumentNullException>(() => Program.ConvertStringToInt(input));
    }

    [Test]
    public void ConvertStringToInt_OnBlank_ThrowsArgumentNullException()
    {
        string? input = "";
        Assert.Throws<ArgumentNullException>(() => Program.ConvertStringToInt(input));
    }
}