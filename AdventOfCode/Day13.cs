using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace AdventOfCode;

public sealed class Day13: BaseDay
{
    private const bool Debug = false;
    
    private const long ButtonACost = 3;
    private const long ButtonBCost = 1;
    private const long ConversionOffset = 10_000_000_000_000;
    
    private struct Coord
    {
        public long X { get; set; }
        public long Y { get; set; }
    }

    private sealed class ClawMachine
    {
        public Coord ButtonA { get; set; }
        public Coord ButtonB { get; set; }
        public Coord Prize { get; set; }

        public override string ToString() => 
            $"A:({ButtonA.X}, {ButtonA.Y}), B:({ButtonB.X}, {ButtonB.Y}) => Prize:({Prize.X}, {Prize.Y})";
    }

    private sealed class Scenario
    {
        public ClawMachine Machine { get; set; }
        public long ButtonAPresses { get; set; }
        public long ButtonBPresses { get; set; }
        public long TokensSpent { get; set; }

        public override string ToString() => $"{ButtonAPresses} and {ButtonBPresses} = {TokensSpent} tokens";
    }
    
    private readonly string[] _lines;
    private readonly ClawMachine[] _clawMachines;
    private readonly ClawMachine[] _clawMachines_adjusted;
    
    public Day13()
    {
        _lines = File.ReadAllLines(InputFilePath);
        _clawMachines = ReadClawMachines(_lines).ToArray();
        _clawMachines_adjusted = _clawMachines.Select(AdjustForOffset).ToArray();
    }
    
    public override ValueTask<string> Solve_1() => new($"{SmallestTokenSpend(_clawMachines)}");

    public override ValueTask<string> Solve_2() => new($"{SmallestTokenSpend(_clawMachines_adjusted)}");

    private long SmallestTokenSpend(ClawMachine[] machines) =>
        machines
            //.Select(BestScenario_brute_force)
            .Select(BestScenario_system_of_equations)
            .Where(s => s != null)
            .Select(Verify)
            .Sum(s => s!.TokensSpent);
    
    private ClawMachine AdjustForOffset(ClawMachine machine) =>
        new ClawMachine
        {
            ButtonA = machine.ButtonA,
            ButtonB = machine.ButtonB,
            Prize = new Coord { X = machine.Prize.X + ConversionOffset, Y = machine.Prize.Y + ConversionOffset },
        };
    
    private Scenario? BestScenario_system_of_equations(ClawMachine machine)
    {
        // Modeling this problem as a linear system of equations (with A presses and B presses as the variables).
        // Using Elimination method - see https://www.cuemath.com/algebra/system-of-equations/)

        checked
        {
            // SOLVE for B presses.
        
            long bCoeff = (machine.ButtonA.X * machine.ButtonB.Y) - (machine.ButtonA.Y * machine.ButtonB.X);
            long modPrize = (machine.ButtonA.X * machine.Prize.Y) - (machine.ButtonA.Y * machine.Prize.X);

            if (modPrize % bCoeff != 0)
            {
                DebugDump(machine, []);
                return null; // No solution.
            }
        
            long bPresses = modPrize / bCoeff;
        
            // DERIVE A presses.

            long aTarget = machine.Prize.X - (machine.ButtonB.X * bPresses);
            
            if (aTarget % machine.ButtonA.X != 0)
            {
                DebugDump(machine, []);
                return null; // No solution.
            }
            
            long aPresses = aTarget / machine.ButtonA.X;
        
            var scenario = new Scenario
            {
                Machine = machine,
                ButtonAPresses = aPresses,
                ButtonBPresses = bPresses,
                TokensSpent = CalcTokensSpent(aPresses, bPresses)
            };
            
            DebugDump(machine, [scenario]);

            return scenario;
        }
    }

    private Scenario? BestScenario_brute_force(ClawMachine machine)
    {
        var scenarios = WinningScenarios(machine)
            .ToArray();
        
        DebugDump(machine, scenarios);

        return scenarios
            .OrderBy(s => s.TokensSpent)
            .FirstOrDefault();
    }

    private void DebugDump(ClawMachine machine, Scenario[] scenarios)
    {
        if (!Debug) return;
        Dump(machine, scenarios);
    }
    
    private void Dump(ClawMachine machine, Scenario[] scenarios)
    {
        if (scenarios.Length > 0)
        {
            // Learn more about cases with multiple scenarios.
            Console.WriteLine(machine.ToString());
            Console.WriteLine("---");
            foreach (var s in scenarios) Dump(s);
            Console.WriteLine("---");
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine($"{machine} - NO SOLUTION");
        }
    }

    private Scenario Dump(Scenario s)
    {
        Console.WriteLine(s.ToString());
        return s;
    }
    
    private IEnumerable<Scenario> WinningScenarios(ClawMachine machine)
    {
        long buttonAPresses = 0;
        long buttonBPresses;

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
                    Machine = machine,
                    ButtonAPresses = buttonAPresses,
                    ButtonBPresses = buttonBPresses,
                    TokensSpent = CalcTokensSpent(buttonAPresses, buttonBPresses),
                };
            }

            buttonAPresses++;
        }
    }

    private static long CalcTokensSpent(long buttonAPresses, long buttonBPresses) => 
        (buttonAPresses * ButtonACost) + (buttonBPresses * ButtonBCost);

    private Scenario? Verify(Scenario? scenario)
    {
        if (scenario == null) return null;

        if ((scenario.Machine.ButtonA.X * scenario.ButtonAPresses) + (scenario.Machine.ButtonB.X * scenario.ButtonBPresses) != scenario.Machine.Prize.X)
        {
            Console.WriteLine("MISMATCH X");
            Dump(scenario.Machine, [scenario]);
        }
        
        if ((scenario.Machine.ButtonA.Y * scenario.ButtonAPresses) + (scenario.Machine.ButtonB.Y * scenario.ButtonBPresses) != scenario.Machine.Prize.Y)
        {
            Console.WriteLine("MISMATCH Y");
            Dump(scenario.Machine, [scenario]);
        }
        
        return scenario;
    }
    
    #region Read from file
    
    private IEnumerable<ClawMachine> ReadClawMachines(string[] lines)
    {
        ClawMachine? clawMachine;
        var index = 0L;
        
        do
        {
            clawMachine = ReadClawMachine(lines, ref index);
            if (clawMachine != null) yield return clawMachine;
        } while (clawMachine != null);
    }

    private ClawMachine? ReadClawMachine(string[] lines, ref long startIndex)
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

    private static Coord ReadButton(string[] lines, long index, string name)
    {
        var pattern = @$"^\s*{name}:\s*X\+(\d+),\s*Y\+(\d+)\s*$";
        
        var match = Regex.Match(lines[index], pattern);
        if (!match.Success) throw new InvalidOperationException($"Expected {name} on line {index}");
        return new Coord { X = long.Parse(match.Groups[1].Value), Y = long.Parse(match.Groups[2].Value) };
    }
    
    private static Coord ReadPrize(string[] lines, long index)
    {
        var pattern = @$"^\s*Prize:\s*X=(\d+),\s*Y=(\d+)\s*$";
        
        var match = Regex.Match(lines[index], pattern);
        if (!match.Success) throw new InvalidOperationException($"Expected Prize on line {index}");
        return new Coord { X = long.Parse(match.Groups[1].Value), Y = long.Parse(match.Groups[2].Value) };
    }
    
    #endregion Read from file
}
