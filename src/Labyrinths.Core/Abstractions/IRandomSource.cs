namespace Labyrinths.Core.Abstractions;

/// <summary>Источник случайности для детерминируемых генераторов</summary>
public interface IRandomSource
{
    /// <summary>Возвращает случайное число в диапазоне [minInclusive, maxExclusive)</summary>
    int Next(int minInclusive, int maxExclusive);

    /// <summary>Перемешивает элементы списка случайным образом</summary>
    void Shuffle<T>(IList<T> list);
}