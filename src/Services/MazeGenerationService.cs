using Labyrinths.CLI.Generate;
using Labyrinths.Core.Abstractions;
using Labyrinths.Engine.Abstractions;
using Labyrinths.Extensions;
using Labyrinths.Shell;

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
        if (!GenerateCommandArgsParser.TryParse(context, out var opts, out var errors))
        {
            foreach (var e in errors)
            {
                Console.Error.WriteLine(e);
            }

            return 1;
        }

        var generator = ResolveGenerator(opts!.Algorithm);

        if (generator is null)
        {
            Console.Error.WriteLine("Неизвестный алгоритм. Используйте dfs или prim.");
            return 1;
        }

        Console.WriteLine($"Генерация лабиринта {opts.Width}×{opts.Height} ({opts.Algorithm.ToUpper()})...");

        var maze = generator.Generate(opts.Width, opts.Height);
        var renderer = _rendererFactory(opts.UseUnicode, opts.UseFrame);
        var preview = renderer.Render(maze);

        if (opts.Output is { Length: > 0 } path)
        {
            path.EnsureParentDirectory();
            await File.WriteAllTextAsync(path, preview);
            Console.WriteLine($"Лабиринт сохранён в файл: {path}");
        }
        else
        {
            Console.WriteLine(preview);
        }

        return 0;
    }

    private IMazeGenerator? ResolveGenerator(string algo)
        => _generators.FirstOrDefault(g =>
                g.GetType().Name.StartsWith(algo,
                    StringComparison.OrdinalIgnoreCase)
        );
}
