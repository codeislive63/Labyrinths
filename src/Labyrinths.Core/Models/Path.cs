namespace Labyrinths.Core.Models;

/// <summary>Результат поиска пути</summary>
public sealed class Path(IEnumerable<Point> steps)
{
    /// <summary>Последовательность шагов от начальной до конечной точки</summary>
    public IReadOnlyList<Point> Steps { get; } = steps.ToList().AsReadOnly();

    /// <summary>Показывает, найден ли путь</summary>
    public bool IsFound => Steps.Count > 0;
}
