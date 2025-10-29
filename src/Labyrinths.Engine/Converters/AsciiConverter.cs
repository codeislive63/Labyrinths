using System.Text;
using Labyrinths.Core.Models;
using Labyrinths.Engine.Abstractions;

namespace Labyrinths.Engine.Converters;

/// <summary>Конвертер лабиринта в ASCII (#/space) и обратно</summary>
public sealed class AsciiConverter : IMazeConverter<string, Maze>
{
    /// <inheritdoc/>
    public string FromModelToFormat(Maze maze)
    {
        var sb = new StringBuilder(maze.Height * (maze.Width + 1));

        for (var y = 0; y < maze.Height; y++)
        {
            for (var x = 0; x < maze.Width; x++)
            {
                sb.Append(maze[x, y] == CellType.Wall ? '#' : ' ');
            }

            sb.Append('\n');
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
    public Maze FromFormatToModel(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            throw new ArgumentException("Пустой текст лабиринта", nameof(data));
        }

        var rawLines = data.Replace("\r", string.Empty)
                           .Split('\n')
                           .Where(l => l.Length > 0)
                           .ToList();

        if (rawLines.Count == 0)
        {
            throw new ArgumentException("Пустой текст лабиринта", nameof(data));
        }

        static bool IsEmptyChar(char ch) => ch is ' ' or '.' or 'O' or 'X';
        static bool IsWallChar(char ch) => !IsEmptyChar(ch);

        bool LooksLikeFramed(IReadOnlyList<string> lines)
        {
            var h = lines.Count;
            var w = lines.Max(l => l.Length);

            if (h < 3 || w < 3)
            {
                return false;
            }

            var padded = lines.Select(l => l.PadRight(w, '#')).ToArray();

            if (!padded[0].All(IsWallChar))
            {
                return false;
            }

            if (!padded[h - 1].All(IsWallChar))
            {
                return false;
            }

            for (int y = 0; y < h; y++)
            {
                if (!IsWallChar(padded[y][0]))
                {
                    return false;
                }

                if (!IsWallChar(padded[y][w - 1]))
                {
                    return false;
                }
            }

            return true;
        }

        if (LooksLikeFramed(rawLines))
        {
            var trimmed = new List<string>(capacity: Math.Max(0, rawLines.Count - 2));

            for (int y = 1; y < rawLines.Count - 1; y++)
            {
                var line = rawLines[y];

                if (line.Length >= 2)
                {
                    trimmed.Add(line[1..^1]);
                }
            }

            rawLines = trimmed;
        }

        var height = rawLines.Count;
        var width = rawLines.Max(l => l.Length);

        if (width <= 0 || height <= 0)
        {
            throw new ArgumentException("Некорректные размеры лабиринта", nameof(data));
        }

        var maze = new Maze(width, height);

        for (int y = 0; y < height; y++)
        {
            var line = rawLines[y].PadRight(width, '#');

            for (int x = 0; x < width; x++)
            {
                var ch = line[x];
                maze[x, y] = IsEmptyChar(ch) ? CellType.Empty : CellType.Wall;
            }
        }

        return maze;
    }
}
