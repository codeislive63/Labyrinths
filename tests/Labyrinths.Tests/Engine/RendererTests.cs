using FluentAssertions;
using Labyrinths.Core.Models;
using Labyrinths.Engine.Rendering;
using Labyrinths.Engine.Rendering.Utils;
using Path = Labyrinths.Core.Models.Path;

namespace Labyrinths.Tests.Engine;

/// <summary>Тесты визуализаторов лабиринта и вспомогательных компонентов рендеринга</summary>
public class RendererTests
{
    private static Maze Open3x3()
    {
        var m = new Maze(3, 3);

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                m[x, y] = CellType.Empty;
            }
        }

        return m;
    }

    private static Path SimplePath() =>
        new([new Point(0, 0), new Point(1, 0), new Point(2, 0)]);

    /// <summary>ASCII-рендер должен корректно отображать старт, финиш и путь</summary>
    [Fact]
    public void AsciiRenderer_Should_Place_Start_End_And_Path()
    {
        var maze = Open3x3();
        var r = new AsciiRenderer();

        var s = r.Render(maze, new Point(0, 0), new Point(2, 0), SimplePath());

        s.Should().Contain("O");
        s.Should().Contain("X");
        s.Should().Contain(".");
    }

    /// <summary>Unicode-рендер использует блоки для стен и пробелы для пустых клеток</summary>
    [Fact]
    public void UnicodeRenderer_Should_Use_Block_For_Walls_And_Space_For_Empty()
    {
        var maze = Open3x3();
        maze[1, 1] = CellType.Wall;

        var r = new UnicodeRenderer();
        var s = r.Render(maze);

        s.Should().Contain("█");
        s.Should().Contain(" ");
    }

    /// <summary>Декоратор рамки добавляет границу вокруг визуализированного лабиринта</summary>
    [Fact]
    public void FrameDecorator_Should_Add_Border()
    {
        var maze = Open3x3();
        var baseR = new AsciiRenderer();
        var framed = new FrameRendererDecorator(baseR, '#');

        var s = framed.Render(maze);

        var lines = s.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.Should().Be(5);
        lines[0].Should().Be("#####");
        lines[^1].Should().Be("#####");
        lines[1][0].Should().Be('#');
        lines[1][^1].Should().Be('#');
    }

    /// <summary>GridStringBuilder должен объединять матрицу символов в текст с переводами строк</summary>
    [Fact]
    public void GridStringBuilder_Should_Join_Char_Matrix_To_Text()
    {
        var grid = new char[2, 3] { { 'a', 'b', 'c' }, { 'd', 'e', 'f' } };
        var txt = GridStringBuilder.ToString(grid);
        txt.Replace("\r", "").Should().Be("abc\ndef\n");
    }
}
