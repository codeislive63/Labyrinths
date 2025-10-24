using System.Text;
using Labyrinths.Core.Models;
using Labyrinths.Engine.Abstractions;
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
        var sb = new StringBuilder(maze.Height * (maze.Width + 1));

        for (var y = 0; y < maze.Height; y++)
        {
            for (var x = 0; x < maze.Width; x++)
            {
                var ch = maze[x, y] == CellType.Wall ? Wall : Space;

                sb.Append(ch);
            }

            sb.Append('\n');
        }

        var lines = sb.ToString().Split('\n').ToList();

        if (lines.Count > 0 && lines[^1] == string.Empty)
        {
            lines.RemoveAt(lines.Count - 1);
        }

        if (path is not null && path.IsFound)
        {
            foreach (var p in path.Steps)
            {
                if (!p.Equals(start) && !p.Equals(end))
                {
                    Replace(lines, p.X, p.Y, PathChar);
                }
            }
        }

        if (start is Point s)
        {
            Replace(lines, s.X, s.Y, StartChar);
        }

        if (end is Point e)
        {
            Replace(lines, e.X, e.Y, EndChar);
        }

        return string.Join('\n', lines) + '\n';
    }

    private static void Replace(List<string> lines, int x, int y, char c)
    {
        var line = lines[y].ToCharArray();
        line[x] = c;

        lines[y] = new string(line);
    }
}
