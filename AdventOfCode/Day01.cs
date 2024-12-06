namespace AdventOfCode;

public sealed class Day01: BaseDay
{
    private readonly string _input;

    public Day01()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1() => new($"{Solve1()}");

    public override ValueTask<string> Solve_2() => new($"{Solve2()}");

    private (int, int)[] RawLists() =>
        _input
            .Split('\n')
            .Select(s => s.Split("   "))
            .Select(s => (int.Parse(s[0]), int.Parse(s[1])))
            .ToArray();
    
    private int Solve1()
    {
        var rawLists = RawLists();

        var list1 = rawLists.Select(l => l.Item1).OrderBy(n => n);
        var list2 = rawLists.Select(l => l.Item2).OrderBy(n => n);

        return list1
            .Zip(list2, (a, b) => Math.Abs(a - b))
            .Sum();
    }

    private int Solve2()
    {
        var rawLists = RawLists();
        var list1 = rawLists.Select(l => l.Item1).ToArray();
        var list2 = rawLists.Select(l => l.Item2).ToArray();
        
        var list2Counts = list2
            .GroupBy(n => n)
            .ToDictionary(g => g.Key, g => g.Count());

        return list1
            .Select(n => n * list2Counts.GetValueOrDefault(n, 0))
            .Sum();
    }
}
