namespace AdventOfCode;

public sealed class Day08: BaseDay
{
    private sealed class Antenna
    {
        public char Frequency { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
    
    private readonly string[] _lines;
    private int _width, _height;
    private readonly Antenna[] _antennas;
    
    public Day08()
    {
        _lines = File.ReadAllLines(InputFilePath);
        _width = _lines[0].Length;
        _height = _lines.Length;
        _antennas = ReadAntennas(_lines).ToArray();
    }
    
    public override ValueTask<string> Solve_1() => new($"{AntinodeCount(_antennas)}");

    public override ValueTask<string> Solve_2() => new($"{LineAntinodeCount(_antennas)}");

    private int AntinodeCount(Antenna[] antennas) =>
        antennas
            .GroupBy(a => a.Frequency)
            //.Select(Dump)
            .SelectMany(AntennaPairs)
            .SelectMany(SingleAntinodes)
            .Where(InGrid)
            .Distinct()
            //.Select(Dump)
            .Count();
    
    private int LineAntinodeCount(Antenna[] antennas) =>
        antennas
            .GroupBy(a => a.Frequency)
            //.Select(Dump)
            .SelectMany(AntennaPairs)
            .SelectMany(LineAntinodes)
            .Where(InGrid)
            .Distinct()
            //.Select(Dump)
            .Count();

    private IEnumerable<Antenna> ReadAntennas(string[] lines)
    {
        for (int y = 0; y < lines.Length; y++)
        for (int x = 0; x < lines[y].Length; x++)
        {
            var cell = lines[y][x];
            if (cell != '.')
            {
                yield return new Antenna { Frequency = cell, X = x, Y = y };
            }
        }
    }

    private IEnumerable<(Antenna, Antenna)> AntennaPairs(IEnumerable<Antenna> antennas)
    {
        var antennaArray = antennas.ToArray();
        
        for (var a = 0; a < antennaArray.Length - 1; a++)
            for (var b = a + 1; b < antennaArray.Length; b++)
                yield return (antennaArray[a], antennaArray[b]);
    }

    private bool InGrid((int X, int Y) pos) => pos.X >= 0 && pos.X < _width && pos.Y >= 0 && pos.Y < _height;
    
    private IEnumerable<(int X, int Y)> SingleAntinodes((Antenna, Antenna) antennaPair)
    {
        var (a1, a2) = antennaPair;
        yield return (a1.X - a2.X + a1.X, a1.Y - a2.Y + a1.Y);
        yield return (a2.X - a1.X + a2.X, a2.Y - a1.Y + a2.Y);
    }
    
    private IEnumerable<(int X, int Y)> LineAntinodes((Antenna, Antenna) antennaPair)
    {
        var (a1, a2) = antennaPair;
        return LineAntinodes(a1, (a1.X - a2.X, a1.Y - a2.Y))
            .Concat(LineAntinodes(a2, (a2.X - a1.X, a2.Y - a1.Y)));
    }

    private IEnumerable<(int X, int Y)> LineAntinodes(Antenna antenna, (int X, int Y) vector)
    {
        var (x, y) = (antenna.X, antenna.Y);

        while (InGrid((x, y)))
        {
            yield return (x, y);
            x += vector.X;
            y += vector.Y;
        }
    }

    private IGrouping<char, Antenna> Dump(IGrouping<char, Antenna> group)
    {
        Console.WriteLine($"{group.Key}: {group.Count()}");
        return group;
    }

    private (int X, int Y) Dump((int X, int Y) pos)
    {
        Console.WriteLine($"Antinode: {pos.X}, {pos.Y}");
        return pos;
    }
}
