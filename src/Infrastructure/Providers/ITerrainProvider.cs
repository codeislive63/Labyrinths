using Labyrinths.Core.Abstractions;

namespace Labyrinths.Infrastructure.Providers;

/// <summary>
/// Интерфейс для загрузки карты покрытий лабиринта из внешнего источника
/// </summary>
public interface ITerrainProvider
{
    /// <summary>
    /// Асинхронно загружает карту покрытий и возвращает провайдер стоимости перемещения
    /// </summary>
    Task<ITraversalCostProvider> LoadAsync(
        string path, 
        int mazeWidth, 
        int mazeHeight, 
        CancellationToken ct = default
    );
}
