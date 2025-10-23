namespace Labyrinths.Core.Models;

/// <summary>Точка на сетке лабиринта.</summary>
public readonly record struct Point(int X, int Y)
{
    /// <summary>Соседи по четырём направлениям.</summary>
    public IEnumerable<Point> Neighbors4()
    {
        yield return new Point(X, Y - 1);
        yield return new Point(X, Y + 1);
        yield return new Point(X - 1, Y);
        yield return new Point(X + 1, Y);
    }

    /// <summary>Сосед на 2 клетки по направлению.</summary>
    public Point Jump2(int dx, int dy) => new(X + dx * 2, Y + dy * 2);
}