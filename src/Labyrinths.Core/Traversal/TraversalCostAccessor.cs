using Labyrinths.Core.Abstractions;

namespace Labyrinths.Core.Traversal;

/// <summary>Хранилище текущего провайдера стоимости перемещения</summary>
public sealed class TraversalCostAccessor : ITraversalCostAccessor
{
    /// <summary>Текущий активный провайдер стоимости</summary>
    public ITraversalCostProvider Current { get; set; } = DefaultCostProvider.Instance;
}
