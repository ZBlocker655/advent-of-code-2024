namespace AdventOfCode;

public sealed class Day09: BaseDay
{
    private const int BlankValue = -1;
    
    private readonly string _input;
    
    public Day09()
    {
        _input = File.ReadAllText(InputFilePath);
    }
    
    public override ValueTask<string> Solve_1() => new($"{ComputeFileChecksum(_input)}");

    public override ValueTask<string> Solve_2() => new($"{ComputeFileChecksum_whole_file_compression(_input)}");

    private long ComputeFileChecksum(string fileMap)
    {
        var blocks = Blocks(fileMap);
        Compress(blocks);
        return Checksum(blocks);
    }
    
    private long ComputeFileChecksum_whole_file_compression(string fileMap)
    {
        var blocks = Blocks(fileMap);
        CompressWithWholeFiles(blocks);
        return Checksum(blocks);
    }

    private int[] Blocks(string fileMap)
    {
        var blockList = new List<int>();
        int id = 0;

        for (var i = 0; i < fileMap.Length; i += 2)
        {
            blockList.AddRange(Enumerable.Repeat(id++, (int)Char.GetNumericValue(fileMap[i])));
            if (i + 1 < fileMap.Length)
                blockList.AddRange(Enumerable.Repeat(BlankValue, (int)Char.GetNumericValue(fileMap[i+1])));
        }
        
        return blockList.ToArray();
    }

    private void Compress(int[] blocks)
    {
        int left = 0;
        int right = blocks.Length - 1;
        
        while (left < right)
        {
            while (blocks[left] != BlankValue && left < right) left++;
            while (blocks[right] == BlankValue && left < right) right--;

            if (left < right)
            {
                // Swap
                blocks[left] = blocks[right];
                blocks[right] = BlankValue;
            }
        }
    }

    private void CompressWithWholeFiles(int[] blocks)
    {
        var blankSegments = BlankSegments(blocks).ToList();
        int right = blocks.Length - 1;
        int length = 0, id;

        while (blankSegments.Count > 0 && right > blankSegments[0].StartIndex)
        {
            // Find next whole file.
            (right, length) = FindNextWholeFile(blocks, right);

            // Try to find next blank space to put it.
            int left = 0;
            bool found = false;
            while (!found && left < blankSegments.Count && blankSegments[left].StartIndex < right)
            {
                if (blankSegments[left].Length < length)
                {
                    // Segment is not suitable for the file.
                    left++;
                    continue;
                }
                
                found = true;
                    
                // Move the file into the blank space.
                for (int i = 0; i < length; i++)
                {
                    blocks[blankSegments[left].StartIndex + i] = blocks[right + 1 + i];
                    blocks[right + 1 + i] = BlankValue;
                }
                    
                // Update blank segment.
                if (blankSegments[left].Length > length)
                {
                    blankSegments[left] = 
                        (blankSegments[left].StartIndex + length, 
                            blankSegments[left].Length - length);
                }
                else
                {
                    blankSegments.RemoveAt(left);
                }
            }
        }
    }
    
    private (int right, int length) FindNextWholeFile(int[] blocks, int seekAt)
    {
        while (blocks[seekAt] == BlankValue) seekAt--;
        int length = 0;
        int id = blocks[seekAt];
        while (blocks[seekAt] == id)
        {
            length++;
            seekAt--;
        }

        return (seekAt, length);
    }

    private IEnumerable<(int StartIndex, int Length)> BlankSegments(int[] blocks)
    {
        int startIndex = -1;
        int length = 0;

        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i] == BlankValue)
            {
                if (startIndex == -1)
                {
                    startIndex = i;
                    length = 0;
                }

                length++;
                
                if (i == blocks.Length - 1) yield return (startIndex, length);
            }
            else
            {
                if (startIndex >= 0)
                {
                    yield return (startIndex, length);
                    startIndex = -1;
                }
            }
        }
    }

    private long Checksum(int[] blocks)
    {
        long result = 0;

        for (int i = 0; i < blocks.Length; i++)
            result += blocks[i] > 0 ? blocks[i] * i : 0;
        
        return result;
    }
}
