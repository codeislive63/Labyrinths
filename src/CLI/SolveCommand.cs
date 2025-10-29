using Labyrinths.Services;
using Labyrinths.Shell;

namespace Labyrinths.CLI;

/// <summary>Команда решения лабиринта</summary>
public sealed class SolveCommand(MazeSolvingService service) : IConsoleCommand
{
    private readonly MazeSolvingService _service = service;

    /// <inheritdoc/>
    public string Name => "solve";

    /// <inheritdoc/>
    public Task<int> ExecuteAsync(CommandContext context)
    {
        return _service.RunAsync(context);
    }
}
