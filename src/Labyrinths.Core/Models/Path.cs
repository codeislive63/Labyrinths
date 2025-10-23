namespace Labyrinths.Core.Models;

/// <summary>Результат поиска пути</summary>
public sealed class Path(IEnumerable<Point> steps)
{
    public IReadOnlyList<Point> Steps { get; } = steps.ToList().AsReadOnly();

    public bool IsFound => Steps.Count > 0;

    public int Length => Steps.Count > 0 ? Steps.Count - 1 : 0;
}
