using FluentAssertions;
using Labyrinths.Core.Abstractions;
using Labyrinths.Core.Models;
using Labyrinths.Engine.Abstractions;
using Labyrinths.Services;
using Labyrinths.Shell;
using Path = System.IO.Path;

namespace Labyrinths.Tests.App;

/// <summary>Тесты сервиса генерации лабиринта</summary>
public class MazeGenerationServiceTests
{
    /// <summary>DFS-заглушка: создаёт пустой лабиринт нужного размера</summary>
    private sealed class DfsGenStub : IMazeGenerator
    {
        public Maze Generate(int w, int h, int? seed = null)
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
    }

    /// <summary>PRIM-заглушка: создаёт пустой лабиринт, но ставит одну стену для отличимости</summary>
    private sealed class PrimGenStub : IMazeGenerator
    {
        public Maze Generate(int w, int h, int? seed = null)
        {
            var m = new Maze(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    m[x, y] = CellType.Empty;
                }
            }

            if (w > 1 && h > 1)
            {
                m[1, 1] = CellType.Wall;
            }

            return m;
        }
    }

    /// <summary>Заглушка рендерера, запоминающая переданные флаги и возвращающая фиктивный текст</summary>
    private sealed class RecorderRenderer : IMazeRenderer
    {
        public (bool unicode, bool frame)? LastArgs { get; private set; }

        public string Render(Maze maze, Point? s = null, Point? e = null, Labyrinths.Core.Models.Path? p = null)
        {
            return "RENDERED";
        }

        public void SetFlags(bool unicode, bool frame) => LastArgs = (unicode, frame);
    }

    private static RecorderRenderer MakeFactory(RecorderRenderer rec, bool u, bool f)
    {
        rec.SetFlags(u, f);
        return rec;
    }

    /// <summary>Должен возвращать код ошибки при отсутствии обязательных аргументов</summary>
    [Fact]
    public async Task Run_Should_Validate_Args_And_Return_Errors()
    {
        var renderer = new RecorderRenderer();

        IMazeRenderer Factory(bool u, bool f) => MakeFactory(renderer, u, f);

        var svc = new MazeGenerationService(
            [new DfsGenStub()],
            Factory
        );

        var bad = new CommandContext("generate", new Dictionary<string, string>());
        (await svc.RunAsync(bad)).Should().Be(1);
    }

    /// <summary>DFS: должен сгенерировать, учесть флаги Unicode/Frame и сохранить результат в файл</summary>
    [Fact]
    public async Task Run_Should_Generate_Dfs_And_Save_With_Flags()
    {
        var rec = new RecorderRenderer();

        IMazeRenderer Factory(bool u, bool f) => MakeFactory(rec, u, f);

        var svc = new MazeGenerationService(
            [new DfsGenStub()],
            Factory
        );

        var tmp = Path.Combine(Path.GetTempPath(), $"maze_dfs_{Guid.NewGuid():N}.txt");

        var ctx = new CommandContext("generate", new Dictionary<string, string>
        {
            ["algorithm"] = "dfs",
            ["width"] = "4",
            ["height"] = "3",
            ["output"] = tmp,
            ["unicode"] = "",
            ["frame"] = ""
        });

        var code = await svc.RunAsync(ctx);

        code.Should().Be(0);

        rec.LastArgs.Should().Be((true, true));
        File.Exists(tmp).Should().BeTrue();

        var text = await File.ReadAllTextAsync(tmp);
        text.Should().NotBeNullOrEmpty();

        File.Delete(tmp);
    }

    /// <summary>PRIM: должен подобрать нужный генератор по имени алгоритма и записать файл</summary>
    [Fact]
    public async Task Run_Should_Generate_Prim_And_Save()
    {
        var rec = new RecorderRenderer();
        IMazeRenderer Factory(bool u, bool f) => MakeFactory(rec, u, f);

        var svc = new MazeGenerationService(
            [new DfsGenStub(), new PrimGenStub()],
            Factory
        );

        var tmp = Path.Combine(Path.GetTempPath(), $"maze_prim_{Guid.NewGuid():N}.txt");

        var ctx = new CommandContext("generate", new Dictionary<string, string>
        {
            ["algorithm"] = "prim",
            ["width"] = "6",
            ["height"] = "4",
            ["output"] = tmp
        });

        var code = await svc.RunAsync(ctx);

        code.Should().Be(0);
        File.Exists(tmp).Should().BeTrue();

        var text = await File.ReadAllTextAsync(tmp);
        text.Should().NotBeNullOrEmpty();

        File.Delete(tmp);
    }
}
