using Labyrinths.Core.Models;
using Labyrinths.Shell;
using System.Globalization;

namespace Labyrinths.CLI.Solve;

/// <summary>Парсер аргументов для команды <c>solve</c></summary>
public static class SolveCommandArgsParser
{
    /// <summary>
    /// Пытается разобрать и провалидировать аргументы контекста <see cref="CommandContext"/>
    /// в структуру <see cref="SolveCommandArgs"/>
    /// </summary>
    public static bool TryParse(CommandContext ctx, out SolveCommandArgs? opts, out List<string> errors)
    {
        errors = [];
        opts = null;

        if (!ctx.Options.TryGetValue("algorithm", out var algo) || string.IsNullOrWhiteSpace(algo))
        {
            errors.Add("Не указан алгоритм (--algorithm astar|dijkstra)");
        }

        if (!ctx.Options.TryGetValue("file", out var file) || string.IsNullOrWhiteSpace(file))
        {
            errors.Add("Не указан файл лабиринта (--file path)");
        }

        if (!TryPoint(ctx.Options.GetValueOrDefault("start"), out var start))
        {
            errors.Add("Некорректная точка старта (--start x,y)");
        }

        if (!TryPoint(ctx.Options.GetValueOrDefault("end"), out var end))
        {
            errors.Add("Некорректная точка финиша (--end x,y)");
        }

        if (errors.Count > 0)
        {
            return false;
        }

        var useUnicode = ctx.Options.ContainsKey("unicode");
        var useFrame = ctx.Options.ContainsKey("frame");
        var showTerrain = ctx.Options.ContainsKey("showterrain");
        var output = ctx.Options.GetValueOrDefault("output");
        var costs = ctx.Options.GetValueOrDefault("costs");

        opts = new SolveCommandArgs(
            Algorithm: algo!,
            File: file!,
            Start: new Point(start.X, start.Y),
            End: new Point(end.X, end.Y),
            UseUnicode: useUnicode,
            UseFrame: useFrame,
            ShowTerrain: showTerrain,
            Output: string.IsNullOrWhiteSpace(output) ? null : output,
            CostsPath: string.IsNullOrWhiteSpace(costs) ? null : costs
        );

        return true;
    }

    /// <summary>
    /// Проверяет, что старт и финиш находятся в пределах лабиринта и проходимы.
    /// </summary>
    private static bool TryPoint(string? value, out (int X, int Y) p)
    {
        p = default;
        
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parts = value.Split(',', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length != 2)
        {
            return false;
        }

        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var x))
        {
            return false;
        }

        if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var y))
        {
            return false;
        }

        p = (x, y); return true;
    }
}
