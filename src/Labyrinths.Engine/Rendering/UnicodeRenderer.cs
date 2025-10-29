using Labyrinths.Core.Models;
using Labyrinths.Engine.Abstractions;
using Labyrinths.Engine.Rendering.Utils;
using System.Text;
using Path = Labyrinths.Core.Models.Path;

namespace Labyrinths.Engine.Rendering;

/// <summary>Unicode рендер с псевдографикой</summary>
public sealed class UnicodeRenderer : IMazeRenderer
{
    private const char Wall = '█';
    private const char Space = ' ';
    private const char PathChar = '·';
    private const char StartChar = 'O';
    private const char EndChar = 'X';

    /// <inheritdoc/>
    public string Render(Maze maze, Point? start = null, Point? end = null, Path? path = null)
    {
        var grid = new char[maze.Height, maze.Width];

        for (var y = 0; y < maze.Height; ++y)
        {
            for (var x = 0; x < maze.Width; ++x)
            {
                grid[y, x] = maze[x, y] == CellType.Wall ? Wall : Space;
            }
        }

        if (path is not null && path.IsFound)
        {
            foreach (var p in path.Steps)
            {
                if (!p.Equals(start) && !p.Equals(end) && maze.InBounds(p.X, p.Y))
                {
                    grid[p.Y, p.X] = PathChar;
                }
            }
        }

        if (start is Point s && maze.InBounds(s.X, s.Y))
        {
            grid[s.Y, s.X] = StartChar;
        }

        if (end is Point e && maze.InBounds(e.X, e.Y))
        {
            grid[e.Y, e.X] = EndChar;
        }

        return GridStringBuilder.ToString(grid);
    }
}
