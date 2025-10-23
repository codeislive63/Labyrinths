using Labyrinths.Core.Abstractions;

namespace Labyrinths.Core.Randomization;

/// <summary>Реализация источника случайности на основе System.Random</summary>
public sealed class RandomSource(int? seed = null) : IRandomSource
{
    private readonly Random _rnd = seed.HasValue ? new Random(seed.Value) : new Random();

    /// <inheritdoc/>
    public int Next(int minInclusive, int maxExclusive) => _rnd.Next(minInclusive, maxExclusive);

    /// <inheritdoc/>
    public void Shuffle<T>(IList<T> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = _rnd.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}