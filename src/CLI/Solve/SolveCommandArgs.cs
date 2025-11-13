using Labyrinths.Core.Models;

namespace Labyrinths.CLI.Solve;

/// <summary>
/// Аргументы команды <c>solve</c>
/// </summary>
public sealed record SolveCommandArgs(
    string Algorithm,
    string File,
    Point Start,
    Point End,
    bool UseUnicode,
    bool UseFrame,
    bool ShowTerrain,
    string? Output,
    string? CostsPath
);
