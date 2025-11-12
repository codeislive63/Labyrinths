namespace Labyrinths.Core.Models;

/// <summary>Карта покрытий для клеток лабиринта</summary>
public sealed class TerrainMap
{
    private readonly TerrainType[,] _cells;

    public int Width { get; }

    public int Height { get; }

    public TerrainMap(int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width должно быть > 0");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height должно быть > 0");
        }

        Width = width;
        Height = height;
        _cells = new TerrainType[height, width];
    }

    public TerrainType this[int x, int y]
    {
        get => _cells[y, x];
        set => _cells[y, x] = value;
    }
}
