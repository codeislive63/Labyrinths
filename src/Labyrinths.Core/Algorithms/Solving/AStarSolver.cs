using Labyrinths.Core.Abstractions;
using Labyrinths.Core.Models;
using Path = Labyrinths.Core.Models.Path;

namespace Labyrinths.Core.Algorithms.Solving;

/// <summary>A* по 4-соседям</summary>
public sealed class AStarSolver(ITraversalCostAccessor costAccessor) : IPathSolver
{
    private readonly ITraversalCostAccessor _costAccessor = costAccessor;

    private static readonly (int dx, int dy)[] Dirs = [(1, 0), (-1, 0), (0, 1), (0, -1)];

    public Path? Solve(Maze maze, Point start, Point end)
    {
        var provider = _costAccessor.Current;
        double minStep = provider.MinStepCost;
        double H(Point p) => (Math.Abs(p.X - end.X) + Math.Abs(p.Y - end.Y)) * minStep;

        var gScore = new Dictionary<Point, double> { [start] = 0.0 };
        var cameFrom = new Dictionary<Point, Point>();

        var open = new PriorityQueue<Point, double>();
        open.Enqueue(start, H(start));

        while (open.TryDequeue(out var current, out _))
        {
            if (current.Equals(end))
            {
                return Reconstruct(cameFrom, current);
            }

            foreach (var n in Neighbors4(maze, current))
            {
                var step = provider.GetCost(maze, n.X, n.Y);

                if (double.IsInfinity(step))
                {
                    continue;
                }

                var tentative = gScore[current] + step;

                if (!gScore.TryGetValue(n, out var old) || tentative < old)
                {
                    gScore[n] = tentative;
                    cameFrom[n] = current;
                    open.Enqueue(n, tentative + H(n));
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

    private static Path Reconstruct(Dictionary<Point, Point> cameFrom, Point last)
    {
        var list = new List<Point> { last };

        while (cameFrom.TryGetValue(last, out var prev))
        {
            last = prev;
            list.Add(last);
        }

        list.Reverse();
        return new Path(list);
    }
}
