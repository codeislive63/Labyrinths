using FluentAssertions;
using Labyrinths.Core.Models;
using Labyrinths.Engine.Rendering;
using Path = Labyrinths.Core.Models.Path;

namespace Labyrinths.Tests.Engine;

/// <summary>Тесты визуализации покрытия местности</summary>
public class TerrainVisualizerTests
{
    private static Maze Open5x3()
    {
        var m = new Maze(5, 3);

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                m[x, y] = CellType.Empty;
            }
        }

        return m;
    }

    /// <summary>ASCII-режим: рисуются символы покрытия, путь, рамка и легенда</summary>
    [Fact]
    public void TerrainVisualizer_Should_Draw_Terrain_Symbols_Path_And_Legend_Ascii()
    {
        var maze = Open5x3();
        var map = new TerrainMap(5, 3);
        map[1, 0] = TerrainType.Sand;
        map[2, 0] = TerrainType.Mud;
        map[3, 0] = TerrainType.Road;
        map[4, 0] = TerrainType.Bonus;

        var path = new Path([new(0, 0), new(1, 0), new(2, 0), new(3, 0), new(4, 0)]);

        var s = TerrainVisualizer.Render(
            maze, map,
            start: new Point(0, 0),
            end: new Point(4, 0),
            path: path,
            unicode: false,
            frame: true,
            legend: true);

        s.Should().Contain("#####");
        s.ToLowerInvariant().Should().Contain("обозначения");

        s.Should().Contain("~");
        s.Should().Contain("≈");
        s.Should().Contain("=");
        s.Should().Contain("$");

        s.Should().Contain("O");
        s.Should().Contain("X");
        s.Should().Contain(".");
    }

    /// <summary>Unicode-режим: вывод содержит хотя бы один специализированный символ.</summary>
    [Fact]
    public void TerrainVisualizer_Should_Respect_Unicode_Mode()
    {
        var maze = Open5x3();
        var map = new TerrainMap(5, 3);
        map[2, 1] = TerrainType.Mud;

        var s = TerrainVisualizer.Render(maze, map, null, null, null, unicode: true, frame: false, legend: false);

        s.Should().MatchRegex("[█▒─•]");
    }
}
