using Labyrinths.Core.Abstractions;
using Labyrinths.Core.Models;
using Path = Labyrinths.Core.Models.Path;

namespace Labyrinths.Core.Algorithms.Solving;

/// <summary>Дейкстра по 4-соседям</summary>
public sealed class DijkstraSolver(ITraversalCostAccessor costAccessor) : IPathSolver
{
    private readonly ITraversalCostAccessor _costAccessor = costAccessor;

    private static readonly (int dx, int dy)[] Dirs = [(1, 0), (-1, 0), (0, 1), (0, -1)];

    public Path? Solve(Maze maze, Point start, Point end)
    {
        var provider = _costAccessor.Current;

        var dist = new Dictionary<Point, double> { [start] = 0.0 };
        var prev = new Dictionary<Point, Point>();

        var pq = new PriorityQueue<Point, double>();
        pq.Enqueue(start, 0.0);

        while (pq.TryDequeue(out var u, out _))
        {
            if (u.Equals(end))
            {
                return Reconstruct(prev, u);
            }

            foreach (var v in Neighbors4(maze, u))
            {
                var w = provider.GetCost(maze, v.X, v.Y);

                if (double.IsInfinity(w))
                {
                    continue;
                }

                var alt = dist[u] + w;

                if (!dist.TryGetValue(v, out var dv) || alt < dv)
                {
                    dist[v] = alt;
                    prev[v] = u;
                    pq.Enqueue(v, alt);
                }
            }
        }

        return null;
    }

    private static IEnumerable<Point> Neighbors4(Maze maze, Point p)
    {
        foreach (var (dx, dy) in Dirs)
        {
            var nx = p.X + dx; var ny = p.Y + dy;

            if (maze.InBounds(nx, ny) && maze.IsPassable(nx, ny))
            {
                yield return new Point(nx, ny);
            }
        }
    }

    private static Path? Reconstruct(Dictionary<Point, Point> prev, Point last)
    {
        var list = new List<Point> { last };

        while (prev.TryGetValue(last, out var p))
        {
            last = p;
            list.Add(last);
        }

        list.Reverse();
        return new Path(list);
    }
}
