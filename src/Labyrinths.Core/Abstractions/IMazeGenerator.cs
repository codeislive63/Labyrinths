using Labyrinths.Core.Models;

namespace Labyrinths.Core.Abstractions;

/// <summary>Интерфейс для генерации лабиринтов</summary>
public interface IMazeGenerator
{
    /// <summary>Сгенерировать лабиринт указанного размера. width/height должны быть > 0, иначе будет брошено исключение</summary>
    Maze Generate(int width, int height, int? seed = null);
}