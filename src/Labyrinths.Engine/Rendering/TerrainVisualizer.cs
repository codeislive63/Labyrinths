using Labyrinths.Core.Models;
using System.Text;
using Labyrinths.Engine.Rendering.Utils;
using PathResult = Labyrinths.Core.Models.Path;

namespace Labyrinths.Engine.Rendering;

/// <summary>Статическая визуализация лабиринта с учётом покрытий</summary>
public static class TerrainVisualizer
{
    public static string Render(
        Maze maze,
        TerrainMap terrain,
        Point? start,
        Point? end,
        PathResult? path,
        bool unicode,
        bool frame,
        bool legend = true)
    {
        var (wall, _, pathCh, startCh, endCh) = unicode
            ? TerrainSymbols.BaseUnicode()
            : TerrainSymbols.BaseAscii();

        char Glyph(TerrainType t) => unicode
            ? TerrainSymbols.GlyphUnicode(t)
            : TerrainSymbols.GlyphAscii(t);

        var inner = new char[maze.Height, maze.Width];

        for (var y = 0; y < maze.Height; y++)
        {
            for (var x = 0; x < maze.Width; x++)
            {
                inner[y, x] = maze[x, y] == CellType.Wall ? wall : Glyph(terrain[x, y]);
            }
        }

        if (path is { IsFound: true })
        {
            foreach (var p in path.Steps)
            {
                if (!p.Equals(start) && !p.Equals(end) && maze.InBounds(p.X, p.Y))
                {
                    inner[p.Y, p.X] = pathCh;
                }
            }
        }

        if (start is Point s && maze.InBounds(s.X, s.Y))
        {
            inner[s.Y, s.X] = startCh;
        }

        if (end is Point e && maze.InBounds(e.X, e.Y))
        {
            inner[e.Y, e.X] = endCh;
        }

        var sb = new StringBuilder();

        if (!frame)
        {
            GridStringBuilder.AppendTo(sb, inner);
        }
        else
        {
            var h = maze.Height + 2;
            var w = maze.Width + 2;
            var framed = new char[h, w];

            for (var x = 0; x < w; x++) { framed[0, x] = wall; framed[h - 1, x] = wall; }
            for (var y = 0; y < h; y++) { framed[y, 0] = wall; framed[y, w - 1] = wall; }

            for (var y = 0; y < maze.Height; y++)
            {
                for (var x = 0; x < maze.Width; x++)
                {
                    framed[y + 1, x + 1] = inner[y, x];
                }
            }

            GridStringBuilder.AppendTo(sb, framed);
        }

        if (legend)
        {
            sb.AppendLine();
            sb.AppendLine(unicode ? TerrainSymbols.LegendUnicode()
                                  : TerrainSymbols.LegendAscii());
        }

        return sb.ToString();
    }
}
