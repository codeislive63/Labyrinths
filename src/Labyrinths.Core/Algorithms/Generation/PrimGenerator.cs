using Labyrinths.Core.Abstractions;
using Labyrinths.Core.Algorithms.Generation.Utils;
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
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Значение width должно быть > 0");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Значение height должно быть > 0");
        }

        var rnd = new RandomSource(seed);
        var maze = new Maze(width, height, CellType.Wall);

        if (width == 1 && height == 1)
        {
            maze[0, 0] = CellType.Empty;
            return maze;
        }

        var start = new Point(rnd.Next(0, width), rnd.Next(0, height));
        var frontier = new List<Point>();
        var frontierSet = new HashSet<Point>();

        maze[start.X, start.Y] = CellType.Empty;
        AddFrontier(maze, start, frontier, frontierSet);

        while (frontier.Count > 0)
        {
            var idx = rnd.Next(0, frontier.Count);
            var cell = frontier[idx];

            frontier.RemoveAt(idx);
            frontierSet.Remove(cell);

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
                AddFrontier(maze, cell, frontier, frontierSet);
            }
        }

        MazePostProcessor.EnsureCornerEntrances(maze);

        return maze;
    }

    private static void AddFrontier(Maze maze, Point cell, 
        List<Point> frontier,
        HashSet<Point> frontierSet)
    {
        foreach (var (dx, dy) in Dirs)
        {
            var n = new Point(cell.X + dx, cell.Y + dy);

            if (maze.InBounds(n.X, n.Y) && maze[n.X, n.Y] == CellType.Wall)
            {
                if (frontierSet.Add(n))
                {
                    frontier.Add(n);
                }
            }
        }
    }
}
