namespace AdventOfCode;

public sealed class Day11: BaseDay
{
    private readonly string _input;
    private readonly List<long> _stones;
    
    public Day11()
    {
        _input = File.ReadAllText(InputFilePath);
        _stones = _input.Split(' ').Select(long.Parse).ToList();
    }
    
    public override ValueTask<string> Solve_1() => new($"{Blink_brute_force(_stones, 25).Length}");

    public override ValueTask<string> Solve_2() => new($"{Blink_avoid_dups(_stones, 75)}");

    private long[] Blink_brute_force(IEnumerable<long> stones, int times)
    {
        long[] results = [..stones];
        
        for (int i = 0; i < times; i++)
        {
            results = Blink(results).ToArray();
            //Console.WriteLine($"After round {i + 1}: {results.Length} stones");
        }
        return results;
    }

    private long Blink_avoid_dups(IEnumerable<long> stones, int times)
    {
        var stoneDict = new Dictionary<long, long>();

        foreach (var stone in stones)
        {
            AddStoneInfo(stoneDict, stone, 1);
        }
        
        for (int i = 0; i < times; i++)
        {
            stoneDict = Blink(stoneDict);
            //Console.WriteLine($"After round {i + 1}: {stoneDict.Count()} stone entries");
        }

        return stoneDict.Values.Sum();
    }
    
    private IEnumerable<long> Blink(long stone)
    {
        if (stone == 0)
        {
            yield return 1;
        }
        else
        {
            var stoneText = Math.Abs(stone).ToString();
            int digits = stoneText.Length;
            if (digits % 2 == 0)
            {
                yield return long.Parse(stoneText[..(digits / 2)]);
                yield return long.Parse(stoneText[(digits / 2)..]);
            }
            else
            {
                yield return checked(stone * 2024);
            }
        }
    }
    
    private IEnumerable<long> Blink(IEnumerable<long> stones) => stones.SelectMany(Blink);

    private Dictionary<long, long> Blink(Dictionary<long, long> stones)
    {
        var nextStones = new Dictionary<long, long>();

        foreach (var stone in stones)
        {
            var quantity = stone.Value;

            foreach (var nextStoneKey in Blink(stone.Key))
            {
                AddStoneInfo(nextStones, nextStoneKey, quantity);
            }
        }
        
        return nextStones;
    }

    private static void AddStoneInfo(Dictionary<long, long> stones, long stone, long quantity)
    {
        if (stones.ContainsKey(stone))
        {
            stones[stone] += quantity;
        }
        else
        {
            stones.Add(stone, quantity);
        }
    }
}
