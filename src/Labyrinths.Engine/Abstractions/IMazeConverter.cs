namespace Labyrinths.Engine.Abstractions;

/// <summary>Интерфейс преобразования модели лабиринта в заданный формат и обратно</summary>
/// <typeparam name="TFormat">Тип формата представления</typeparam>
/// <typeparam name="TModel">Тип модели лабиринта</typeparam>
public interface IMazeConverter<TFormat, TModel>
{
    /// <summary>Преобразует модель в формат</summary>
    TFormat FromModelToFormat(TModel model);

    /// <summary>Преобразует формат обратно в модель</summary>
    TModel FromFormatToModel(TFormat data);
}
