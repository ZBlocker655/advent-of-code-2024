using System.Text.RegularExpressions;

namespace AdventOfCode;

public sealed class Day14: BaseDay
{
    private const bool Debug = false;
    private const int Width = 101;
    private const int Height = 103;

    private sealed class Robot
    {
        public (int X, int Y) Position { get; set; }
        public (int X, int Y) Vector { get; set; }

        public void Move()
        {
            Position =
                (
                    NewPosition(Position.X, Vector.X, Width),
                    NewPosition(Position.Y, Vector.Y, Height)
                );
        }

        private static int NewPosition(int orig, int vector, int spanSize)
        {
            var rawNew = orig + vector;
            while (rawNew < 0) rawNew += spanSize;
            while (rawNew >= spanSize) rawNew -= spanSize;
            return rawNew;
        }

        public int Quadrant =>
            Position switch
            {
                (< Width / 2, < Height / 2) => 1,
                (> Width / 2, < Height / 2) => 2,
                (< Width / 2, > Height / 2) => 3,
                (> Width / 2, > Height / 2) => 4,
                _ => 0
            };
    }
    
    private readonly string[] _lines;
    private readonly Robot[] _robots1;
    private readonly Robot[] _robots2;
    
    public Day14()
    {
        _lines = File.ReadAllLines(InputFilePath);
        _robots1 = _lines.Select(ReadRobot).ToArray();
        _robots2 = _lines.Select(ReadRobot).ToArray();
    }
    
    public override ValueTask<string> Solve_1() => new($"{SafetyFactorAfterSeconds(_robots1, 100)}");

    public override ValueTask<string> Solve_2() => new($"{SecondsToChristmasTree(_robots2)}");

    private int SafetyFactorAfterSeconds(Robot[] robots, int seconds)
    {
        MoveRobots(robots, seconds);
        return SafetyFactor(robots);
    }
    
    private int SafetyFactor(Robot[] robots) => robots
        .GroupBy(r => r.Quadrant)
        .Where(q => q.Key != 0)
        .Select(q => q.Count())
        .Aggregate(1, (total, next) => total * next);

    #region Grid
    
    private int[,] RenderGrid(Robot[] robots)
    {
        var grid = new int[Height, Width];
        foreach (var robot in robots)
        {
            grid[robot.Position.Y, robot.Position.X]++;
        }
        return grid;
    }

    private void DisplayGrid(int[,] grid)
    {
        Console.WriteLine(new string('-', Width + 2));
        
        for (int y = 0; y < Height; y++)
        {
            var row = Enumerable.Range(0, Width - 1)
                .Select(x => grid[y, x])
                .Select(n => n > 0 ? 'x' : ' ')
                .ToArray();
            Console.Write('|');
            Console.Write(row);
            Console.WriteLine('|');
        }

        Console.WriteLine(new string('-', Width + 2));
        Console.WriteLine();
    }
    
    #endregion Grid
    
    #region Christmas tree

    private int SecondsToChristmasTree(Robot[] robots)
    {
        const int cutoffSeconds = 500_000_000;
        int seconds = 0;
        
        while (++seconds < cutoffSeconds)
        {
            MoveRobots(robots);
            var grid = RenderGrid(robots);
            
            if (GuessIsChristmasTree(grid))
            {
                Console.WriteLine($"{seconds} seconds: POSSIBLE SOLUTION");
                DisplayGrid(grid);
                var userResponse = Console.ReadKey().KeyChar;
                if (userResponse == 'y') return seconds;
            }
            else if (seconds % 1_000_000 == 0)
            {
                Console.WriteLine(seconds);
            }
        }

        return -1;
    }
    
    /// <summary>
    /// Try to guess if the given grid contains a Christmas tree picture encoded in it.
    /// </summary>
    /// <remarks>
    /// The basic idea is: if we find one row of x's that's long enough, followed by
    /// another row that's two x's longer, we likely have the Christmas tree.
    ///
    ///   xxxxxxx
    ///  xxxxxxxxx
    /// </remarks>
    private bool GuessIsChristmasTree(int[,] grid)
    {
        const int treeRowMinWidth = 7;
        const int conformingRowsThreshold = 2;
        int conformingRowsFound = 0;
        int lastStartOfXs = -1, lastXLength = 0;
        
        for (int y = 0; y < Height; y++)
        {
            int xLength = 0;
            int startOfXs = -1;
            
            for (int x = 0; x < Width; x++)
            {
                if (grid[y, x] > 0)
                {
                    if (startOfXs == -1)
                    {
                        startOfXs = x;
                    }
                    xLength++;
                }
                else if (xLength >= treeRowMinWidth)
                {
                    if (conformingRowsFound > 0)
                    {
                        if (startOfXs == lastStartOfXs - 1 && xLength == lastXLength + 2)
                        {
                            conformingRowsFound++;
                            if (conformingRowsFound >= conformingRowsThreshold)
                            {
                                Console.WriteLine($"See row {y}");
                                return true;
                            }
                        }
                        else
                        {
                            conformingRowsFound = 1;
                        }
                    }
                    else
                    {
                        conformingRowsFound = 1;
                    }
                    
                    lastXLength = xLength;
                    lastStartOfXs = startOfXs;

                    break;
                }
                else
                {
                    xLength = 0;
                    startOfXs = -1;
                }
            }
        }
        
        return false;
    }
    
    #endregion Christmas tree
    
    #region Move
    
    private void MoveRobots(Robot[] robots, int seconds)
    {
        for (int t = 0; t < seconds; t++) MoveRobots(robots);
    }
    
    private void MoveRobots(Robot[] robots)
    {
        foreach (var robot in robots) robot.Move();
    }
    
    #endregion Move
    
    #region Read from file

    private Robot ReadRobot(string line)
    {
        const string pattern = @"^p=(\d+),(\d+)\sv=(-?\d+),(-?\d+)$";
        var match = Regex.Match(line, pattern);
        if (!match.Success) throw new InvalidDataException($"Bad input line: {line}");
        return new Robot
        {
            Position = (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value)),
            Vector = (int.Parse(match.Groups[3].Value), int.Parse(match.Groups[4].Value)),
        };
    }
    
    #endregion Read from file
}
