namespace AdventOfCode;

public sealed class Day10: BaseDay
{
    private readonly string[] _lines;
    private readonly int[,] _grid;
    private int _width, _height;
    
    public Day10()
    {
        _lines = File.ReadAllLines(InputFilePath);
        _width = _lines[0].Length;
        _height = _lines.Length;
        _grid = GetGrid(_lines);
    }
    
    public override ValueTask<string> Solve_1() => new($"{SumOfTrailheadScores()}");

    public override ValueTask<string> Solve_2() => new($"{SumOfTrailheadRatings()}");

    private int SumOfTrailheadScores()
    {
        var sum = 0;
        
        for (int y = 0; y < _height; y++)
        for (int x = 0; x < _width; x++)
        {
            if (_grid[y, x] == 0)
            {
                sum += TrailheadScore((x, y));
            }
        }
        
        return sum;
    }
    
    private int SumOfTrailheadRatings()
    {
        var sum = 0;
        
        for (int y = 0; y < _height; y++)
        for (int x = 0; x < _width; x++)
        {
            if (_grid[y, x] == 0)
            {
                sum += TrailheadRating((x, y));
            }
        }
        
        return sum;
    }

    private int TrailheadScore((int X, int Y) trailhead)
    {
        if (!IsTrailhead(trailhead)) return 0;
        
        var visited = new HashSet<(int X, int Y)>();
        
        var posToVisit = new Stack<(int X, int Y)>();
        posToVisit.Push(trailhead);
        int score = 0;

        while (posToVisit.Count > 0)
        {
            var pos = posToVisit.Pop();
            visited.Add(pos);

            if (_grid[pos.Y, pos.X] == 9)
            {
                // Got to the end of the trail.
                score++;
            }
            else
            {
                foreach (var nextPos in TrailNext(pos))
                {
                    if (!visited.Contains(nextPos))
                    {
                        posToVisit.Push(nextPos);
                    }
                }
            }
        }
        
        return score;
    }

    private int TrailheadRating((int X, int Y) trailhead)
    {
        if (!IsTrailhead(trailhead)) return 0;

        var visited = new HashSet<string>();
        var path = new Stack<(int X, int Y)>();

        return TrailheadRating(trailhead, path, visited);
    }
    
    private int TrailheadRating((int X, int Y) pos, Stack<(int X, int Y)> path, HashSet<string> visited)
    {
        path.Push(pos);
        var pathKey = string.Join('|', path);
        int localRating = 0;

        if (visited.Add(pathKey))
        {
            if (_grid[pos.Y, pos.X] == 9)
            {
                localRating++;
            }
            else
            {
                foreach (var nextPos in TrailNext(pos))
                {
                    localRating += TrailheadRating(nextPos, path, visited);
                }
            }
        }

        path.Pop();
        return localRating;
    }
    
    private IEnumerable<(int X, int Y)> TrailNext((int X, int Y) pos) => Neighbors(pos)
        .Where(nextPos => _grid[nextPos.Y, nextPos.X] == _grid[pos.Y, pos.X] + 1);

    private IEnumerable<(int X, int Y)> Neighbors((int X, int Y) pos)
    {
        if (pos.Y > 0) yield return (pos.X, pos.Y - 1);
        if (pos.X < _width - 1) yield return (pos.X + 1, pos.Y);
        if (pos.Y < _height - 1) yield return (pos.X, pos.Y + 1);
        if (pos.X > 0) yield return (pos.X - 1, pos.Y);
    }
    
    private bool IsTrailhead((int X, int Y) pos) => InGrid(pos) && _grid[pos.Y, pos.X] == 0;
    
    private bool InGrid((int X, int Y) pos) => pos.X >= 0 && pos.X < _width && pos.Y >= 0 && pos.Y < _height;

    private int[,] GetGrid(string[] lines)
    {
        int rows = lines.Length;
        int cols = lines[0].Length;
        var grid = new int[rows, cols];
        
        for (int y = 0; y < rows; y++)
        for (int x = 0; x < cols; x++)
        {
            grid[y, x] = lines[y][x] - '0';
        }
        
        return grid;
    }
}
