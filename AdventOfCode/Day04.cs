using System.Text.RegularExpressions;

namespace AdventOfCode;

public sealed class Day04: BaseDay
{
    private const string Target1 = "XMAS";
    private const string Target2 = "MAS";
    
    private readonly string[] _lines;
    private int _width, _height;
    
    public Day04()
    {
        _lines = File.ReadAllLines(InputFilePath);
        _width = _lines[0].Length;
        _height = _lines.Length;
    }
    
    public override ValueTask<string> Solve_1() => new($"{TargetCount(Target1)}");

    public override ValueTask<string> Solve_2() => new($"{XMatchCount(Target2)}");

    private int TargetCount(string target) =>
        TargetStartLocations(target)
            .Select(loc => MatchesFromLocation(loc, target))
            .Sum();

    private int XMatchCount(string target)
    {
        int count = 0;
        
        for (int x = 0; x <= _width - target.Length; x++)
        for (int y = 0; y <= _height - target.Length; y++)
        {
            if (IsXMatch((x, y), target)) count++;
        }
        
        return count;
    }

    private IEnumerable<(int X, int Y)> TargetStartLocations(string target)
    {
        for (int x = 0; x < _width; x++)
        for (int y = 0; y < _height; y++)
        {
            if (_lines[y][x] == target[0]) yield return (x, y);
        }
    }

    private int MatchesFromLocation((int X, int Y) location, string target)
    {
        return
            TestMatch(location, (1, 0), target)
            + TestMatch(location, (1, 1), target)
            + TestMatch(location, (0, 1), target)
            + TestMatch(location, (-1, 1), target)
            + TestMatch(location, (-1, 0), target)
            + TestMatch(location, (-1, -1), target)
            + TestMatch(location, (0, -1), target)
            + TestMatch(location, (1, -1), target);
    }

    /// <summary>
    /// Test for a match from one location, going in a certain direction.
    /// </summary>
    /// <param name="location">Coordinates of location to start from.</param>
    /// <param name="vector">Which direction to find the match.</param>
    /// <param name="target">Search string</param>
    /// <returns>1 if match found, 0 if no match (will be summed by caller.)</returns>
    private int TestMatch((int X, int Y) location, (int X, int Y) vector, string target)
    {
        for (int n = 0; n < target.Length; n++)
        {
            int x = location.X + (vector.X * n);
            int y = location.Y + (vector.Y * n);
            if (x < 0 || x >= _width || y < 0 || y >= _height) return 0;
            if (_lines[y][x] != target[n]) return 0;
        }
        return 1;
    }

    private bool IsXMatch((int X, int Y) corner, string target)
    {
        // Support only odd length.
        if (target.Length % 2 == 0) return false;
        
        // Test within boundaries.
        if (corner.X < 0 || corner.Y < 0) return false;
        if (_width - corner.X < target.Length || _height - corner.Y < target.Length) return false;

        var corner2 = (corner.X + target.Length - 1, corner.Y + target.Length - 1);
        if (TestMatch(corner, (1, 1), target) == 0
            && TestMatch(corner2, (-1, -1), target) == 0)
            return false;
        
        var corner3 = (corner.X + target.Length - 1, corner.Y);
        var corner4 = (corner.X, corner.Y + target.Length - 1);
        if (TestMatch(corner3, (-1, 1), target) == 0
            && TestMatch(corner4, (1, -1), target) == 0)
            return false;

        return true;
    }
}