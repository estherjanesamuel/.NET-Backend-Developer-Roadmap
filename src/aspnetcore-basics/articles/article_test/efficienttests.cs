//https://goatreview.com/create-efficent-csharp-tests-with-xunit/
using Article.EfficienTests;

namespace article_test;
public class GoatServiceTests
{
    private readonly GoatService _goatService = new GoatService();

    [Fact]
    public void ShouldHaveJobInternship_WhenSetJobToGoat()
    {
        // Given
        var goat = new Goat();
        // When
        var goatWithJob = _goatService.SetJob(goat);
        // Then
        Assert.Equal(GoatJob.Intern, goatWithJob.CurrentJob);
    }

    //error xUnit1003 : Theory methods must have test data. Use InlineData, MemberData, or ClassData to provide test data for the Theory.
    /*
    [Theory]
    public void ShouldHaveJobInternship_WhenSetJobToGoat_Theory(int age = 1, GoatJob goatJob = GoatJob.Intern)
    {
        // Given
        var goat = new Goat() { Age = age };
        // When
        var goatWithJob = _goatService.SetJob(goat);
        // Then
        Assert.Equal(goatJob, goatWithJob.CurrentJob);
    }
    */

    [Theory]
    [InlineData(int.MinValue, GoatJob.Intern)]
    [InlineData(20, GoatJob.Intern)]
    [InlineData(21, GoatJob.Developer)]
    [InlineData(35, GoatJob.Developer)]
    [InlineData(36, GoatJob.TechLead)]
    [InlineData(int.MaxValue, GoatJob.SupremGod)]
    public void ShouldHaveDefineJob_WhenSetJobToGoat(int age, GoatJob goatJobExpected)
    {
        // Given
        var goat = new Goat() {Age = age};
        // When
        var goatWithJob = _goatService.SetJob(goat);
        // Then
        Assert.Equal(goatJobExpected, goatWithJob.CurrentJob);
    }


    [Theory]
    [ClassData(typeof(GoatLanguageExperienceClassData))]
    public void ShouldHaveDefineJob_WhenSetJobToGoat_UseClassData(LanguageExperience languageExperience, GoatJob goatJobExpected)
    {
        // Given
        var goat = new Goat() {Experience = languageExperience};
        // When
        var goatWithJob = _goatService.SetJobByExperience(goat);
        // Then
        Assert.Equal(goatJobExpected, goatWithJob.CurrentJob);
    }

    [Theory]
    [MemberData(nameof(GetGoatExperienceMemberData))]
    public void ShouldHaveDefineJob_WhenSetJobToGoat_UseMemberData(LanguageExperience languageExperience, GoatJob goatJobExpected)
    {
        // Given
        var goat = new Goat() {Experience = languageExperience};
        // When
        var goatWithJob = _goatService.SetJobByExperience(goat);
        // Then
        Assert.Equal(goatJobExpected, goatWithJob.CurrentJob);
    }

    public static IEnumerable<Object[]> GetGoatExperienceMemberData()
    {
        yield return new object[] {new LanguageExperience() {Name = "C#", YearOfPractice = int.MinValue}, GoatJob.Intern};
        yield return new object[] {new LanguageExperience() {Name = "C#", YearOfPractice = 3}, GoatJob.Intern};
        yield return new object[] {new LanguageExperience() {Name = "C#", YearOfPractice = 4}, GoatJob.Developer};
        yield return new object[] {new LanguageExperience() {Name = "C#", YearOfPractice = 10}, GoatJob.Developer};
        yield return new object[] {new LanguageExperience() {Name = "C#", YearOfPractice = 11}, GoatJob.TechLead};
        yield return new object[] {new LanguageExperience() {Name = "C#", YearOfPractice = int.MaxValue}, GoatJob.SupremGod};
    }
}