using System.Text;

namespace Labyrinths.Engine.Rendering.Utils;

/// <summary>Утилита для преобразования двумерного массива символов в строку</summary>
public static class GridStringBuilder
{
    /// <summary>Преобразует двумерный массив символов в строку с переводами строк</summary>
    public static string ToString(char[,] grid, char newline = '\n')
    {
        var sb = new StringBuilder(grid.GetLength(0) * (grid.GetLength(1) + 1));
        AppendTo(sb, grid, newline);
        return sb.ToString();
    }

    /// <summary>Добавляет содержимое двумерного массива символов в существующий StringBuilder</summary>
    public static void AppendTo(StringBuilder sb, char[,] grid, char newline = '\n')
    {
        var h = grid.GetLength(0);
        var w = grid.GetLength(1);

        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                sb.Append(grid[y, x]);
            }

            sb.Append(newline);
        }
    }
}
