namespace Labyrinths.Shell;

/// <summary>Контекст, содержащий распознанную команду и её параметры</summary>
public sealed class CommandContext(string command, IReadOnlyDictionary<string, string> options)
{
    /// <summary>Имя распознанной команды</summary>
    public string Command { get; } = command;

    /// <summary>Параметры, переданные команде</summary>
    public IReadOnlyDictionary<string, string> Options { get; } = options;
}
