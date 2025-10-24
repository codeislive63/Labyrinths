using Labyrinths.Core.Models;
using Labyrinths.Engine.Abstractions;
using Labyrinths.Engine.Rendering;
using Labyrinths.Engine.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Labyrinths.Engine.Extensions;

/// <summary>Методы расширения для регистрации компонентов движка Labyrinths</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Добавляет в контейнер сериализатор и рендереры лабиринтов</summary>
    public static IServiceCollection AddLabyrinthsEngine(this IServiceCollection services)
    {
        services.AddSingleton<IMazeConverter<string, Maze>, AsciiMazeConverter>();

        services.AddSingleton<IMazeRenderer, AsciiMazeRenderer>();
        services.AddSingleton<UnicodeRenderer>();

        return services;
    }
}
