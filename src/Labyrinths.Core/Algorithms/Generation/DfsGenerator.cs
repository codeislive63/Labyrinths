using Labyrinths.Core.Abstractions;
using Labyrinths.Core.Algorithms.Generation.Utils;
using Labyrinths.Core.Models;
using Labyrinths.Core.Randomization;

namespace Labyrinths.Core.Algorithms.Generation;

/// <summary>Генерация лабиринта методом поиска в глубину</summary>
public sealed class DfsGenerator : IMazeGenerator
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
        var stack = new Stack<Point>();

        Carve(maze, start);
        stack.Push(start);

        var dirs = (ValueTuple<int, int>[])Dirs.Clone();

        while (stack.Count > 0)
        {
            var current = stack.Peek();
            rnd.Shuffle(dirs);
            var carved = false;

            foreach (var (dx, dy) in dirs)
            {
                var next = current.Jump2(dx, dy);

                if (!maze.InBounds(next.X, next.Y))
                {
                    continue;
                }

                if (maze[next.X, next.Y] == CellType.Wall)
                {
                    var mid = new Point(current.X + dx, current.Y + dy);
                    Carve(maze, mid);
                    Carve(maze, next);
                    stack.Push(next);
                    carved = true;
                    break;
                }
            }

            if (!carved)
            {
                stack.Pop();
            }
        }

        MazePostProcessor.EnsureCornerEntrances(maze);

        return maze;
    }

    private static void Carve(Maze maze, Point point) => maze[point.X, point.Y] = CellType.Empty;
}
