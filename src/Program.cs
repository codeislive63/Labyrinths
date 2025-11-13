using Labyrinths.CLI;
using Labyrinths.CLI.Generate;
using Labyrinths.CLI.Help;
using Labyrinths.CLI.Solve;
using Labyrinths.Core.Abstractions;
using Labyrinths.Core.Algorithms.Generation;
using Labyrinths.Core.Algorithms.Solving;
using Labyrinths.Engine.Extensions;
using Labyrinths.Infrastructure.Providers;
using Labyrinths.Services;
using Labyrinths.Shell;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.OutputEncoding = System.Text.Encoding.UTF8;

using var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        // Engine
        services.AddLabyrinthsEngine();

        // Core: генераторы и солверы
        services.AddSingleton<IMazeGenerator, DfsGenerator>();
        services.AddSingleton<IMazeGenerator, PrimGenerator>();
        services.AddSingleton<IPathSolver, AStarSolver>();
        services.AddSingleton<IPathSolver, DijkstraSolver>();

        // Shell/CLI
        services.AddSingleton<CommandDispatcher>();
        services.AddSingleton<IConsoleCommand, HelpCommand>();
        services.AddSingleton<IConsoleCommand, GenerateCommand>();
        services.AddSingleton<IConsoleCommand, SolveCommand>();

        // сервисы
        services.AddSingleton<MazeGenerationService>();
        services.AddSingleton<MazeSolvingService>();

        // Providers
        services.AddSingleton<ITerrainProvider, JsonTerrainProvider>();
    }).Build();

var context = CommandParser.Parse(args);
var dispatcher = host.Services.GetRequiredService<CommandDispatcher>();

int exitCode;

try
{
    exitCode = await dispatcher.DispatchAsync(context);
}
catch (OperationCanceledException)
{
    exitCode = 130;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Неожиданная ошибка: {ex.Message}");
    exitCode = 1;
}

return exitCode;
