using Labyrinths.Core.Abstractions;
using Labyrinths.Core.Traversal;
using Labyrinths.Engine.Abstractions;
using Labyrinths.Engine.Converters;
using Labyrinths.Engine.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace Labyrinths.Engine.Extensions;

/// <summary>Методы расширения для регистрации компонентов движка Labyrinths</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Добавляет в контейнер сериализатор и рендереры лабиринтов</summary>
    public static IServiceCollection AddLabyrinthsEngine(this IServiceCollection services)
    {
        services.AddSingleton<IMazeConverter<string, Core.Models.Maze>, AsciiConverter>();

        services.AddSingleton<AsciiRenderer>();
        services.AddSingleton<UnicodeRenderer>();

        services.AddSingleton<Func<bool /*unicode*/, bool /*frame*/, IMazeRenderer>>(sp => (unicode, frame) =>
        {
            IMazeRenderer baseRenderer = unicode
                ? sp.GetRequiredService<UnicodeRenderer>()
                : sp.GetRequiredService<AsciiRenderer>();

            return frame
                ? new FrameRendererDecorator(baseRenderer, unicode ? '█' : '#')
                : baseRenderer;
        });

        services.AddSingleton<ITraversalCostAccessor, TraversalCostAccessor>();

        return services;
    }
}
