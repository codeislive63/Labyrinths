using System.Text;
using Labyrinths.Core.Models;
using Labyrinths.Engine.Abstractions;

namespace Labyrinths.Engine.Serialization;

/// <summary>Сериализатор лабиринта в ASCII формат и обратно</summary>
public sealed class AsciiMazeConverter : IMazeConverter<string, Maze>
{
    /// <summary>Преобразовать лабиринт в ASCII строку с символами # и пробел</summary>
    public string FromModelToFormat(Maze maze)
    {
        var sb = new StringBuilder(maze.Height * (maze.Width + 1));

        for (var y = 0; y < maze.Height; y++)
        {
            for (var x = 0; x < maze.Width; x++)
            {
                var ch = maze[x, y] == CellType.Wall ? '#' : ' ';
                sb.Append(ch);
            }

            sb.Append('\n');
        }

        return sb.ToString();
    }

    /// <summary>Прочитать лабиринт из ASCII строки</summary>
    public Maze FromFormatToModel(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            throw new ArgumentException("Пустой текст лабиринта", nameof(data));
        }

        var lines = data.Replace("\r", string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var height = lines.Length;
        var width = lines.Max(l => l.Length);

        if (width <= 0 || height <= 0)
        {
            throw new ArgumentException("Некорректные размеры лабиринта", nameof(data));
        }

        var maze = new Maze(width, height);

        for (var y = 0; y < height; y++)
        {
            var line = lines[y].PadRight(width, '#');

            for (var x = 0; x < width; x++)
            {
                var ch = line[x];

                maze[x, y] = ch == '#' ? CellType.Wall : CellType.Empty;
            }
        }

        return maze;
    }
}
