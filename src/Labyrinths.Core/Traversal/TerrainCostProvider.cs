using Labyrinths.Core.Abstractions;
using Labyrinths.Core.Models;

namespace Labyrinths.Core.Traversal;

/// <summary>Провайдер стоимости перемещения по карте покрытий</summary>
public sealed class TerrainCostProvider : ITraversalCostProvider
{
    private readonly TerrainMap _map;
    private readonly Dictionary<TerrainType, double> _costs;

    /// <summary>Создаёт провайдер на основе карты покрытий</summary>
    public TerrainCostProvider(TerrainMap map)
    {
        _map = map;

        _costs = new()
        {
            [TerrainType.Normal] = 1.0,
            [TerrainType.Sand] = 2.0,
            [TerrainType.Mud] = 3.0,
            [TerrainType.Road] = 0.6,
            [TerrainType.Bonus] = 0.3
        };

        MinStepCost = _costs.Values.Min();
    }

    /// <summary>Минимальная стоимость шага (нужна A* для эвристики)</summary>
    public double MinStepCost { get; }

    /// <summary>Карта покрытий, чтобы визуализатор мог её отрисовать</summary>
    public TerrainMap Map => _map;

    /// <inheritdoc/>
    public double GetCost(Maze maze, int x, int y)
    {
        if (!maze.InBounds(x, y))
        {
            return double.PositiveInfinity;
        }

        if (x < 0 || y < 0 || x >= _map.Width || y >= _map.Height)
        {
            return _costs[TerrainType.Normal];
        }

        var type = _map[x, y];
        return _costs.TryGetValue(type, out var c) ? c : _costs[TerrainType.Normal];
    }
}
