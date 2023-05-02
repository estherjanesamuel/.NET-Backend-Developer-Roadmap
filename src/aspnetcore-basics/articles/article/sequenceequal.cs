//https://goatreview.com/mastering-sequenceequal-csharp/
using System.Diagnostics.CodeAnalysis;

namespace Article.SequenceEqual;

public class Goat
{
    public string Name { get; set; }
    public int Age { get; set; }

    public override string ToString()
    {
        return $"Goat name : {Name}, and age : {Age}!";
    }
}

public class GoatSkillEqualityComparer : IEqualityComparer<Goat>
{
    public bool Equals(Goat? x, Goat? y)
    {
        if (x is null || y is null) return false;

        return x.Name == y.Name && x.Age == y.Age;
    }

    public int GetHashCode([DisallowNull] Goat obj)
    {
        return obj.GetHashCode();
    }
}