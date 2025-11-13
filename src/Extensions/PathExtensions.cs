namespace Labyrinths.Extensions;

/// <summary>
/// Вспомогательные расширения для путей файловой системы
/// </summary>
internal static class PathExtensions
{
    /// <summary>Гарантирует наличие родительской директории для файла</summary>
    internal static void EnsureParentDirectory(this string path)
        => new FileInfo(path).Directory?.Create();
}
