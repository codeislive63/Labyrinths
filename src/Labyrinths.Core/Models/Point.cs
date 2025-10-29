namespace Labyrinths.Core.Models;

/// <summary>Точка на сетке лабиринта</summary>
public readonly record struct Point(int X, int Y)
{
    /// <summary>Сосед на 2 клетки по направлению</summary>
    public Point Jump2(int dx, int dy) => new(X + dx * 2, Y + dy * 2);
}