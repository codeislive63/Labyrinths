using Labyrinths.Core.Abstractions;
using Labyrinths.Core.Models;

namespace Labyrinths.Core.Traversal;

/// <summary>Провайдер с фиксированной стоимостью шагов (по умолчанию)</summary>
public sealed class DefaultCostProvider : ITraversalCostProvider
{
    /// <summary>Единственный экземпляр по умолчанию</summary>
    public static readonly DefaultCostProvider Instance = new();

    /// <summary>Возвращает фиксированную стоимость шага (1.0)</summary>
    public double GetCost(Maze _, int __, int ___) => 1.0;

    /// <summary>Минимальная возможная стоимость шага</summary>
    public double MinStepCost => 1.0;
}
