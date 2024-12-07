namespace AdventOfCode;

public sealed class Day02: BaseDay
{
    private const int DiffMin = 1;
    private const int DiffMax = 3;
    
    private readonly string _input;
    
    public Day02()
    {
        _input = File.ReadAllText(InputFilePath);
    }
    
    public override ValueTask<string> Solve_1() => new($"{SafeReports()}");

    public override ValueTask<string> Solve_2() => new($"{SafeReportsWithDampener()}");
    
    private int[][] Reports() =>
        _input
            .Split('\n')
            .Select(s => Array.ConvertAll(s.Split(' '), int.Parse))
            .ToArray();

    private int SafeReports() =>
        Reports()
            .Where(IsSafeReport)
            .Count();

    private int SafeReportsWithDampener() => 
        Reports()
            .Where(IsSafeReportWithDampener)
            .Count();

    private bool IsSafeReport(int[] report)
    {
        if (report.Length <= 1) return true;
        var offsets = report[1] > report[0] ? (0,1) : (1,0);

        for (int i = 0; i < report.Length - 1; i++)
        {
            var diff = report[i + offsets.Item2] - report[i + offsets.Item1];
            if (diff < DiffMin || diff > DiffMax) return false;
        }
        
        return true;
    }

    private bool IsSafeReportWithDampener(int[] report)
    {
        if (IsSafeReport(report)) return true;

        for (int i = 0; i < report.Length; i++)
        {
            if (IsSafeReport([.. report[..i], .. report[(i + 1)..]])) return true;
        }
        
        return false;
    }
}
