namespace Labyrinths.CLI.Generate;

/// <summary>
/// Аргументы для команды генерации лабиринта
/// </summary>
public sealed record GenerateCommandArgs(
    string Algorithm,
    int Width,
    int Height,
    bool UseUnicode,
    bool UseFrame,
    string? Output
);
