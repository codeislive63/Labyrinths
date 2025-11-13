using Labyrinths.Shell;

namespace Labyrinths.CLI.Generate;

/// <summary>
/// Парсер аргументов для команды <c>generate</c>
/// </summary>
public static class GenerateCommandArgsParser
{
    /// <summary>
    /// Пытается разобрать и провалидировать аргументы контекста <see cref="CommandContext"/>
    /// в структуру <see cref="GenerateCommandArgs"/>s
    /// </summary>
    public static bool TryParse(CommandContext ctx, out GenerateCommandArgs? opts, out List<string> errors)
    {
        errors = [];
        opts = null;

        if (!ctx.Options.TryGetValue("algorithm", out var algo) || string.IsNullOrWhiteSpace(algo))
        {
            errors.Add("Не указан алгоритм генерации (--algorithm dfs|prim)");
        }

        if (!TryPositiveInt(ctx.Options.GetValueOrDefault("width"), out var width))
        {
            errors.Add("Некорректная ширина (--width > 0)");
        }

        if (!TryPositiveInt(ctx.Options.GetValueOrDefault("height"), out var height))
        {
            errors.Add("Некорректная высота (--height > 0)");
        }

        if (errors.Count > 0)
        {
            return false;
        }

        var useUnicode = ctx.Options.ContainsKey("unicode");
        var useFrame = ctx.Options.ContainsKey("frame");
        var output = ctx.Options.GetValueOrDefault("output");

        opts = new GenerateCommandArgs(
            Algorithm: algo!,
            Width: width,
            Height: height,
            UseUnicode: useUnicode,
            UseFrame: useFrame,
            Output: string.IsNullOrWhiteSpace(output) ? null : output
        );

        return true;
    }

    private static bool TryPositiveInt(string? s, out int v)
        => int.TryParse(s, out v) && v > 0;
}
