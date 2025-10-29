using Labyrinths.Core.Models;

namespace Labyrinths.Engine.Rendering;

/// <summary>Набор символов для визуализации покрытий</summary>
public static class TerrainSymbols
{
    public static (char wall, char empty, char path, char start, char end) BaseAscii()
        => ('#', ' ', '.', 'O', 'X');

    public static (char wall, char empty, char path, char start, char end) BaseUnicode()
        => ('█', ' ', '·', 'O', 'X');

    public static char GlyphAscii(TerrainType t) => t switch
    {
        TerrainType.Normal => ' ',
        TerrainType.Sand => '~', // песок
        TerrainType.Mud => '≈', // болото
        TerrainType.Road => '=', // дорога
        TerrainType.Bonus => '$', // бонус/монетки
        _ => ' '
    };

    public static char GlyphUnicode(TerrainType t) => t switch
    {
        TerrainType.Normal => ' ',
        TerrainType.Sand => '░',
        TerrainType.Mud => '▒',
        TerrainType.Road => '─',
        TerrainType.Bonus => '•',
        _ => ' '
    };

    public static string LegendAscii() =>
        "Обозначения:  ~ Песок  ≈ Болото  = Дорога  $ Монетка  . Путь  O Начало  X Конец";

    public static string LegendUnicode() =>
        "Обозначения: ░ Песок  ▒ Болото  ─ Дорога  • Монетка  · Путь  O Начало  X Конец";
}
