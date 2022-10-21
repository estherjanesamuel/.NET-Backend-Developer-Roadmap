var goat = new Goat();
GoatUtils.AddGoadSkill(goat, "dotnet");
var level = GoatUtils.GetGoatLevel(goat);
Console.WriteLine("Goat level: {0}", level);

goat.AddSkill("c#");
goat.AddSkill("blazor");
goat.AddSkill("ef");
goat.AddSkill("dapper");
goat.AddSkill("linq");
Console.WriteLine("Goat skills: [{0}]", string.Join(", ", goat.Skills));
Console.WriteLine("Goat level: {0}", goat.CurrentLevel());


public record class Goat
{
    public List<string> Skills {get; set;} = new();
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