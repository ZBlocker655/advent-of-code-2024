namespace AdventOfCode;

public sealed class Day12: BaseDay
{
    private enum EdgeType { Top, Right, Bottom, Left }

    private sealed class Edge
    {
        public EdgeType Type { get; set; }
        public int Level { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
    }
    
    private sealed class Region
    {
        private Dictionary<(EdgeType, int), List<Edge>> _edges = new();
        
        public int Area { get; set; }
        public int Perimeter { get; set; }
        
        public int EdgeCount => _edges.Values.SelectMany(x => x).Count();

        public void AddEdge(Edge edge)
        {
            if (!_edges.ContainsKey((edge.Type, edge.Level))) _edges.Add((edge.Type, edge.Level), new());
            AddEdge(edge, _edges[(edge.Type, edge.Level)]);
        }

        private static void AddEdge(Edge edge, List<Edge> edgeList)
        {
            bool inserted = false;
            for (int i = 0; i < edgeList.Count; i++)
            {
                if (edge.Start < edgeList[i].Start)
                {
                    edgeList.Insert(i, edge);
                    inserted = true;
                    break;
                }
            }
            if (!inserted) edgeList.Add(edge);
            ConsolidateEdges(edgeList);
        }

        private static void ConsolidateEdges(List<Edge> edges)
        {
            for (int i = edges.Count - 2; i >= 0; i--)
            {
                if (edges[i].End == edges[i + 1].Start - 1)
                {
                    edges[i].End = edges[i + 1].End;
                    edges.RemoveAt(i + 1);
                }
            }
        }
    }
    
    private readonly string[] _lines;
    private int _width, _height;
    
    public Day12()
    {
        _lines = File.ReadAllLines(InputFilePath);
        _width = _lines[0].Length;
        _height = _lines.Length;
    }
    
    public override ValueTask<string> Solve_1() => new($"{FenceCost_with_perimeter(_lines)}");

    public override ValueTask<string> Solve_2() => new($"{FenceCost_with_edge_count(_lines)}");

    private int FenceCost_with_perimeter(string[] plotMap)
    {
        var regions = GetRegionData(plotMap);
        return regions.Values.Select(r => r.Area * r.Perimeter).Sum();
    }
    
    private int FenceCost_with_edge_count(string[] plotMap)
    {
        var regions = GetRegionData(plotMap);
        return regions.Values.Select(r => r.Area * r.EdgeCount).Sum();
    }
    
    private Dictionary<int, Region> GetRegionData(string[] plotMap)
    {
        int regionId = 0;
        Dictionary<int, Region> regions = new();
        var regionMap = new int[_height, _width];

        for (int y = 0; y < _height; y++)
        for (int x = 0; x < _width; x++)
        {
            // Have we entered a new region?
            if (regionMap[y, x] == 0)
            {
                regions.Add(++regionId, new());
                MapNewRegion(regionId, (x, y), plotMap, regionMap);
            }
            
            TallyPlot((x, y), regionMap, regions);
        }

        return regions;
    }
    
    /// <summary>
    /// Having discovered a new region, map out the region on the regionMap,
    /// using the plotMap as a reference.
    /// </summary>
    /// <param name="regionId">Pre-assigned ID of new region.</param>
    /// <param name="firstPlot">First plot of region discovered.</param>
    /// <param name="plotMap">The given map of different plots and their plant type.</param>
    /// <param name="regionMap">The incomplete map of regions.</param>
    private void MapNewRegion(int regionId, (int X, int Y) firstPlot, string[] plotMap, int[,] regionMap)
    {
        char cropType = plotMap[firstPlot.Y][firstPlot.X];
        var plotsToExplore = new Queue<(int X, int Y)>();
        plotsToExplore.Enqueue(firstPlot);

        while (plotsToExplore.Count > 0)
        {
            var plot = plotsToExplore.Dequeue();

            if (plot.X < 0 || plot.X >= _width || plot.Y < 0 || plot.Y >= _height) continue; // Left the map.
            if (plotMap[plot.Y][plot.X] != cropType) continue; // Left the region; stop exploring this way.
            if (regionMap[plot.Y, plot.X] == regionId) continue; // Already mapped it.
            
            regionMap[plot.Y, plot.X] = regionId;
            
            // Find neighbors in the same region to explore.
            plotsToExplore.Enqueue((plot.X, plot.Y - 1));
            plotsToExplore.Enqueue((plot.X + 1, plot.Y));
            plotsToExplore.Enqueue((plot.X, plot.Y + 1));
            plotsToExplore.Enqueue((plot.X - 1, plot.Y));
        }
    }
    
    private void TallyPlot((int X, int Y) plot, int[,] regionMap, Dictionary<int, Region> regions)
    {
        int regionId = regionMap[plot.Y, plot.X];
        var region = regions[regionId];
        region.Area++;
        foreach (var edge in Edges(plot, regionMap))
        {
            region.Perimeter++;
            region.AddEdge(edge);
        }
    }
    
    private IEnumerable<Edge> Edges((int X, int Y) plot, int[,] regionMap)
    {
        int regionId = regionMap[plot.Y, plot.X];
        
        if (plot.Y == 0 || regionMap[plot.Y - 1, plot.X] != regionId)
        {
            yield return new Edge { Type = EdgeType.Top, Level = plot.Y, Start = plot.X, End = plot.X };
        }
        
        if (plot.X == _width - 1 || regionMap[plot.Y, plot.X + 1] != regionId)
        {
            yield return new Edge { Type = EdgeType.Right, Level = plot.X, Start = plot.Y, End = plot.Y };
        }

        if (plot.Y == _height - 1 || regionMap[plot.Y + 1, plot.X] != regionId)
        {
            yield return new Edge { Type = EdgeType.Bottom, Level = plot.Y, Start = plot.X, End = plot.X };
        }

        if (plot.X == 0 || regionMap[plot.Y, plot.X - 1] != regionId)
        {
            yield return new Edge { Type = EdgeType.Left, Level = plot.X, Start = plot.Y, End = plot.Y };
        }
    }
}
