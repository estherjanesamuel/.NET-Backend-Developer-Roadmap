# xUnit

Among all C# test libraries, xUnit is one of the most used by .NET developers. It's simple to use, and that's the subject of this post: How to use xUnit attributes to create powerful tests?

Imagine a parallel world, where goats are brilliant developers, who start learning in school to become tech lead or even more, a supreme god for all other developers, even humans.

public class Goat
{
    public int Age { get; set; } = 0;
    public GoatJob CurrentJob { get; set; } = GoatJob.Unassigned;
}

public enum GoatJob
{
    Unassigned = 0,
    Intern = 1,
    Developer = 2,
    TechLead = 3,
    SupremGod = 4
}
As humain, goat will have a job defined by the GoatService. For the moment, all goats will be interns.

public class GoatService
{
    public Goat SetJob(Goat goat)
    {
        goat.CurrentJob = GoatJob.Intern;
        return goat;
    }
}
All the code is set, perfect! Now it's time to test it!

public class GoatServiceTests
{
    private readonly GoatService _goatService = new GoatService();
}
Note here the instantiation of the GoatService in the class. Each property will be instantiated for each test, in the same way as in the constructor, the two are equivalent. It's best to keep the class as simple as possible.

Like most test libraries, xUnit needs to define attributes on method tests. These are divided into two categories: Fact and Theory. Test methods with Fact describe the test without any parameters. Those with Theory expect some input data to run the test.

Attribute Fact: test without parameters
Using the Fact attribute is the best way to start a test. It gets you to focus on the behavior of the tests and what is being tested, instead of trying to refactor or having a preconceived notion of on how the tests should be built before running them.

Remember that tests are incremental. Start as simple as possible, code without complexity is much easier to maintain. It is a good practice to start with this attribute if you don't already know the cases you are going to test.

Currently, goats can only have one job: intern.

public class GoatServiceTests
{
    private readonly GoatService _goatService = new GoatService();

    [Fact]
    public void ShouldHaveJobInternship_WhenSetJobToGoat()
    {
        // Assign
        var goat = new Goat();

        // Act
        var goatWithJob = _goatService.SetJob(goat);

        // Assert
        Assert.Equal(GoatJob.Intern, goatWithJob.CurrentJob);
    }
}
The test passed, great!

Now, instead of basing the position on actual experience and side projects, let's say it follows a curve based on the age of the goat. If the goat is under 21, it's an internship, between 21 and 35 a developer and a tech lead after that.

public Goat SetJob(Goat goat)
{
    goat.CurrentJob = goat.Age switch
    {
        > 35 => GoatJob.TechLead,
        > 20 => GoatJob.Developer,
        _ => GoatJob.Internship
    };
    return goat;
}
Attribute Theory: test with parameters
The test is updated to use the attribute Theory with parameters to define the age of the current goat and the work it is supposed to have.

[Theory]
public void ShouldHaveDefinedJob_WhenSetJobToGoat(int age = 1, GoatJob goatJob = GoatJob.Intern)
{
    // Assign
    var goat = new Goat() { Age = age };

    // Act
    var goatWithJob = _goatService.SetJob(goat);

    // Assert
    Assert.Equal(goatJob, goatWithJob.CurrentJob);
}
If you think directly about duplicating this test, assigning the age in the goat constructor and updating the assertions, you are going in the wrong direction. What you'll be creating are unmaintainable tests, because all the tests created validate exactly the same thing, the use case of the SetJob function.

Furthermore, the above code should raise the following error:

Theory methods must have test data. Use InlineData, MemberData, or ClassData to provide test data for the Theory

The Theory attribute cannot be used alone, but only in combination with one of the following attributes: InlineData, MemberData or ClassData. Beware of the or, the attributes can not be mixed.

In order to describe each of them, we will do some incremental refactoring tests.

InlineData: primitive types
InlineData is the first way to add parameters, directly inline just above the Fact attribute. You can add parameters with private types or enums, but neither instantiated objects nor statics. This is mainly used to validate values more than to compare objects.

Parameters are unlimited, but if you have more than 3 or 4, it means your test is doing too many things at once. Split it up!

InlineData is needed here to validate all the boundary cases of the use case.

[Theory]
[InlineData(int.MinValue, GoatJob.Intern)]
[InlineData(20, GoatJob.Intern)]
[InlineData(21, GoatJob.Developer)]
[InlineData(35, GoatJob.Developer)]
[InlineData(36, GoatJob.TechLead)]
[InlineData(int.MaxValue, GoatJob.TechLead)]
public void ShouldHaveDefinedJob_WhenSetJobToGoat(int age, GoatJob goatJobExpected)
{
    // Assign
    var goat = new Goat() { Age = age };

    // Act
    var goatWithJob = _goatService.SetJob(goat);

    // Assert
    Assert.Equal(goatJobExpected, goatWithJob.CurrentJob);
}
Okay, keep imagining: times are tough. Work is no longer based on age, but on language experience (they only learn one language at the moment). If they have less than 3 years experience, it's an internship, between 3 and 10 years a developer and tech lead after that.

If you thought about instantiating the LanguageExperience object inside the InlineData like this [InlineData(new LanguageExperience("C#", 10), GoatJob.Developer)], you can't. As said before, no instantiation inside.

ClassData: instanciated objects
ClassData attribute also to set all kind of objects as parameters for the test.

Before going further, let's update the entities.

The LanguageExperience class will associate a language name with the year of practice.

public class LanguageExperience
{
    public string Name { get; set; } = string.Empty;
    public int YearOfPractice { get; set; } = 0;
}
The property is also added to the Goat class.

public LanguageExperience? Experience { get; set; }
I leave it to you to update the SetJob function, it is largely within your reach.

In this case, the LanguageExperience object will need to be instantiated in the parameters, then assigned to the goat in the test.

The ClassData attribute needs the type of  have the class type set to provide parameters, like this:

[Theory]
[ClassData(typeof(GoatLanguageExperienceClassData))]
public void ShouldHaveDefinedJob_WhenSetJobToGoat(LanguageExperience languageExperience, GoatJob goatJobExpected)
{
    // Assign
    var goat = new Goat() { Experience = languageExperience };

    // Act
    var goatWithJob = _goatService.SetJob(goat);

    // Assert
    Assert.Equal(goatJobExpected, goatWithJob.CurrentJob);
}
Since it is called ClassData, we have to create a class with our data (smart right ?).

public class GoatLanguageExperienceClassData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { new LanguageExperience() { Name = "C#", YearOfPractice = int.MinValue }, GoatJob.Intern };
        yield return new object[] { new LanguageExperience() { Name = "C#", YearOfPractice = 3 }, GoatJob.Intern };
        yield return new object[] { new LanguageExperience() { Name = "C#", YearOfPractice = 4 }, GoatJob.Developer };
        yield return new object[] { new LanguageExperience() { Name = "C#", YearOfPractice = 10 }, GoatJob.Developer };
        yield return new object[] { new LanguageExperience() { Name = "C#", YearOfPractice = 11 }, GoatJob.TechLead };
        yield return new object[] { new LanguageExperience() { Name = "C#", YearOfPractice = int.MaxValue }, GoatJob.TechLead };
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
The GoatLanguageExperienceClassData class inherits from IEnumerable<object[]> to implement the GetEnumerator() function. Inside, the parameters are defined in a new object[], even instantiated objects like LanguageExperience.

You can see here that the return is object[], so technically you can use whatever you need to test your function.

This class can be duplicated like this. The only thing that needs to be changed is the content of each  object[] and the class name.

MemberData: statics
Ok, so what about MemberData?

It is used to classify the statics of any element: property, field, or method.

I must admit that I don't use this attribute. It can be compared to ClassData, but without creating an additional class.

Also, I rarely create statics in my tests to validate a use case. Either I create a constant to have InlineData, or I use an external method with ClassData.

First, create a static method returning an IEnumerable<object[]> (be careful, this is not an IEnumerator<object[]>). You can literally copy the contents of ClassData.

public static IEnumerable<object[]> GetGoatExperienceMemberData()
{
    yield return new object[] { new LanguageExperience() { Name = "C#", YearOfPractice = int.MinValue }, GoatJob.Intern };
    yield return new object[] { new LanguageExperience() { Name = "C#", YearOfPractice = 3 }, GoatJob.Intern };
    yield return new object[] { new LanguageExperience() { Name = "C#", YearOfPractice = 4 }, GoatJob.Developer };
    yield return new object[] { new LanguageExperience() { Name = "C#", YearOfPractice = 10 }, GoatJob.Developer };
    yield return new object[] { new LanguageExperience() { Name = "C#", YearOfPractice = 11 }, GoatJob.TechLead };
    yield return new object[] { new LanguageExperience() { Name = "C#", YearOfPractice = int.MaxValue }, GoatJob.TechLead };
}
MemberData is linked to a name, in this case GetGoatExperienceMemberData.

[Theory]
[MemberData(nameof(GetGoatExperienceMemberData))]
public void ShouldHaveDefinedJob_WhenSetJobToGoat2(LanguageExperience languageExperience, GoatJob goatJobExpected)
{
    // Assign
    var goat = new Goat() { Experience = languageExperience };

    // Act
    var goatWithJob = _goatService.SetJob(goat);

    // Assert
    Assert.Equal(goatJobExpected, goatWithJob.CurrentJob);
}
As you can see, it's really close to ClassData. I recommend separating the data from the tests, to make it cleaner and easier to find.

The cleaner you make your tests, the easier it will be to maintain and refactor them! Always keep this in mind.

Conclusion
xUnit is a powerful library for testing, and easy enough to define methods with and without parameters:

Fact without parameters
Theory - InlineData for primitive types
Theory - MemberData for static fields
Theory - ClassData for instantiated objects
This way, your tests will be refactored, easier to maintain and quite fast to run!