using Labyrinths.Core.Abstractions;
using Labyrinths.Core.Models;
using Labyrinths.Core.Traversal;
using Labyrinths.Engine.Abstractions;
using Labyrinths.Engine.Rendering;
using Labyrinths.Shell;
using System.Globalization;
using System.Text.Json;
using Path = System.IO.Path;

namespace Labyrinths.Services;

/// <summary>Сервис решения лабиринта</summary>
public sealed class MazeSolvingService(
    IEnumerable<IPathSolver> solvers,
    Func<bool, bool, IMazeRenderer> rendererFactory,
    IMazeConverter<string, Maze> converter,
    ITraversalCostAccessor costAccessor)
{
    private readonly IEnumerable<IPathSolver> _solvers = solvers;
    private readonly Func<bool, bool, IMazeRenderer> _rendererFactory = rendererFactory;
    private readonly IMazeConverter<string, Maze> _converter = converter;
    private readonly ITraversalCostAccessor _costAccessor = costAccessor;

    private static readonly JsonSerializerOptions CaseInsensitive = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Запускает процесс решения лабиринта с параметрами из CLI</summary>
    public async Task<int> RunAsync(CommandContext context)
    {
        if (!context.Options.TryGetValue("algorithm", out var algo))
        {
            Console.Error.WriteLine("Не указан алгоритм (--algorithm astar|dijkstra)");
            return 1;
        }

        if (!context.Options.TryGetValue("file", out var file) || !File.Exists(file))
        {
            Console.Error.WriteLine("Файл лабиринта не найден (--file path)");
            return 1;
        }

        if (!TryParsePoint(context.Options.GetValueOrDefault("start"), out var start) ||
            !TryParsePoint(context.Options.GetValueOrDefault("end"), out var end))
        {
            Console.Error.WriteLine("Укажите корректные точки: --start x,y --end x,y");
            return 1;
        }

        var solver = _solvers.FirstOrDefault(s =>
            s.GetType().Name.Contains(algo, StringComparison.OrdinalIgnoreCase)
        );

        if (solver is null)
        {
            Console.Error.WriteLine("Неизвестный алгоритм. Используйте astar или dijkstra.");
            return 1;
        }

        var raw = await File.ReadAllTextAsync(file);
        var maze = _converter.FromFormatToModel(raw);

        if (!maze.InBounds(start.X, start.Y) || !maze.IsPassable(start.X, start.Y))
        {
            Console.Error.WriteLine($"Старт вне поля или непроходим: {start.X},{start.Y}");
            return 1;
        }

        if (!maze.InBounds(end.X, end.Y) || !maze.IsPassable(end.X, end.Y))
        {
            Console.Error.WriteLine($"Финиш вне поля или непроходим: {end.X},{end.Y}");
            return 1;
        }

        var useUnicode = context.Options.ContainsKey("unicode");
        var useFrame = context.Options.ContainsKey("frame");
        var showTerrain = context.Options.ContainsKey("showterrain");

        if (context.Options.TryGetValue("costs", out var costsPath) && !string.IsNullOrWhiteSpace(costsPath))
        {
            try
            {
                var terrain = await LoadTerrainAsync(costsPath, maze.Width, maze.Height);
                _costAccessor.Current = terrain;

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Не удалось загрузить карту покрытий ({costsPath}): {ex.Message}");
                return 1;
            }
        }
        else
        {
            _costAccessor.Current = DefaultCostProvider.Instance;
        }

        Console.WriteLine($"Решение лабиринта {algo.ToUpperInvariant()} " +
                          $"({start.X},{start.Y})→({end.X},{end.Y})...");

        var path = solver.Solve(maze, start, end);

        if (path is null || !path.IsFound)
        {
            Console.Error.WriteLine("Маршрут не найден.");
            return 4;
        }

        string rendered;

        if (showTerrain && _costAccessor.Current is TerrainCostProvider tp)
        {
            rendered = TerrainVisualizer.Render(
                maze,
                tp.Map,
                start,
                end,
                path,
                unicode: useUnicode,
                frame: useFrame,
                legend: true);
        }
        else
        {
            var renderer = _rendererFactory(useUnicode, useFrame);
            rendered = renderer.Render(maze, start, end, path);
        }

        if (context.Options.TryGetValue("output", out var output) && !string.IsNullOrWhiteSpace(output))
        {
            var dir = Path.GetDirectoryName(output);

            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await File.WriteAllTextAsync(output, rendered);
            Console.WriteLine($"Результат сохранён: {output}");
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine(rendered);
        }

        return 0;
    }

    // ===== helpers =====

    // Преобразует строку формата "x,y" в Point
    private static bool TryParsePoint(string? value, out Point p)
    {
        p = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        value = value.Trim();
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

        p = new Point(x, y);
        return true;
    }

    // Читает JSON и строит TerrainCostProvider
    private static async Task<ITraversalCostProvider> LoadTerrainAsync(string path, int mazeWidth, int mazeHeight)
    {
        var json = await File.ReadAllTextAsync(path);

        var cells = JsonSerializer.Deserialize<List<CostCell>>(json, CaseInsensitive) ?? [];

        var terrainMap = new TerrainMap(mazeWidth, mazeHeight);

        foreach (var c in cells)
        {
            if (c.X < 0 || c.Y < 0 || c.X >= mazeWidth || c.Y >= mazeHeight)
            {
                continue;
            }

            var type = c.Cost switch
            {
                >= 3.0 => TerrainType.Mud,
                >= 2.0 => TerrainType.Sand,
                >= 1.0 => TerrainType.Normal,
                >= 0.6 => TerrainType.Road,
                _ => TerrainType.Bonus
            };

            terrainMap[c.X, c.Y] = type;
        }

        return new TerrainCostProvider(terrainMap);
    }

    // Простая модель для десериализации JSON
    private sealed record CostCell(int X, int Y, double Cost);
}
