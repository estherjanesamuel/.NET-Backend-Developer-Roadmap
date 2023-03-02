namespace mad_libs_test;

public class Tests
{
    [Test]
    public void GetOutPut_WithAllWorlds_ReturnsPhrase()
    {
        string noun = "dog";
        string verb = "walk";
        string adjective = "blue";
        string adverb = "quickly";
        string want = "Do you walk your blue dog quickly? That's hilarious!";
        string got = Program.GetOutput(noun, verb, adjective,adverb);
        
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void GetOutPut_WithMissingNoun_ThrowException()
    {
        string noun = "";
        string verb = "walk";
        string adjective = "blue";
        string adverb = "quickly";
        
        Assert.Throws<ArgumentNullException>(() => Program.GetOutput(noun,verb, adjective, adverb));
    }

    [Test]
    public void GetOutPut_WithNullNoun_ThrowException()
    {
        string? noun = null;
        string verb = "walk";
        string adjective = "blue";
        string adverb = "quickly";
        
        Assert.Throws<ArgumentNullException>(() => Program.GetOutput(noun!,verb, adjective, adverb));
    }

    [Test]
    public void GetOutPut_WithMissingVerb_ThrowException()
    {
        string noun = "dog";
        string verb = "";
        string adjective = "blue";
        string adverb = "quickly";
        
        Assert.Throws<ArgumentNullException>(() => Program.GetOutput(noun,verb, adjective, adverb));
    }

    [Test]
    public void GetOutPut_WithNullVerb_ThrowException()
    {
        string noun = "dog";
        string? verb = null;
        string adjective = "blue";
        string adverb = "quickly";
        
        Assert.Throws<ArgumentNullException>(() => Program.GetOutput(noun,verb!, adjective, adverb));
    }

    [Test]
    public void GetOutPut_WithMissingAdjective_ReturnsPhrase()
    {
        string noun = "dog";
        string verb = "walk";
        string adjective = "";
        string adverb = "quickly";
        string want = "Do you walk your dog quickly? That's hilarious!";
        string got = Program.GetOutput(noun, verb, adjective,adverb);
        
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void GetOutPut_WithMissingAdverb_ReturnsPhrase()
    {
        string noun = "dog";
        string verb = "walk";
        string adjective = "blue";
        string adverb = "";
        string want = "Do you walk your blue dog? That's hilarious!";
        string got = Program.GetOutput(noun, verb, adjective,adverb);
        
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void GetOutPut_WithPaddedNoun_ReturnsPhrase()
    {
        string noun = "     dog     ";
        string verb = "walk";
        string adjective = "blue";
        string adverb = "quickly";
        
        string want = "Do you walk your blue dog quickly? That's hilarious!";
        string got = Program.GetOutput(noun, verb, adjective,adverb);
        
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public void GetOutPut_WithPaddedVerb_ReturnsPhrase()
    {
        string noun = "dog";
        string verb = "     walk    ";
        string adjective = "blue";
        string adverb = "quickly";
        
        string want = "Do you walk your blue dog quickly? That's hilarious!";
        string got = Program.GetOutput(noun, verb, adjective,adverb);
        
        Assert.That(got, Is.EqualTo(want));
    }

}