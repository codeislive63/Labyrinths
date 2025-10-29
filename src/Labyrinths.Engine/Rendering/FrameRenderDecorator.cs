using Labyrinths.Core.Models;
using Labyrinths.Engine.Abstractions;
using System.Text;
using Path = Labyrinths.Core.Models.Path;

namespace Labyrinths.Engine.Rendering;

/// <summary>Декоратор, добавляющий рамку вокруг результата другого рендера</summary>
public sealed class FrameRendererDecorator(IMazeRenderer inner, char wallChar) : IMazeRenderer
{
    private readonly IMazeRenderer _inner = inner;
    private readonly char _wall = wallChar;

    /// <inheritdoc/>
    public string Render(Maze maze, Point? start = null, Point? end = null, Path? path = null)
    {
        var body = _inner.Render(maze, start, end, path);

        var lines = body.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var h = lines.Length;
        var w = lines.Max(l => l.Length);

        var framed = new char[h + 2, w + 2];

        for (int x = 0; x < w + 2; x++)
        {
            framed[0, x] = _wall;
            framed[h + 1, x] = _wall;
        }

        for (int y = 0; y < h + 2; y++)
        {
            framed[y, 0] = _wall;
            framed[y, w + 1] = _wall;
        }

        for (int y = 0; y < h; y++)
        {
            var line = lines[y].PadRight(w, ' ');

            for (int x = 0; x < w; x++)
            {
                framed[y + 1, x + 1] = line[x];
            }
        }

        var sb = new StringBuilder((w + 2) * (h + 2 + 1));

        for (int y = 0; y < h + 2; y++)
        {
            for (int x = 0; x < w + 2; x++)
            {
                sb.Append(framed[y, x]);
            }

            sb.Append('\n');
        }

        return sb.ToString();
    }
}