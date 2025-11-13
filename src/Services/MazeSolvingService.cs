using Labyrinths.CLI.Solve;
using Labyrinths.Core.Abstractions;
using Labyrinths.Core.Models;
using Labyrinths.Core.Traversal;
using Labyrinths.Engine.Abstractions;
using Labyrinths.Engine.Rendering;
using Labyrinths.Extensions;
using Labyrinths.Infrastructure.Providers;
using Labyrinths.Shell;

namespace Labyrinths.Services;

/// <summary>Сервис решения лабиринта</summary>
public sealed class MazeSolvingService(
    IEnumerable<IPathSolver> solvers,
    Func<bool, bool, IMazeRenderer> rendererFactory,
    IMazeConverter<string, Maze> converter,
    ITraversalCostAccessor costAccessor,
    ITerrainProvider terrainProvider)
{
    private readonly IEnumerable<IPathSolver> _solvers = solvers;
    private readonly Func<bool, bool, IMazeRenderer> _rendererFactory = rendererFactory;
    private readonly IMazeConverter<string, Maze> _converter = converter;
    private readonly ITraversalCostAccessor _costAccessor = costAccessor;
    private readonly ITerrainProvider _terrainProvider = terrainProvider;

    /// <summary>Запускает процесс решения лабиринта с параметрами из CLI</summary>
    public async Task<int> RunAsync(CommandContext context)
    {
        if (!SolveCommandArgsParser.TryParse(context, out var opts, out var errors))
        {
            foreach (var e in errors)
            {
                Console.Error.WriteLine(e);
            }

            return 1;
        }

        var solver = ResolveSolver(opts!.Algorithm);

        if (solver is null)
        {
            Console.Error.WriteLine("Неизвестный алгоритм. Используйте astar или dijkstra.");
            return 1;
        }

        var raw = await File.ReadAllTextAsync(opts.File);
        var maze = _converter.FromFormatToModel(raw);

        var mazeErrors = SolveCommandArgsValidator.Validate(opts, maze).ToList();

        if (mazeErrors.Count != 0)
        {
            mazeErrors.ForEach(Console.Error.WriteLine);
            return 1;
        }

        try
        {
            _costAccessor.Current = opts.CostsPath is { Length: > 0 }
                ? await _terrainProvider.LoadAsync(opts.CostsPath, maze.Width, maze.Height)
                : DefaultCostProvider.Instance;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Не удалось загрузить карту покрытий ({opts.CostsPath}): {ex.Message}");
            return 1;
        }

        Console.WriteLine($"Решение лабиринта {opts.Algorithm.ToUpperInvariant()} " +
                          $"({opts.Start.X},{opts.Start.Y})→({opts.End.X},{opts.End.Y})...");

        var path = solver.Solve(maze, opts.Start, opts.End);

        if (path is null || !path.IsFound)
        {
            Console.Error.WriteLine("Маршрут не найден.");
            return 4;
        }

        string rendered = opts.ShowTerrain && _costAccessor.Current is TerrainCostProvider tp
                ? TerrainVisualizer.Render(maze, tp.Map, opts.Start, opts.End, path,
                    unicode: opts.UseUnicode, frame: opts.UseFrame, legend: true)
                : _rendererFactory(opts.UseUnicode, opts.UseFrame).Render(maze, opts.Start, opts.End, path);

        if (opts.Output is { Length: > 0 } outPath)
        {
            outPath.EnsureParentDirectory();
            await File.WriteAllTextAsync(outPath, rendered);
            Console.WriteLine($"Результат сохранён: {outPath}");
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine(rendered);
        }

        return 0;
    }

    private IPathSolver? ResolveSolver(string algo)
        => _solvers.FirstOrDefault(s =>
                s.GetType().Name.StartsWith(algo,
                    StringComparison.OrdinalIgnoreCase)
        );
}
