using FluentAssertions;
using Labyrinths.Core.Models;
using Labyrinths.Engine.Converters;

namespace Labyrinths.Tests.Engine;

/// <summary>Тесты для ASCII-конвертера лабиринта</summary>
public class AsciiConverterTests
{
    /// <summary>Корректно рендерит лабиринт в текст</summary>
    [Fact]
    public void FromModelToFormat_Should_Render_Width_By_Height_With_NewLines()
    {
        var maze = new Maze(4, 3);

        for (int y = 0; y < maze.Height; y++)
        {
            for (int x = 0; x < maze.Width; x++)
            {
                maze[x, y] = CellType.Empty;
            }
        }

        maze[0, 0] = CellType.Wall;
        maze[2, 0] = CellType.Wall;
        maze[1, 1] = CellType.Wall;
        maze[2, 1] = CellType.Wall;
        maze[0, 2] = CellType.Wall;
        maze[1, 2] = CellType.Wall;

        var conv = new AsciiConverter();
        var s = conv.FromModelToFormat(maze);

        var lines = s.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.Should().Be(3);
        lines.Should().AllSatisfy(l => l.Length.Should().Be(4));
        lines[0].Should().Be("# # ");
        lines[1].Should().Be(" ## ");
        lines[2].Should().Be("##  ");
    }

    /// <summary>Парсинг текстового представления должен восстанавливать исходные клетки лабиринта</summary>
    [Fact]
    public void FromFormatToModel_Should_Parse_Back_To_The_Same_Cells()
    {
        var text = """
                   # # 
                    ## 
                   ##  
                   """;

        var conv = new AsciiConverter();
        var maze = conv.FromFormatToModel(text);

        maze.Width.Should().Be(4);
        maze.Height.Should().Be(3);
        maze[0, 0].Should().Be(CellType.Wall);
        maze[1, 0].Should().Be(CellType.Empty);
        maze[2, 0].Should().Be(CellType.Wall);
        maze[3, 0].Should().Be(CellType.Empty);
    }

    /// <summary>Пустой ввод должен вызывать исключение ArgumentException</summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FromFormatToModel_Should_Throw_On_Empty_Input(string data)
    {
        var conv = new AsciiConverter();
        Action act = () => conv.FromFormatToModel(data);
        act.Should().Throw<ArgumentException>();
    }
}
