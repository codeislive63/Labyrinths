using Labyrinths.Core.Models;
using Labyrinths.Engine.Abstractions;
using Labyrinths.Engine.Rendering.Utils;
using System.Text;
using Path = Labyrinths.Core.Models.Path;

namespace Labyrinths.Engine.Rendering;

/// <summary>ASCII рендер</summary>
public sealed class AsciiRenderer : IMazeRenderer
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
                if (!p.Equals(start) && !p.Equals(end) && maze.InBounds(p.X, p.Y))
                {
                    grid[p.Y, p.X] = '.';
                }
            }
        }

        if (start is Point s && maze.InBounds(s.X, s.Y))
        {
            grid[s.Y, s.X] = 'O';
        }

        if (end is Point e && maze.InBounds(e.X, e.Y))
        {
            grid[e.Y, e.X] = 'X';
        }

        return GridStringBuilder.ToString(grid);
    }
}
