namespace Labyrinths.Core.Models;

/// <summary>Лабиринт</summary>
public sealed class Maze
{
    private readonly CellType[,] _cells;

    public int Width { get; }

    public int Height { get; }

    public Maze(int width, int height, CellType fill = CellType.Wall)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Размеры лабиринта должны быть положительными");
        }

        Width = width;
        Height = height;
        _cells = new CellType[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                _cells[y, x] = fill;
            }
        }
    }

    /// <summary>Получить/установить тип ячейки</summary>
    public CellType this[int x, int y]
    {
        get => _cells[y, x];
        set => _cells[y, x] = value;
    }

    /// <summary>Проверка на нахождение координат внутри поля</summary>
    public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

    /// <summary>Проверка на проходимость ячейки</summary>
    public bool IsPassable(int x, int y) => InBounds(x, y) && _cells[y, x] == CellType.Empty;

    /// <summary>Устанавливает для всех ячеек сетки заданный тип <paramref name="type"/></summary>
    public void Fill(CellType type)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                _cells[y, x] = type;
            }
        }
    }
}
