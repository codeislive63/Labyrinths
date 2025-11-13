using FluentAssertions;
using Labyrinths.Core.Abstractions;
using Labyrinths.Core.Models;
using Labyrinths.Core.Traversal;
using Labyrinths.Engine.Abstractions;
using Labyrinths.Infrastructure.Providers;
using Labyrinths.Services;
using Labyrinths.Shell;
using Path = System.IO.Path;

namespace Labyrinths.Tests.App;

/// <summary>Тесты сервиса решения лабиринта (выбор алгоритма, флаги, costs/terrain)</summary>
public class MazeSolvingServiceTests
{
    private static Maze EmptyMaze(int w, int h)
    {
        var m = new Maze(w, h);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                m[x, y] = CellType.Empty;
            }
        }

        return m;
    }

    private static string WriteMazeToTemp(Maze m)
    {
        var file = Path.Combine(Path.GetTempPath(), $"maze_{Guid.NewGuid():N}.txt");
        var conv = new Labyrinths.Engine.Converters.AsciiConverter();

        File.WriteAllText(file, conv.FromModelToFormat(m));
        return file;
    }

    private sealed class RecorderRenderer : IMazeRenderer
    {
        public (bool unicode, bool frame)? LastFlags { get; private set; }
        public string Render(Maze maze, Point? s = null, Point? e = null, Labyrinths.Core.Models.Path? p = null) => "SOLVED";
        public void SetFlags(bool u, bool f) => LastFlags = (u, f);
    }

    private static RecorderRenderer Factory(RecorderRenderer rec, bool u, bool f)
    {
        rec.SetFlags(u, f);
        return rec;
    }

    // Имена классов содержат "AStar" и "Dijkstra", чтобы сервис их корректно выбирал по --algorithm
    private sealed class AStarOkSolver : IPathSolver
    {
        public Labyrinths.Core.Models.Path? Solve(Maze maze, Point start, Point end)
        {
            var steps = new List<Point> { start };
            var p = start;

            while (p.X < end.X)
            {
                p = new Point(p.X + 1, p.Y);
                steps.Add(p);
            }

            while (p.Y < end.Y)
            {
                p = new Point(p.X, p.Y + 1);
                steps.Add(p);
            }

            return new Labyrinths.Core.Models.Path(steps);
        }
    }

    private sealed class DijkstraOkSolver : IPathSolver
    {
        public Labyrinths.Core.Models.Path? Solve(Maze maze, Point start, Point end)
        {
            var steps = new List<Point> { start };
            var p = start;

            while (p.X < end.X)
            {
                p = new Point(p.X + 1, p.Y);
                steps.Add(p);
            }

            while (p.Y < end.Y)
            {
                p = new Point(p.X, p.Y + 1);
                steps.Add(p);
            }

            return new Labyrinths.Core.Models.Path(steps);
        }
    }

    private sealed class DijkstraFailSolver : IPathSolver
    {
        public Labyrinths.Core.Models.Path? Solve(Maze maze, Point start, Point end) => null;
    }

    private sealed class StubTerrainProvider : ITerrainProvider
    {
        public Task<ITraversalCostProvider> LoadAsync(string path, int mazeWidth, int mazeHeight, CancellationToken ct = default)
        {
            var map = new TerrainMap(mazeWidth, mazeHeight);
            return Task.FromResult<ITraversalCostProvider>(new TerrainCostProvider(map));
        }
    }

    /// <summary>Должен вернуть ошибку, если отсутствуют обязательные аргументы</summary>
    [Fact]
    public async Task Run_Should_Return_Error_When_Args_Missing()
    {
        var rec = new RecorderRenderer();
        IMazeRenderer F(bool u, bool f) => Factory(rec, u, f);

        var svc = new MazeSolvingService(
            [new AStarOkSolver()],
            F,
            new Labyrinths.Engine.Converters.AsciiConverter(),
            new TraversalCostAccessor
            {
                Current = DefaultCostProvider.Instance
            },
            new StubTerrainProvider()
        );

        var ctx = new CommandContext("solve", new Dictionary<string, string>());
        (await svc.RunAsync(ctx)).Should().Be(1);
    }

    /// <summary>Должен выбрать A* по имени алгоритма, решить, записать файл и проставить флаги unicode+frame</summary>
    [Fact]
    public async Task Run_Should_Solve_With_AStar_And_Save_With_Unicode_And_Frame()
    {
        var rec = new RecorderRenderer();
        IMazeRenderer F(bool u, bool f) => Factory(rec, u, f);

        var svc = new MazeSolvingService(
            [new AStarOkSolver(), new DijkstraOkSolver()],
            F,
            new Labyrinths.Engine.Converters.AsciiConverter(),
            new TraversalCostAccessor
            {
                Current = DefaultCostProvider.Instance
            },
            new StubTerrainProvider()
        );

        var mazeFile = Path.Combine(Path.GetTempPath(), $"maze_{Guid.NewGuid():N}.txt");
        var outFile = Path.Combine(Path.GetTempPath(), $"sol_{Guid.NewGuid():N}.txt");

        try
        {
            var ascii = string.Join('\n', Enumerable.Repeat(".....", 5)) + "\n";
            await File.WriteAllTextAsync(mazeFile, ascii);

            var ctx = new CommandContext("solve", new Dictionary<string, string>
            {
                ["algorithm"] = "astar",
                ["file"] = mazeFile,
                ["start"] = "0,0",
                ["end"] = "4,4",
                ["output"] = outFile,
                ["unicode"] = "",
                ["frame"] = ""
            });

            var code = await svc.RunAsync(ctx);

            code.Should().Be(0);
            rec.LastFlags.Should().Be((true, true));

            File.Exists(outFile).Should().BeTrue();
            (await File.ReadAllTextAsync(outFile)).Should().Contain("SOLVED");
        }
        finally
        {
            if (File.Exists(mazeFile))
            {
                File.Delete(mazeFile);
            }

            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }
        }
    }

    /// <summary>Должен выбрать Dijkstra по имени алгоритма, сохранить результат и учесть флаг frame без unicode</summary>
    [Fact]
    public async Task Run_Should_Solve_With_Dijkstra_And_Respect_Frame_Only()
    {
        var rec = new RecorderRenderer();
        IMazeRenderer F(bool u, bool f) => Factory(rec, u, f);

        var svc = new MazeSolvingService(
            [new DijkstraOkSolver(), new AStarOkSolver()],
            F,
            new Labyrinths.Engine.Converters.AsciiConverter(),
            new TraversalCostAccessor
            {
                Current = DefaultCostProvider.Instance
            },
            new StubTerrainProvider()
        );

        var mazeFile = Path.Combine(Path.GetTempPath(), $"maze_{Guid.NewGuid():N}.txt");
        var outFile = Path.Combine(Path.GetTempPath(), $"sol_{Guid.NewGuid():N}.txt");

        try
        {
            var ascii = string.Join('\n', Enumerable.Repeat("....", 4)) + "\n";
            await File.WriteAllTextAsync(mazeFile, ascii);

            var ctx = new CommandContext("solve", new Dictionary<string, string>
            {
                ["algorithm"] = "dijkstra",
                ["file"] = mazeFile,
                ["start"] = "0,0",
                ["end"] = "3,3",
                ["output"] = outFile,
                ["frame"] = ""
            });

            var code = await svc.RunAsync(ctx);

            code.Should().Be(0);
            rec.LastFlags.Should().Be((false, true));

            File.Exists(outFile).Should().BeTrue();
            (await File.ReadAllTextAsync(outFile)).Should().Contain("SOLVED");
        }
        finally
        {
            if (File.Exists(mazeFile))
            {
                File.Delete(mazeFile);
            }

            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }
        }
    }

    /// <summary>Должен вернуть код 4, если выбранный алгоритм не находит путь</summary>
    [Fact]
    public async Task Run_Should_Return_4_When_No_Path_Found()
    {
        var rec = new RecorderRenderer();
        IMazeRenderer F(bool u, bool f) => Factory(rec, u, f);

        var svc = new MazeSolvingService(
            [new DijkstraFailSolver()],
            F,
            new Labyrinths.Engine.Converters.AsciiConverter(),
            new TraversalCostAccessor
            {
                Current = DefaultCostProvider.Instance
            },
            new StubTerrainProvider()
        );

        var mazeFile = Path.Combine(Path.GetTempPath(), $"maze_{Guid.NewGuid():N}.txt");

        try
        {
            var ascii = string.Join('\n', Enumerable.Repeat("...", 3)) + "\n";
            await File.WriteAllTextAsync(mazeFile, ascii);

            var ctx = new CommandContext("solve", new Dictionary<string, string>
            {
                ["algorithm"] = "dijkstra",
                ["file"] = mazeFile,
                ["start"] = "0,0",
                ["end"] = "2,2"
            });

            var code = await svc.RunAsync(ctx);

            code.Should().Be(4);
        }
        finally
        {
            if (File.Exists(mazeFile))
            {
                File.Delete(mazeFile);
            }
        }
    }

    /// <summary>Должен принимать карту покрытий --costs и при --showterrain переключать провайдер на TerrainCostProvider</summary>
    [Fact]
    public async Task Run_Should_Apply_Costs_And_ShowTerrain()
    {
        var rec = new RecorderRenderer();
        IMazeRenderer F(bool u, bool f) => Factory(rec, u, f);

        var accessor = new TraversalCostAccessor { Current = DefaultCostProvider.Instance };

        var svc = new MazeSolvingService(
            [new AStarOkSolver()],
            F,
            new Labyrinths.Engine.Converters.AsciiConverter(),
            accessor,
            new StubTerrainProvider()
        );

        var mazeFile = Path.Combine(Path.GetTempPath(), $"maze_{Guid.NewGuid():N}.txt");
        var outFile = Path.Combine(Path.GetTempPath(), $"sol_{Guid.NewGuid():N}.txt");
        var costs = Path.Combine(Path.GetTempPath(), $"terrain_{Guid.NewGuid():N}.json");

        try
        {
            var ascii = string.Join('\n', Enumerable.Repeat(".....", 5)) + "\n";
            await File.WriteAllTextAsync(mazeFile, ascii);

            // карта покрытий
            var json = """
            [
              { "x": 1, "y": 1, "cost": 2.0 },
              { "x": 2, "y": 2, "cost": 0.6 }
            ]
            """;

            await File.WriteAllTextAsync(costs, json);

            var ctx = new CommandContext("solve", new Dictionary<string, string>
            {
                ["algorithm"] = "astar",
                ["file"] = mazeFile,
                ["start"] = "0,0",
                ["end"] = "4,4",
                ["output"] = outFile,
                ["costs"] = costs,
                ["showterrain"] = ""
            });

            var code = await svc.RunAsync(ctx);

            code.Should().Be(0);
            accessor.Current.Should().BeOfType<TerrainCostProvider>();
            File.Exists(outFile).Should().BeTrue();

            var output = await File.ReadAllTextAsync(outFile);

            output.Should().NotBeNullOrEmpty();
            output.Should().NotContain("SOLVED");

            output.Should().Contain("O");
            output.Should().Contain("X");
            output.Should().Contain(".");
            output.ToLowerInvariant().Should().Contain("обозначения");

            output.Should().Contain("~");
            output.Should().Contain("≈");
            output.Should().Contain("=");
            output.Should().Contain("$");
        }
        finally
        {
            if (File.Exists(mazeFile))
            {
                File.Delete(mazeFile);
            }

            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }

            if (File.Exists(costs))
            {
                File.Delete(costs);
            }
        }
    }

    /// <summary>Должен валидировать старт и финиш: непроходимая стартовая клетка возвращает код 1</summary>
    [Fact]
    public async Task Run_Should_Validate_Start_End_Cells()
    {
        var rec = new RecorderRenderer();
        IMazeRenderer F(bool u, bool f) => Factory(rec, u, f);

        var svc = new MazeSolvingService(
            [new AStarOkSolver()],
            F,
            new Labyrinths.Engine.Converters.AsciiConverter(),
            new TraversalCostAccessor
            {
                Current = DefaultCostProvider.Instance
            },
            new StubTerrainProvider()
        );

        var m = EmptyMaze(3, 3);
        m[0, 0] = CellType.Wall;
        var mazeFile = WriteMazeToTemp(m);

        var ctx = new CommandContext("solve", new Dictionary<string, string>
        {
            ["algorithm"] = "astar",
            ["file"] = mazeFile,
            ["start"] = "0,0",
            ["end"] = "2,2"
        });

        var code = await svc.RunAsync(ctx);
        code.Should().Be(1);

        File.Delete(mazeFile);
    }
}
