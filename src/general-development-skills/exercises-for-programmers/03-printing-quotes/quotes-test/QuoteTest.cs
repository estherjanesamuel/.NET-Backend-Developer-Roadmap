using quotes;
namespace quotes_test;

public class Tests
{
    [Test]
    public void GetOutput_OnEntry_returnQuoteAndAuthor()
    {
        string quote= "These aren't the droids you're looking for.";
        string author = "Obi-Wan Kenobi";
        string want = "Obi-Wan Kenobi says, \"These aren't the droids you're looking for.\"";

        string got = Program.GetOutput(quote, author);

        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void GetOutput_OnNUllQuote_Errors()
    {
        string? quote= null;
        string author = "Obi-Wan Kenobi";
        Assert.Throws<ArgumentNullException>(() =>  Program.GetOutput(quote, author));
    }

    [Test]
    public void GetOutput_OnEmptyQuote_Errors()
    {
        string? quote= "";
        string author = "Obi-Wan Kenobi";
        Assert.Throws<ArgumentNullException>(() =>  Program.GetOutput(quote, author));
    }

    [Test]
    public void GetOutput_OnNullAuthor_Errors()
    {
        string quote = "These aren't the droids you're looking for.";

        string author = null!;
        Assert.Throws<ArgumentNullException>(() =>  Program.GetOutput(quote, author));
    }

    [Test]
    public void GetOutput_OnBlankAuthor_Errors()
    {
        string quote = "These aren't the droids you're looking for.";
        string author = "";
        Assert.Throws<ArgumentNullException>(() =>  Program.GetOutput(quote, author));
    }

    [Test]
    public void GetOutput_OnWhiteSpacesQuote_Errors()
    {
        string? quote= "      ";
        string author = "Obi-Wan Kenobi";
        Assert.Throws<ArgumentNullException>(() =>  Program.GetOutput(quote, author));
    }

    [Test]
    public void GetOutput_OnWhiteSpacesAuthor_Errors()
    {
        string quote = "These aren't the droids you're looking for.";
        string? author= "      ";
        Assert.Throws<ArgumentNullException>(() =>  Program.GetOutput(quote, author));
    }
}