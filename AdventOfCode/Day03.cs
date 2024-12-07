using System.Text.RegularExpressions;

namespace AdventOfCode;

public sealed class Day03: BaseDay
{
    private const string MulPattern = @"mul\((\d+),(\d+)\)";
    private const string MulWithConditionalPattern = @"(mul|do|don't)\(((\d+),(\d+))?\)";
    
    private readonly string _input;
    
    public Day03()
    {
        _input = File.ReadAllText(InputFilePath);
    }
    
    public override ValueTask<string> Solve_1() => new($"{MulResultSum()}");

    public override ValueTask<string> Solve_2() => new($"{MulWithConditionalsResultSum()}");

    private int MulResultSum()
    {
        var matches = Regex.Matches(_input, MulPattern);

        return matches
            .Select(m => new { Operand1 = int.Parse(m.Groups[1].Value), Operand2 = int.Parse(m.Groups[2].Value) })
            .Select(ops => ops.Operand1 * ops.Operand2)
            .Sum();
    }

    private int MulWithConditionalsResultSum()
    {
        var matches = Regex.Matches(_input, MulWithConditionalPattern);

        var sum = 0;
        var enabled = true;

        foreach (Match match in matches)
        {
            var instruction = match.Groups[1].Value;

            if (instruction == "mul" && !string.IsNullOrEmpty(match.Groups[2].Value))
            {
                if (enabled)
                {
                    sum += int.Parse(match.Groups[3].Value) * int.Parse(match.Groups[4].Value);
                }
            }
            else if (instruction == "do" && string.IsNullOrEmpty(match.Groups[2].Value))
            {
                enabled = true;
            }
            else if (instruction == "don't" && string.IsNullOrEmpty(match.Groups[2].Value))
            {
                enabled = false;
            }
        }
        
        return sum;
    }
}
