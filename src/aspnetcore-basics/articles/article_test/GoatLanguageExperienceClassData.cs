using System.Collections;
using Article.EfficienTests;

namespace article_test;

public class GoatLanguageExperienceClassData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] {new LanguageExperience() {Name = "C#", YearOfPractice = int.MinValue}, GoatJob.Intern};
        yield return new object[] {new LanguageExperience() {Name = "C#", YearOfPractice = 3}, GoatJob.Intern};
        yield return new object[] {new LanguageExperience() {Name = "C#", YearOfPractice = 4}, GoatJob.Developer};
        yield return new object[] {new LanguageExperience() {Name = "C#", YearOfPractice = 10}, GoatJob.Developer};
        yield return new object[] {new LanguageExperience() {Name = "C#", YearOfPractice = 11}, GoatJob.TechLead};
        yield return new object[] {new LanguageExperience() {Name = "C#", YearOfPractice = int.MaxValue}, GoatJob.SupremGod};
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
