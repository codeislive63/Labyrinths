using Labyrinths.Core.Abstractions;
using Labyrinths.Core.Models;
using Path = Labyrinths.Core.Models.Path;

namespace Labyrinths.Core.Algorithms.Solving;

/// <summary>Поиск пути A* с манхэттенской эвристикой</summary>
public sealed class AStarSolver : IPathSolver
{
    /// <inheritdoc/>
    public Path? Solve(Maze maze, Point start, Point end)
    {
        if (!maze.InBounds(start.X, start.Y) || !maze.InBounds(end.X, end.Y))
        {
            return null;
        }

        if (!maze.IsPassable(start.X, start.Y) || !maze.IsPassable(end.X, end.Y))
        {
            return null;
        }

        var g = new Dictionary<Point, int> { [start] = 0 };
        var prev = new Dictionary<Point, Point>();
        var open = new PriorityQueue<Point, int>();

        open.Enqueue(start, Manhattan(start, end));

        while (open.TryDequeue(out var u, out _))
        {
            if (u.Equals(end))
            {
                return Reconstruct(prev, start, end);
            }

            foreach (var v in u.Neighbors4())
            {
                if (!maze.InBounds(v.X, v.Y) || !maze.IsPassable(v.X, v.Y))
                {
                    continue;
                }

                var tentative = g[u] + 1;

                if (!g.TryGetValue(v, out var cur) || tentative < cur)
                {
                    g[v] = tentative;
                    prev[v] = u;
                    var f = tentative + Manhattan(v, end);
                    open.Enqueue(v, f);
                }
            }
        }

        return null;
    }

    private static int Manhattan(Point a, Point b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

    private static Path? Reconstruct(Dictionary<Point, Point> prev, Point start, Point end)
    {
        var list = new List<Point>();
        var cur = end;

        list.Add(cur);

        while (!cur.Equals(start))
        {
            if (!prev.TryGetValue(cur, out cur))
            {
                return null;
            }

            list.Add(cur);
        }

        list.Reverse();

        return new Path(list);
    }
}
