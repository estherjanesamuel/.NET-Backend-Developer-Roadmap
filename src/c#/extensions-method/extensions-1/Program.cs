var goat = new Goat();
GoatUtils.AddGoadSkill(goat, "dotnet");
var level = GoatUtils.GetGoatLevel(goat);
Console.WriteLine("Goat level: {0}", level);
Console.WriteLine("Goat skills: [{0}]", string.Join(", ", goat.Skills));

goat.AddSkill("c#");
goat.AddSkill("blazor");
goat.AddSkill("ef");
goat.AddSkill("dapper");
goat.AddSkill("linq");
Console.WriteLine("Goat skills: [{0}]", string.Join(", ", goat.Skills));
Console.WriteLine("Goat level: {0}", goat.CurrentLevel());

goat.AddSkill("minimal api");
goat.AddSkill("mvc");
goat.AddSkill("iis");
goat.AddSkill("identity server");
Console.WriteLine("Goat skills: [{0}]", string.Join(", ", goat.Skills));
Console.WriteLine("Goat level: {0}", goat.CurrentLevel());
Console.WriteLine("Goat level: {0}", GoatUtils.GetGoatLevelByYearsExperience(new DateTime(2019,11,04)));

public record class Goat
{
    public List<string> Skills {get; set;} = new();
    public int YearsExperience { get; set; } = 0;
}
public enum GoatLevel
{
    Junior,
    Senior,
    God
}
public static class GoatExtensions
{
    public static void AddSkill(this Goat goat, string skill)
    {
        if (!goat.Skills.Contains(skill))
        {
            goat.Skills.Add(skill);
        }
    }

    public static GoatLevel CurrentLevel(this Goat goat)
    {
        switch (goat.Skills.Count())
        {
            case > 9:
                return GoatLevel.God;
            case > 5:
                return GoatLevel.Senior;
            default:
                return GoatLevel.Junior;
        }
    }
}
public static class GoatUtils
{
    public static void AddGoadSkill(Goat goat, string skill)
    {
        if (!goat.Skills.Contains(skill))
        {
            goat.Skills.Add(skill);
        }
    }

    public static GoatLevel GetGoatLevelByYearsExperience(DateTime startDate)
    {
        /*
            1 year is 12 month 
            how to compare now with the start date
        */
        var totalDays = DateTime.Today.Subtract(startDate).TotalDays;
        var daysToSenior = 365 * 3 - totalDays;
        Console.WriteLine("Need days to be a senior: {0} ", daysToSenior);
        switch (totalDays)
        {
            case < 365 * 3:
            return GoatLevel.Junior;
            case < 365 * 5:
            
            return GoatLevel.Senior;
            default:
            return GoatLevel.God;
        }
    }

    public static GoatLevel GetGoatLevel(Goat goat)
    {
        switch (goat.Skills.Count())
        {
            case > 9:
                return GoatLevel.God;
            case > 5:
                return GoatLevel.Senior;
            default:
                return GoatLevel.Junior;
        }
    }
}