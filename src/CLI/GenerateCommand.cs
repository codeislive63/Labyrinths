using Labyrinths.Services;
using Labyrinths.Shell;

namespace Labyrinths.CLI;

/// <summary>Команда генерации лабиринта</summary>
public sealed class GenerateCommand(MazeGenerationService service) : IConsoleCommand
{
    private readonly MazeGenerationService _service = service;

    /// <inheritdoc/>
    public string Name => "generate";

    /// <inheritdoc/>
    public Task<int> ExecuteAsync(CommandContext context)
    {
        return _service.RunAsync(context);
    }
}
