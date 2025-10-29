using Labyrinths.CLI;
using System.Collections.Immutable;

namespace Labyrinths.Shell;

/// <summary>Диспетчер выполнения CLI-команд</summary>
public sealed class CommandDispatcher
{
    private const string HelpCommand = "help";

    private readonly ImmutableDictionary<string, IConsoleCommand> _commands;

    public CommandDispatcher(IEnumerable<IConsoleCommand> commands)
    {
        ArgumentNullException.ThrowIfNull(commands);

        _commands = commands.ToImmutableDictionary(
            c => c.Name,
            c => c,
            StringComparer.OrdinalIgnoreCase
        );
    }

    /// <summary>Находит и выполняет команду по контексту</summary>
    public Task<int> DispatchAsync(CommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (_commands.TryGetValue(context.Command, out var cmd))
        {
            return cmd.ExecuteAsync(context);
        }

        if (_commands.TryGetValue(HelpCommand, out var help))
        {
            return help.ExecuteAsync(new CommandContext(HelpCommand, new Dictionary<string, string>()));
        }

        return Task.FromResult(2);
    }
}
