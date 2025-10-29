using FluentAssertions;
using Labyrinths.Core.Models;
using Labyrinths.Core.Traversal;

namespace Labyrinths.Tests.Core;

/// <summary>Тесты для TerrainCostProvider</summary>
public class TerrainCostProviderTests
{
    private static Maze Maze(int w, int h)
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

    /// <summary>Возвращает ожидаемую стоимость для каждого типа покрытия</summary>
    [Fact]
    public void ReturnsExpectedCosts_ForEachTerrainType()
    {
        var map = new TerrainMap(5, 1);
        map[0, 0] = TerrainType.Normal;
        map[1, 0] = TerrainType.Sand;
        map[2, 0] = TerrainType.Mud;
        map[3, 0] = TerrainType.Road;
        map[4, 0] = TerrainType.Bonus;

        var p = new TerrainCostProvider(map);
        var m = Maze(5, 1);

        p.GetCost(m, 0, 0).Should().Be(1.0);
        p.GetCost(m, 1, 0).Should().Be(2.0);
        p.GetCost(m, 2, 0).Should().Be(3.0);
        p.GetCost(m, 3, 0).Should().Be(0.6);
        p.GetCost(m, 4, 0).Should().Be(0.3);
    }

    /// <summary>Для координат вне лабиринта возвращает бесконечность</summary>
    [Fact]
    public void ReturnsInfinity_For_OutOfBounds()
    {
        var map = new TerrainMap(2, 2);
        var p = new TerrainCostProvider(map);
        var m = Maze(2, 2);

        p.GetCost(m, -1, 0).Should().Be(double.PositiveInfinity);
        p.GetCost(m, 0, -1).Should().Be(double.PositiveInfinity);
        p.GetCost(m, 2, 1).Should().Be(double.PositiveInfinity);
        p.GetCost(m, 1, 2).Should().Be(double.PositiveInfinity);
    }

    /// <summary>Если клетка вне TerrainMap но внутри Maze — используется Normal</summary>
    [Fact]
    public void Returns_Normal_For_Cells_Outside_TerrainMap()
    {
        var map = new TerrainMap(1, 1);
        var p = new TerrainCostProvider(map);
        var m = Maze(3, 3);

        p.GetCost(m, 2, 2).Should().Be(1.0);
    }

    /// <summary>MinStepCost равен минимальной стоимости покрытия</summary>
    [Fact]
    public void MinStepCost_Is_Minimum_Of_All()
    {
        var map = new TerrainMap(3, 1);
        map[0, 0] = TerrainType.Sand;
        map[1, 0] = TerrainType.Bonus;
        map[2, 0] = TerrainType.Normal;

        var p = new TerrainCostProvider(map);
        p.MinStepCost.Should().Be(0.3);
    }

    /// <summary>Провайдер корректно работает если размеры карты меньше лабиринта</summary>
    [Fact]
    public void Handles_Mismatched_Sizes()
    {
        var map = new TerrainMap(2, 2);
        var p = new TerrainCostProvider(map);
        var m = Maze(5, 5);

        var cost = p.GetCost(m, 4, 4);
        cost.Should().Be(1.0);
    }

    /// <summary>Не падает при обращении к неизвестным типам покрытия</summary>
    [Fact]
    public void Safe_For_Uninitialized_Cells()
    {
        var map = new TerrainMap(2, 2);
        var p = new TerrainCostProvider(map);
        var m = Maze(2, 2);

        // Все ячейки TerrainType.Normal по умолчанию
        p.GetCost(m, 1, 1).Should().Be(1.0);
    }
}
