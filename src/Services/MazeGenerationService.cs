using Labyrinths.Core.Abstractions;
using Labyrinths.Engine.Abstractions;
using Labyrinths.Shell;
using Path = System.IO.Path;

namespace Labyrinths.Services;

/// <summary>Сервис генерации лабиринта</summary>
public sealed class MazeGenerationService(
    IEnumerable<IMazeGenerator> generators,
    Func<bool, bool, IMazeRenderer> rendererFactory)
{
    private readonly IEnumerable<IMazeGenerator> _generators = generators;
    private readonly Func<bool, bool, IMazeRenderer> _rendererFactory = rendererFactory;

    /// <summary>Запускает процесс генерации с параметрами из контекста CLI</summary>
    public async Task<int> RunAsync(CommandContext context)
    {
        if (!context.Options.TryGetValue("algorithm", out var algo))
        {
            Console.Error.WriteLine("Не указан алгоритм генерации (--algorithm dfs|prim)");
            return 1;
        }

        if (!TryParsePositiveInt(context.Options.GetValueOrDefault("width"), out var width) ||
            !TryParsePositiveInt(context.Options.GetValueOrDefault("height"), out var height))
        {
            Console.Error.WriteLine("Некорректные размеры (--width/--height > 0)");
            return 1;
        }

        var generator = _generators.FirstOrDefault(g =>
            g.GetType().Name.StartsWith(algo, StringComparison.OrdinalIgnoreCase)
        );

        if (generator is null)
        {
            Console.Error.WriteLine("Неизвестный алгоритм. Используйте dfs или prim.");
            return 1;
        }

        Console.WriteLine($"Генерация лабиринта {width}×{height} ({algo.ToUpper()})...");

        var maze = generator.Generate(width, height);
        var useUnicode = context.Options.ContainsKey("unicode");
        var useFrame = context.Options.ContainsKey("frame");

        var renderer = _rendererFactory(useUnicode, useFrame);
        var preview = renderer.Render(maze);

        if (context.Options.TryGetValue("output", out var output) && !string.IsNullOrWhiteSpace(output))
        {
            var dir = Path.GetDirectoryName(output);

            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await File.WriteAllTextAsync(output, preview);

            Console.WriteLine($"Лабиринт сохранён в файл: {output}");
        }
        else
        {
            Console.WriteLine(preview);
        }

        return 0;
    }

    private static bool TryParsePositiveInt(string? value, out int n)
        => int.TryParse(value, out n) && n > 0;
}
