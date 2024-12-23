using System.Text.RegularExpressions;

namespace AdventOfCode;

public sealed class Day13: BaseDay
{
    private struct Coord
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    private sealed class ClawMachine
    {
        public Coord ButtonA { get; set; }
        public Coord ButtonB { get; set; }
        public Coord Prize { get; set; }
    }

    private sealed class Scenario
    {
        public int ButtonAPresses { get; set; }
        public int ButtonBPresses { get; set; }
        public int TokensSpent { get; set; }
    }
    
    private readonly string[] _lines;
    
    public Day13()
    {
        _lines = File.ReadAllLines(InputFilePath);
    }
    
    public override ValueTask<string> Solve_1() => new($"{SmallestTokenSpend(_lines)}");

    public override ValueTask<string> Solve_2() => new($"{null}");

    private int SmallestTokenSpend(string[] lines)
    {
        var clawMachines = ReadClawMachines(lines).ToArray();
        return SmallestTokenSpend(clawMachines);
    }

    private int SmallestTokenSpend(ClawMachine[] machines) =>
        machines
            .Select(BestScenario)
            .Where(s => s != null)
            .Sum(s => s!.TokensSpent);

    private Scenario? BestScenario(ClawMachine machine) => WinningScenarios(machine)
        .OrderBy(s => s.TokensSpent)
        .FirstOrDefault();
    
    private IEnumerable<Scenario> WinningScenarios(ClawMachine machine)
    {
        const int buttonACost = 3;
        const int buttonBCost = 1;
        int buttonAPresses = 0;
        int buttonBPresses;

        while (buttonAPresses * machine.ButtonA.X <= machine.Prize.X &&
               buttonAPresses * machine.ButtonA.Y <= machine.Prize.Y)
        {
            bool foundScenario = false;
            buttonBPresses = 0;
            
            var afterAPresses = new Coord
            {
                X = machine.ButtonA.X * buttonAPresses,
                Y = machine.ButtonA.Y * buttonAPresses
            };
            
            var deltaX = machine.Prize.X - afterAPresses.X;
            var deltaY = machine.Prize.Y - afterAPresses.Y;

            if (deltaX == 0 && deltaY == 0)
            {
                foundScenario = true;
                buttonBPresses = 0;
            }
            else if (
                deltaX > 0 && deltaY > 0
                && deltaX % machine.ButtonB.X == 0 && deltaY % machine.ButtonB.Y == 0
                && deltaX / machine.ButtonB.X == deltaY / machine.ButtonB.Y)
            {
                foundScenario = true;
                buttonBPresses = deltaX / machine.ButtonB.X;
            }

            if (foundScenario)
            {
                yield return new Scenario
                {
                    ButtonAPresses = buttonAPresses,
                    ButtonBPresses = buttonBPresses,
                    TokensSpent = (buttonAPresses * buttonACost) + (buttonBPresses * buttonBCost),
                };
            }

            buttonAPresses++;
        }
    }
    
    #region Read from file
    
    private IEnumerable<ClawMachine> ReadClawMachines(string[] lines)
    {
        ClawMachine? clawMachine;
        var index = 0;
        
        do
        {
            clawMachine = ReadClawMachine(lines, ref index);
            if (clawMachine != null) yield return clawMachine;
        } while (clawMachine != null);
    }

    private ClawMachine? ReadClawMachine(string[] lines, ref int startIndex)
    {
        while (startIndex < lines.Length && string.IsNullOrWhiteSpace(lines[startIndex])) startIndex++;
        if (startIndex > lines.Length - 3) return null;
        
        return new ClawMachine
        {
            ButtonA = ReadButton(lines, startIndex++, "Button A"),
            ButtonB = ReadButton(lines, startIndex++, "Button B"),
            Prize = ReadPrize(lines, startIndex++)
        };
    }

    private static Coord ReadButton(string[] lines, int index, string name)
    {
        var pattern = @$"^\s*{name}:\s*X\+(\d+),\s*Y\+(\d+)\s*$";
        
        var match = Regex.Match(lines[index], pattern);
        if (!match.Success) throw new Exception($"Expected {name} on line {index}");
        return new Coord { X = int.Parse(match.Groups[1].Value), Y = int.Parse(match.Groups[2].Value) };
    }
    
    private static Coord ReadPrize(string[] lines, int index)
    {
        var pattern = @$"^\s*Prize:\s*X=(\d+),\s*Y=(\d+)\s*$";
        
        var match = Regex.Match(lines[index], pattern);
        if (!match.Success) throw new Exception($"Expected Prize on line {index}");
        return new Coord { X = int.Parse(match.Groups[1].Value), Y = int.Parse(match.Groups[2].Value) };
    }
    
    #endregion Read from file
}
