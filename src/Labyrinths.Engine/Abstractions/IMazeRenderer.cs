using Labyrinths.Core.Models;
using Path = Labyrinths.Core.Models.Path;
using Point = Labyrinths.Core.Models.Point;

namespace Labyrinths.Engine.Abstractions;

/// <summary>Рендер лабиринта в строковое представление</summary>
public interface IMazeRenderer
{
    /// <summary>Отрисовать лабиринт с опциональными стартом, финишем и путём</summary>
    string Render(Maze maze, Point? start = null, Point? end = null, Path? path = null);
}
