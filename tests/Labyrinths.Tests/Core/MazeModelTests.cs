using FluentAssertions;
using Labyrinths.Core.Models;

namespace Labyrinths.Tests.Core;

/// <summary>Тесты для базовой модели лабиринта Maze</summary>
public class MazeModelTests
{
    /// <summary>Indexer корректно сохраняет и возвращает значения клеток</summary>
    [Fact]
    public void Indexer_Sets_And_Gets_Cells_Correctly()
    {
        var m = new Maze(3, 3);
        m[1, 1] = CellType.Wall;
        m[0, 0] = CellType.Empty;

        m[1, 1].Should().Be(CellType.Wall);
        m[0, 0].Should().Be(CellType.Empty);
    }

    /// <summary>InBounds возвращает истину только для координат внутри размеров</summary>
    [Fact]
    public void InBounds_Returns_True_Only_For_Valid_Coords()
    {
        var m = new Maze(3, 3);

        m.InBounds(0, 0).Should().BeTrue();
        m.InBounds(2, 2).Should().BeTrue();

        m.InBounds(-1, 0).Should().BeFalse();
        m.InBounds(0, -1).Should().BeFalse();
        m.InBounds(3, 0).Should().BeFalse();
        m.InBounds(0, 3).Should().BeFalse();
    }

    /// <summary>IsPassable возвращает истину только для пустых клеток</summary>
    [Fact]
    public void IsPassable_Returns_False_For_Walls()
    {
        var m = new Maze(2, 2);
        m[0, 0] = CellType.Wall;
        m[1, 1] = CellType.Empty;

        m.IsPassable(0, 0).Should().BeFalse();
        m.IsPassable(1, 1).Should().BeTrue();
    }

    /// <summary>Можно заполнить лабиринт полностью стенами и пустыми клетками</summary>
    [Fact]
    public void Can_Fill_Full_Walls_And_Empty()
    {
        var m = new Maze(4, 4);

        for (int y = 0; y < m.Height; y++)
        {
            for (int x = 0; x < m.Width; x++)
            {
                m[x, y] = (x + y) % 2 == 0 ? CellType.Wall : CellType.Empty;
            }
        }

        int walls = 0, empty = 0;

        for (int y = 0; y < m.Height; y++)
        {
            for (int x = 0; x < m.Width; x++)
            {
                if (m[x, y] == CellType.Wall)
                {
                    walls++;
                }
                else if (m[x, y] == CellType.Empty)
                {
                    empty++;
                }
            }
        }

        walls.Should().BeGreaterThan(0);
        empty.Should().BeGreaterThan(0);
    }

    /// <summary>Созданный лабиринт корректно хранит размеры</summary>
    [Fact]
    public void Maze_Stores_Dimensions_Correctly()
    {
        var m = new Maze(10, 7);
        m.Width.Should().Be(10);
        m.Height.Should().Be(7);
    }
}
