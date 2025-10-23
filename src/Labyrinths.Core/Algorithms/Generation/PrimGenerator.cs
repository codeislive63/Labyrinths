using Labyrinths.Core.Abstractions;
using Labyrinths.Core.Models;
using Labyrinths.Core.Randomization;

namespace Labyrinths.Core.Algorithms.Generation;

/// <summary>Генерация лабиринта алгоритмом Прима</summary>
public sealed class PrimGenerator : IMazeGenerator
{
    private static readonly (int dx, int dy)[] Dirs = [(1, 0), (-1, 0), (0, 1), (0, -1)];

    /// <inheritdoc/>
    public Maze Generate(int width, int height, int? seed = null)
    {
        var rnd = new RandomSource(seed);
        var maze = new Maze(width, height, CellType.Wall);

        if (width == 1 && height == 1)
        {
            maze[0, 0] = CellType.Empty;
            return maze;
        }

        var start = new Point(rnd.Next(0, width), rnd.Next(0, height));
        var frontier = new List<(Point cell, Point from)>();

        maze[start.X, start.Y] = CellType.Empty;
        AddFrontier(maze, start, frontier);

        while (frontier.Count > 0)
        {
            var idx = rnd.Next(0, frontier.Count);
            var (cell, _) = frontier[idx];

            frontier.RemoveAt(idx);

            var passableNeighbors = 0;

            foreach (var (dx, dy) in Dirs)
            {
                var n = new Point(cell.X + dx, cell.Y + dy);

                if (maze.InBounds(n.X, n.Y) && maze[n.X, n.Y] == CellType.Empty)
                {
                    passableNeighbors++;
                }
            }

            if (passableNeighbors == 1)
            {
                maze[cell.X, cell.Y] = CellType.Empty;
                AddFrontier(maze, cell, frontier);
            }
        }

        return maze;
    }

    private static void AddFrontier(Maze maze, Point cell, List<(Point cell, Point from)> frontier)
    {
        foreach (var (dx, dy) in Dirs)
        {
            var n = new Point(cell.X + dx, cell.Y + dy);

            if (maze.InBounds(n.X, n.Y) && maze[n.X, n.Y] == CellType.Wall)
            {
                frontier.Add((n, cell));
            }
        }
    }
}
