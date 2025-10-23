using Labyrinths.Core.Models;

namespace Labyrinths.Core.Abstractions;

/// <summary>Интерфейс для генерации лабиринтов</summary>
public interface IMazeGenerator
{
    /// <summary>Сгенерировать лабиринт заданного размера</summary>
    Maze Generate(int width, int height, int? seed = null);
}