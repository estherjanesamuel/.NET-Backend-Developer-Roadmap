using hello;

namespace hello_test;

public class Tests
{
    [Test]
    public void GetMessage_WithName_ReturnMessageWithName()
    {
        string? name = "ariefs";
        string expected = "Hello, ariefs, nice to meet you!";

        string message = Program.GetMessage(name);

        Assert.That(message, Is.EqualTo(expected));
    }
    [Test]
    public void GetMessage_WithNullName_ReturnsDefaultMessage()
    {
        string? name = null;
        string want = "Hello, It is nice to meet you!";

        string got = Program.GetMessage(name);
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void GetMessage_WithEmptyName_ReturnsDefaultMessage()
    {
        string? name = "";
        string want = "Hello, It is nice to meet you!";

        string got = Program.GetMessage(name);
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void GetMessage_WithAriefs_ReturnMessageWithName()
    {
        string? name = "Ariefs";
        string want = "Hello, Ariefs, How's it going?";

        string got = Program.GetMessageWithName(name);
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void GetMessage_WithJuita_ReturnMessageWithName()
    {
        string? name = "Juita";
        string want = "Hello, Juita, You are awesome!";

        string got = Program.GetMessageWithName(name);
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void GetMessage_WithEphra_ReturnMessageWithName()
    {
        string? name = "Ephra";
        string want = "Hello, Ephra, You are handsome!";

        string got = Program.GetMessageWithName(name);
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void GetMessage_WithEsther_ReturnMessageWithName()
    {
        string? name = "Esther";
        string want = "Hello, Esther, You are cute!";

        string got = Program.GetMessageWithName(name);
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void GetMessage_WithNullName_ReturnMessageWithName()
    {
        string? name = null;
        string want = "Hello, It is nice to meet you";

        string got = Program.GetMessageWithName(name);
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void GetMessage_WithEmptyName_ReturnMessageWithName()
    {
        string? name = "";
        string want = "Hello, It is nice to meet you";

        string got = Program.GetMessageWithName(name);
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void GetMessage_WithOtherName_ReturnMessageWithName()
    {
        string? name = "Bro/Sist";
        string want = $"Hello, {name}, It looks like you are new here.";

        string got = Program.GetMessageWithName(name);
        Assert.That(got, Is.EqualTo(want));
    }
}