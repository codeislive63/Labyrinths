using Labyrinths.Core.Models;
using Labyrinths.Engine.Abstractions;
using System.Text;
using Path = Labyrinths.Core.Models.Path;

namespace Labyrinths.Engine.Rendering;

/// <summary>ASCII рендер</summary>
public sealed class AsciiMazeRenderer : IMazeRenderer
{
    /// <inheritdoc/>
    public string Render(Maze maze, Point? start = null, Point? end = null, Path? path = null)
    {
        var grid = new char[maze.Height, maze.Width];

        for (var y = 0; y < maze.Height; y++)
        {
            for (var x = 0; x < maze.Width; x++)
            {
                grid[y, x] = maze[x, y] == CellType.Wall ? '#' : ' ';
            }
        }

        if (path is not null && path.IsFound)
        {
            foreach (var p in path.Steps)
            {
                if (!p.Equals(start) && !p.Equals(end))
                {
                    grid[p.Y, p.X] = '.';
                }
            }
        }

        if (start is Point s)
        {
            grid[s.Y, s.X] = 'O';
        }

        if (end is Point e)
        {
            grid[e.Y, e.X] = 'X';
        }

        return ToString(grid);
    }

    private static string ToString(char[,] grid)
    {
        var height = grid.GetLength(0);
        var width = grid.GetLength(1);
        var sb = new StringBuilder(height * (width + 1));

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                sb.Append(grid[y, x]);
            }

            sb.Append('\n');
        }

        return sb.ToString();
    }
}
