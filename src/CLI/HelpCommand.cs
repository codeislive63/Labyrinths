using Labyrinths.Shell;

namespace Labyrinths.CLI;

/// <summary>Команда отображения справки по CLI-утилите</summary>
public sealed class HelpCommand : IConsoleCommand
{
    /// <inheritdoc/>
    public string Name => "help";

    /// <inheritdoc/>
    public Task<int> ExecuteAsync(CommandContext context)
    {
        Console.WriteLine(
@"Использование: Labyrinths [команда] [параметры]

Доступные команды:
    generate        Сгенерировать лабиринт
    solve           Найти путь в лабиринте
    help            Показать справку

Параметры команды generate:
    --algorithm     dfs | prim
    --width         ширина лабиринта (>=1)
    --height        высота лабиринта (>=1)
    --output        необязательный файл для сохранения лабиринта
    --unicode       использовать псевдографику Unicode при выводе
    --frame         предпросмотр рендеринга с рамкой границы

Параметры команды solve:
    --algorithm     astar | dijkstra
    --file          путь к файлу лабиринта
    --start         начальная точка в формате x,y
    --end           конечная точка в формате x,y
    --output        необязательный файл для сохранения решения
    --unicode       использовать псевдографику Unicode при выводе
    --frame         предпросмотр рендеринга с рамкой границы
    --costs=<json>  карта покрытий (x,y,cost) для учёта в стоимости шага
    --showterrain   визуализирует карту местности

Примеры:
    Labyrinths generate --algorithm=dfs --width=10 --height=10 --output=maze1.txt
    Labyrinths generate --algorithm=prim --width=10 --height=10 --output=maze2.txt
    Labyrinths solve --algorithm=astar --file=maze1.txt --start=0,0 --end=9,9 --output=solution_maze1.txt
    Labyrinths solve --algorithm=dijkstra --file=maze2.txt --start=0,0 --end=9,9 --output=solution_maze2.txt"
        );

        return Task.FromResult(0);
    }
}
