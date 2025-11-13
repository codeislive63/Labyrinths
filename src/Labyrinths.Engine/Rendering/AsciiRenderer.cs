using Labyrinths.Core.Models;
using Labyrinths.Engine.Abstractions;
using Labyrinths.Engine.Rendering.Utils;
using Path = Labyrinths.Core.Models.Path;

namespace Labyrinths.Engine.Rendering;

/// <summary>ASCII рендер</summary>
public sealed class AsciiRenderer : IMazeRenderer
{
    /// <inheritdoc/>
    public string Render(Maze maze, Point? start = null, Point? end = null, Path? path = null)
    {
        var (wall, space, pathCh, startCh, endCh) = TerrainSymbols.BaseAscii();

        var grid = new char[maze.Height, maze.Width];

        for (var y = 0; y < maze.Height; y++)
        {
            for (var x = 0; x < maze.Width; x++)
            {
                grid[y, x] = maze[x, y] == CellType.Wall ? wall : space;
            }
        }

        if (path is { IsFound: true })
        {
            foreach (var p in path.Steps)
            {
                if (!p.Equals(start) && !p.Equals(end) && maze.InBounds(p.X, p.Y))
                {
                    grid[p.Y, p.X] = pathCh;
                }
            }
        }

        if (start is Point s && maze.InBounds(s.X, s.Y))
        {
            grid[s.Y, s.X] = startCh;
        }

        if (end is Point e && maze.InBounds(e.X, e.Y))
        {
            grid[e.Y, e.X] = endCh;
        }

        return GridStringBuilder.ToString(grid);
    }
}
