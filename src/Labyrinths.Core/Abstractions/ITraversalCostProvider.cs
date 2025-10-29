using Labyrinths.Core.Models;

namespace Labyrinths.Core.Abstractions;

/// <summary>
/// Интерфейс провайдера стоимостей перемещения по клеткам лабиринта
/// </summary>
public interface ITraversalCostProvider
{
    /// <summary>Нижняя оценка стоимости шага (для корректной эвристики A*)</summary>
    double MinStepCost { get; }

    /// <summary>Цена входа на клетку (x,y)</summary>
    double GetCost(Maze maze, int x, int y);
}