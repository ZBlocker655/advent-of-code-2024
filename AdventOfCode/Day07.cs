namespace AdventOfCode;

public sealed class Day07: BaseDay
{
    private sealed class Equation
    {
        public long Result { get; set; }
        public long[] Operands { get; set; }

        public bool TestValid(Func<long, long, long>[] operators)
        {
            var test = CounterValues(Operands.Length - 1, operators.Length - 1).ToArray();
            
            foreach (var opsToTry in CounterValues(Operands.Length - 1, operators.Length - 1))
            {
                long left = Operands[0];

                for (int i = 1; i < Operands.Length; i++)
                {
                    long right = Operands[i];
                    var op = operators[opsToTry[i - 1]];
                    left = op(left, right);
                }

                if (left == Result) return true;
            }
            
            return false;
        }
    }
    
    private readonly string[] _lines;
    private readonly Equation[] _equations;

    private static long Add(long a, long b) => a + b;
    private static long Multiply(long a, long b) => a * b;
    private static long Concatenate(long a, long b) => long.Parse($"{a}{b}");
    
    public Day07()
    {
        _lines = File.ReadAllLines(InputFilePath);
        _equations = _lines.Select(ParseLine).ToArray();
    }
    
    public override ValueTask<string> Solve_1() => new($"{SumOfValidEquationResults(_equations, [Add, Multiply])}");

    public override ValueTask<string> Solve_2() => new($"{SumOfValidEquationResults(_equations, [Add, Multiply, Concatenate])}");

    private long SumOfValidEquationResults(Equation[] equations, Func<long, long, long>[] operators) =>
        equations
            .Where(e => e.TestValid(operators))
            .Sum(e => e.Result);

    private Equation ParseLine(string line)
    {
        var parts = line.Split(": ");
        return new Equation
        {
            Result = long.Parse(parts[0]),
            Operands = parts[1].Split(' ').Select(long.Parse).ToArray()
        };
    }

    private static IEnumerable<int[]> CounterValues(int setSize, int itemMax)
    {
        var counters = Enumerable.Repeat(0, setSize).ToList();
        bool done = false;
        bool readyToRelease = true;
        int place = 0;

        while (!done)
        {
            if (readyToRelease) yield return counters.ToArray();

            if (counters[place] < itemMax)
            {
                counters[place]++;
                place = 0;
                readyToRelease = true;
            }
            else if (place == setSize - 1)
            {
                done = true;
            }
            else
            {
                counters[place] = 0;
                place++;
                readyToRelease = false;
            }
        }
    }
}
