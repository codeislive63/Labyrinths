namespace Labyrinths.Core.Models;

/// <summary>Карта покрытий для клеток лабиринта</summary>
public sealed class TerrainMap(int width, int height)
{
    public TerrainType[,] Cells { get; } = new TerrainType[width, height];

    public int Width { get; } = width;

    public int Height { get; } = height;

    public TerrainType this[int x, int y]
    {
        get => Cells[x, y];
        set => Cells[x, y] = value;
    }
}
