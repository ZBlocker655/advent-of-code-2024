namespace AdventOfCode;

public sealed class Day05: BaseDay
{
    private readonly string[] _lines;
    private HashSet<(int, int)> _rules;
    private int[][] _updates;
    
    public Day05()
    {
        _lines = File.ReadAllLines(InputFilePath);
        ProcessLines();
    }

    private void ProcessLines()
    {
        int blank = FindBlankIndex();
        
        _rules = _lines[..blank]
            .Select(l => l.Split('|'))
            .Select(x => (int.Parse(x[0]), int.Parse(x[1])))
            .ToHashSet();

        _updates = _lines[(blank + 1)..]
            .Select(l => l.Split(','))
            .Select(x => Array.ConvertAll(x, int.Parse))
            .ToArray();
    }

    private int FindBlankIndex()
    {
        // Use binary search to find the index of the blank line between the rules section and updates section.
        int start = 0, end = _lines.Length - 1;
        int test;

        while (start < end)
        {
            test = ((end - start) / 2) + start;
            if (string.IsNullOrWhiteSpace(_lines[test])) return test;
            if (_lines[test].Contains('|'))
            {
                start = test;
            }
            else
            {
                end = test;
            }
        }
        
        return start;
    }
    
    public override ValueTask<string> Solve_1() => new($"{CorrectUpdates_MiddlePageNumberSum()}");

    public override ValueTask<string> Solve_2() => new($"{IncorrectUpdates_MiddlePageNumberSum()}");

    private int CorrectUpdates_MiddlePageNumberSum() =>
        _updates
            .Where(IsUpdatePageOrderCorrect)
            .Select(MiddlePageNumber)
            .Sum();
    
    private int IncorrectUpdates_MiddlePageNumberSum() =>
        _updates
            .Where(u => !IsUpdatePageOrderCorrect(u))
            .Select(ReorderUpdate)
            .Select(MiddlePageNumber)
            .Sum();

    private static int MiddlePageNumber(int[] u) => u[u.Length / 2];

    private bool IsUpdatePageOrderCorrect(int[] update)
    {
        for (int i = 0; i < update.Length - 1; i++)
            for (int j = i + 1; j < update.Length; j++)
                if (_rules.Contains((update[j], update[i])))
                    return false;
        return true;
    }

    private int[] ReorderUpdate(int[] update)
    {
        var updateList = update.ToList();
        updateList.Sort(ComparePages);
        return updateList.ToArray();
    }
    
    private int ComparePages(int a, int b)
    {
        if (_rules.Contains((a, b))) return -1;
        if (_rules.Contains((b, a))) return 1;
        return 0;
    }
}