namespace Labyrinths.Core.Abstractions;

/// <summary>
/// Позволяет алгоритмам и визуализаторам получать или менять активный <see cref="ITraversalCostProvider"/>.
/// </summary>
public interface ITraversalCostAccessor
{
    /// <summary>Текущий источник стоимостей перемещения по лабиринту</summary>
    ITraversalCostProvider Current { get; set; }
}