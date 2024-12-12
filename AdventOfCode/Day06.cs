namespace AdventOfCode;

public sealed class Day06: BaseDay
{
    private readonly string[] _lines;
    private int _width, _height;
    
    private readonly Dictionary<char, (int, int)> _initialVectors = new()
    {
        ['^'] = (0, -1),
        ['v'] = (0, 1),
        ['<'] = (-1, 0),
        ['>'] = (1, 0),
    };
    
    public Day06()
    {
        _lines = File.ReadAllLines(InputFilePath);
        _width = _lines[0].Length;
        _height = _lines.Length;
    }
    
    public override ValueTask<string> Solve_1() => new($"{DistinctGuardPositions()}");

    public override ValueTask<string> Solve_2() => new($"{ObstructionsForGuardLoop_test_default_path()}");

    private int DistinctGuardPositions()
    {
        var initialPos = GetInitialGuardPosition();
        var initialVector = _initialVectors[_lines[initialPos.y][initialPos.x]];
        var grid = GetGrid(_lines);

        return GetGuardPath(initialPos, initialVector, grid)
            .Select(p => p.Position)
            .Distinct()
            .Count();
    }

    private int ObstructionsForGuardLoop_test_all_cells()
    {
        var initialPos = GetInitialGuardPosition(); 
        var initialVector = _initialVectors[_lines[initialPos.y][initialPos.x]];
        int candidatesFound = 0;
        var testGrid = GetGrid(_lines);

        // For each cell in the grid, if it doesn't already have an obstruction there, put one in 
        // and test if that makes the guard enter an infinite loop.
        for (var y = 0; y < _height; y++)
        for (var x = 0; x < _width; x++)
        {
            if (!ObstacleAt((x, y), testGrid))
            {
                if (TestNewObstacle(testGrid, initialPos, initialVector, (x,y)))
                    candidatesFound++;
            }
        }
        
        return candidatesFound;
    }

    private int ObstructionsForGuardLoop_test_default_path()
    {
        var initialPos = GetInitialGuardPosition(); 
        var initialVector = _initialVectors[_lines[initialPos.y][initialPos.x]];
        var foundCandidates = new HashSet<(int x, int y)>();
        var testGrid = GetGrid(_lines);
        var defaultPath = GetGuardPath(initialPos, initialVector, testGrid);

        foreach (var (pos, vector) in defaultPath)
        {
            // Omit initial position.
            if (pos == initialPos) continue;
            
            if (!ObstacleInFront(pos, vector, testGrid))
            {
                var testObstaclePos = ApplyVector(pos, vector);
                if (!foundCandidates.Contains(testObstaclePos))
                {
                    if (TestNewObstacle(testGrid, initialPos, initialVector, testObstaclePos))
                        foundCandidates.Add(testObstaclePos);
                }
            }
        }
        
        return foundCandidates.Count;
    }

    private bool TestNewObstacle(char[][] grid, (int x, int y) pos, (int x, int y) vector, (int x, int y) newObstaclePos)
    {
        if (!InGrid(newObstaclePos)) return false;
        var orig = grid[newObstaclePos.y][newObstaclePos.x];
        var testResult = false;
        
        grid[newObstaclePos.y][newObstaclePos.x] = '#'; // Added the obstacle to the test grid!
        if (IsGuardInfiniteLoop(pos, vector, grid))
        {
            testResult = true;
        }

        grid[newObstaclePos.y][newObstaclePos.x] = orig; // Take the obstacle back out.
        
        return testResult;
    }

    private (int x, int y) GetInitialGuardPosition()
    {
        for (var y = 0; y < _height; y++)
            for (var x = 0; x < _width; x++)
                if (_initialVectors.ContainsKey(_lines[y][x])) return (x, y);

        throw new Exception("No initial guard position found");
    }

    private IEnumerable<((int x, int y) Position, (int x, int y) Vector)> GetGuardPath((int x, int y) initialPos, 
        (int x, int y) initialVector, char[][] grid)
    {
        var pos = initialPos;
        var vector = initialVector;

        while (InGrid(pos))
        {
            yield return (pos, vector);
            
            var testNextPos = ApplyVector(pos, vector);

            if (ObstacleAt(testNextPos, grid))
            {
                vector = TurnRight(vector);
            }
            else
            {
                pos = testNextPos;
            }
        }
    }
    
    private (int x, int y) ApplyVector((int x, int y) pos, (int x, int y) vector) => 
        (pos.x + vector.x, pos.y + vector.y);
    
    private bool ObstacleAt((int x, int y) pos, char[][] grid) => InGrid(pos) && grid[pos.y][pos.x] == '#';

    private bool ObstacleInFront((int x, int y) pos, (int x, int y) vector, char[][] grid) =>
        InGrid(pos) && ObstacleAt(ApplyVector(pos, vector), grid);
    
    private bool InGrid((int x, int y) pos) => pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;

    private (int x, int y) TurnRight((int x, int y) vector) => vector switch
    {
        (-1, 0) => new(0, -1),
        (0, -1) => new(1, 0),
        (1, 0) => new(0, 1),
        (0, 1) => new(-1, 0),
        _ => new(1, 0), // Safety catch-all.
    };

    private bool IsGuardInfiniteLoop((int x, int y) initialPos, 
        (int x, int y) initialVector, char[][] grid)
    {
        var observedStates = new HashSet<((int, int), (int, int))>(); // Two-part key: position, vector
        var infiniteLoopDetected = false;

        foreach (var (pos, vector) in GetGuardPath(initialPos, initialVector, grid))
        {
            if (observedStates.Contains((pos, vector)))
            {
                infiniteLoopDetected = true;
                break;
            }

            observedStates.Add((pos, vector));
        }
        
        return infiniteLoopDetected;
    }
    
    private char[][] GetGrid(string[] lines) => lines.Select(l => l.ToCharArray()).ToArray();
}
