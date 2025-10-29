using Labyrinths.Core.Models;

namespace Labyrinths.Core.Algorithms.Generation.Utils;

/// <summary>
/// Постобработчик лабиринта после генерации.
/// Отвечает за создание гарантированных входа и выхода в углах,
/// чтобы лабиринт всегда имел проходимый старт и финиш
/// </summary>
public static class MazePostProcessor
{
    /// <summary>Гарантирует проходимые вход и выход в углах и соединяет их с внутренними коридорами</summary>
    public static void EnsureCornerEntrances(Maze maze)
    {
        var w = maze.Width;
        var h = maze.Height;

        maze[0, 0] = CellType.Empty;

        if (!TryTunnelToCorridor(maze, 0, 0, +1, 0))
        {
            TryTunnelToCorridor(maze, 0, 0, 0, +1);
        }

        var ex = w - 1;
        var ey = h - 1;

        maze[ex, ey] = CellType.Empty;

        if (!TryTunnelToCorridor(maze, ex, ey, -1, 0))
        {
            TryTunnelToCorridor(maze, ex, ey, 0, -1);
        }
    }

    private static bool TryTunnelToCorridor(Maze maze, int x, int y, int dx, int dy)
    {
        var x1 = x + dx;
        var y1 = y + dy;
        var x2 = x + 2 * dx;
        var y2 = y + 2 * dy;

        if (maze.InBounds(x2, y2) && maze[x2, y2] == CellType.Empty)
        {
            maze[x1, y1] = CellType.Empty;
            maze[x2, y2] = CellType.Empty;

            return true;
        }

        if (maze.InBounds(x1, y1) && maze[x1, y1] == CellType.Wall)
        {
            maze[x1, y1] = CellType.Empty;
        }

        return false;
    }
}
