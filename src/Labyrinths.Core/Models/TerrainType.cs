namespace Labyrinths.Core.Models;

public enum TerrainType
{
    Normal,   // базовая стоимость
    Sand,     // песок — дороже
    Mud,      // болото — ещё дороже
    Road,     // хорошее покрытие — дешевле
    Bonus     // «монетки» — слегка удешевляют клетку
}
