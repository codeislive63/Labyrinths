using Labyrinths.Shell;

namespace Labyrinths.CLI;

/// <summary>Интерфейс консольной команды</summary>
public interface IConsoleCommand
{
    /// <summary>Имя команды, используемое в CLI</summary>
    string Name { get; }

    /// <summary>Выполняет команду в указанном контексте</summary>
    Task<int> ExecuteAsync(CommandContext context);
}
