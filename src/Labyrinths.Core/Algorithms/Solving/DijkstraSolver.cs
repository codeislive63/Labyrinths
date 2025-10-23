using Labyrinths.Core.Abstractions;
using Labyrinths.Core.Models;
using Path = Labyrinths.Core.Models.Path;

namespace Labyrinths.Core.Algorithms.Solving;

/// <summary>Поиск кратчайшего пути методом Дейкстры</summary>
public sealed class DijkstraSolver : IPathSolver
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

        var dist = new Dictionary<Point, int> { [start] = 0 };
        var prev = new Dictionary<Point, Point>();
        var pq = new PriorityQueue<Point, int>();

        pq.Enqueue(start, 0);

        while (pq.TryDequeue(out var u, out _))
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

                var nd = dist[u] + 1;

                if (!dist.TryGetValue(v, out var cur) || nd < cur)
                {
                    dist[v] = nd;
                    prev[v] = u;
                    pq.Enqueue(v, nd);
                }
            }
        }

        return null;
    }

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
