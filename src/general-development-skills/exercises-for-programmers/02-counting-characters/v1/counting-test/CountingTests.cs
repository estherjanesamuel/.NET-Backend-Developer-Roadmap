using counting;

namespace counting_test;

public class Tests
{
    [Test]
    public void GetCharacterCount_WhenHello_Returns5()
    {
        string input = "hello";
        int want = 5;

        int got = Program.GetCharacterCount(input);

        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void GetCharacterCount_WhenNull_Returns0()
    {
        string? input = null;
        int want = 0;

        int got = Program.GetCharacterCount(input!);

        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void GetCharacterCount_WhenEmpty_Returns0()
    {
        string? input = "";
        int want = 0;

        int got = Program.GetCharacterCount(input!);

        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void GetOutputMessage_OnValid_ReturnsExpected()
    {
        string? input = "hello";
        int count = 5;
        string want = $"{input} has {count} characters.";

        string got = Program.GetOutputMessage(input!, count);

        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void GetOutputMessage_WhenNull_ReturnsO()
    {
        string? input = null;
        int count = Program.GetCharacterCount(input!);
        string want = $"{input} has {count} characters.";

        string got = Program.GetOutputMessage(input!, count);

        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void GetOutputMessage_WhenEmpty_Returns0()
    {
        string? input = "";
        int count = Program.GetCharacterCount(input);
        string want = $"{input} has {count} characters.";

        string got = Program.GetOutputMessage(input!, count);

        Assert.That(got, Is.EqualTo(want));
    }
}
