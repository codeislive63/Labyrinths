using Labyrinths.Core.Models;
using Path = Labyrinths.Core.Models.Path;

namespace Labyrinths.Core.Abstractions;

/// <summary>Интерфейс для поиска пути в лабиринте</summary>
public interface IPathSolver
{
    /// <summary>Найти путь от start к end в заданном лабиринте. Возвращает null, если его нет.</summary>
    Path? Solve(Maze maze, Point start, Point end);
}