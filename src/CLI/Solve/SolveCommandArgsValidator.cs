using Labyrinths.Core.Models;

namespace Labyrinths.CLI.Solve; 

public static class SolveCommandArgsValidator
{
    /// <summary>
    /// Проверяет, что старт и финиш находятся в пределах лабиринта и проходимы.
    /// </summary>
    public static IEnumerable<string> Validate(SolveCommandArgs opts, Maze maze)
    {
        if (!maze.InBounds(opts.Start.X, opts.Start.Y) || !maze.IsPassable(opts.Start.X, opts.Start.Y))
        {
            yield return $"Старт вне поля или непроходим: {opts.Start.X},{opts.Start.Y}";
        }

        if (!maze.InBounds(opts.End.X, opts.End.Y) || !maze.IsPassable(opts.End.X, opts.End.Y))
        {
            yield return $"Финиш вне поля или непроходим: {opts.End.X},{opts.End.Y}";
        }
    }
}
