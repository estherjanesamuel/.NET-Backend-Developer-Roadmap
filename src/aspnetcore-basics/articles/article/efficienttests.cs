//https://goatreview.com/create-efficent-csharp-tests-with-xunit/
namespace Article.EfficienTests;

public class Goat
{
    public int Age { get; set; } = 0;
    public GoatJob CurrentJob { get; set; } = GoatJob.Unassigned;
    public LanguageExperience Experience { get; set; } = new LanguageExperience() {Name = "C#", YearOfPractice = 0};
}

public class LanguageExperience
{
    public string Name { get; set; } = string.Empty;
    public int YearOfPractice { get; set; } = 0;
}

public enum GoatJob
{
    Unassigned,
    Intern,
    Developer,
    TechLead,
    SupremGod
}

public class GoatService
{
    public Goat SetJob(Goat goat)
    {
        goat.CurrentJob = goat.Age switch
        {
            > 45 => GoatJob.SupremGod,
            > 35 => GoatJob.TechLead,
            > 20 => GoatJob.Developer,
            _ => GoatJob.Intern
        };
        return goat;
    }

    public Goat SetJobByExperience(Goat goat)
    {
        goat.CurrentJob = goat.Experience.YearOfPractice switch
        {
            > 20 => GoatJob.SupremGod,
            > 10 => GoatJob.TechLead,
            > 3 => GoatJob.Developer,
            _ => GoatJob.Intern
        };
        return goat;
    }
}