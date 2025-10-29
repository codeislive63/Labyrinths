using FluentAssertions;
using Labyrinths.Core.Abstractions;
using Labyrinths.Core.Algorithms.Generation;
using Labyrinths.Core.Algorithms.Solving;
using Labyrinths.Core.Models;
using Labyrinths.Core.Traversal;

namespace Labyrinths.Tests.Core;

/// <summary>Общие тесты для генераторов DFS и Prim</summary>
public class MazeGeneratorsTests
{
    public static IEnumerable<object[]> Generators =>
    [
        ["dfs"],
        ["prim"],
    ];

    private static IMazeGenerator Create(string kind) =>
        kind.Equals("dfs", StringComparison.OrdinalIgnoreCase)
            ? new DfsGenerator()
            : new PrimGenerator();

    private static int Count(Maze m, CellType t)
    {
        var c = 0;

        for (int y = 0; y < m.Height; y++)
        {
            for (int x = 0; x < m.Width; x++)
            {
                if (m[x, y] == t)
                {
                    c++;
                }
            }
        }

        return c;
    }

    private static IEnumerable<(int x, int y)> Neigh4(int x, int y)
    {
        yield return (x + 1, y);
        yield return (x - 1, y);
        yield return (x, y + 1);
        yield return (x, y - 1);
    }

    private static int BfsReachableEmpty(Maze m, int sx, int sy)
    {
        if (!m.InBounds(sx, sy) || !m.IsPassable(sx, sy))
        {
            return 0;
        }

        var q = new Queue<(int x, int y)>();
        var vis = new bool[m.Width, m.Height];
        q.Enqueue((sx, sy));
        vis[sx, sy] = true;
        var cnt = 0;

        while (q.Count > 0)
        {
            var (x, y) = q.Dequeue();
            cnt++;
            foreach (var (nx, ny) in Neigh4(x, y))
            {
                if (nx < 0 || ny < 0 || nx >= m.Width || ny >= m.Height)
                {
                    continue;
                }

                if (vis[nx, ny])
                {
                    continue;
                }

                if (!m.IsPassable(nx, ny))
                {
                    continue;
                }

                vis[nx, ny] = true;
                q.Enqueue((nx, ny));
            }
        }

        return cnt;
    }

    private static bool TryFindFirstEmpty(Maze m, out (int x, int y) p)
    {
        for (int y = 0; y < m.Height; y++)
        {
            for (int x = 0; x < m.Width; x++)
            {
                if (m.IsPassable(x, y)) { p = (x, y); return true; }
            }
        }

        p = default;
        return false;
    }

    // ------------------------------------------------------------
    // Тесты
    // ------------------------------------------------------------

    /// <summary>Генерация создаёт лабиринт с запрошенными размерами</summary>
    [Theory]
    [MemberData(nameof(Generators))]
    public void Generates_With_Correct_Dimensions(string kind)
    {
        var g = Create(kind);
        var m1 = g.Generate(5, 7);
        var m2 = g.Generate(10, 10);

        m1.Width.Should().Be(5);
        m1.Height.Should().Be(7);
        m2.Width.Should().Be(10);
        m2.Height.Should().Be(10);
    }

    /// <summary>Лабиринт содержит только стены и пустые клетки и имеет хотя бы один проход</summary>
    [Theory]
    [MemberData(nameof(Generators))]
    public void Contains_Only_Wall_And_Empty(string kind)
    {
        var m = Create(kind).Generate(15, 12);
        int empty = 0, wall = 0;

        for (int y = 0; y < m.Height; y++)
        {
            for (int x = 0; x < m.Width; x++)
            {
                (m[x, y] == CellType.Empty || m[x, y] == CellType.Wall).Should().BeTrue();
                if (m[x, y] == CellType.Empty) { empty++; }
                else
                {
                    wall++;
                }
            }
        }

        empty.Should().BeGreaterThan(0);
        wall.Should().BeGreaterThan(0);
    }

    /// <summary>Доля стен лежит в разумных пределах для случайного лабиринта</summary>
    [Theory]
    [MemberData(nameof(Generators))]
    public void Wall_Ratio_Is_Reasonable(string kind)
    {
        var m = Create(kind).Generate(31, 31);
        var walls = Count(m, CellType.Wall);
        var ratio = (double)walls / (m.Width * m.Height);

        ratio.Should().BeInRange(0.35, 0.75);
    }

    /// <summary>Все коридоры лежат в одной связной компоненте</summary>
    [Theory]
    [MemberData(nameof(Generators))]
    public void Empty_Cells_Form_Single_Component(string kind)
    {
        var m = Create(kind).Generate(21, 21);
        TryFindFirstEmpty(m, out var s).Should().BeTrue();

        var reachable = BfsReachableEmpty(m, s.x, s.y);
        var totalEmpty = Count(m, CellType.Empty);

        reachable.Should().Be(totalEmpty);
    }

    /// <summary>Из угла в угол существует путь для A* если генератор обеспечивает входы по углам</summary>
    [Theory]
    [MemberData(nameof(Generators))]
    public void Solvable_From_00_To_End_With_AStar(string kind)
    {
        var m = Create(kind).Generate(25, 25);

        var solver = new AStarSolver(new TraversalCostAccessor
        {
            Current = DefaultCostProvider.Instance
        });

        var start = new Point(0, 0);
        var end = new Point(m.Width - 1, m.Height - 1);

        if (!m.IsPassable(start.X, start.Y))
        {
            var first = Enumerable.Range(0, m.Height)
                .SelectMany(y => Enumerable.Range(0, m.Width)
                    .Select(x => new { x, y }))
                .First(p => m.IsPassable(p.x, p.y));

            start = new Point(first.x, first.y);
        }

        if (!m.IsPassable(end.X, end.Y))
        {
            var last = Enumerable.Range(0, m.Height).Reverse()
                .SelectMany(y => Enumerable.Range(0, m.Width).Reverse()
                    .Select(x => new { x, y }))
                .First(p => m.IsPassable(p.x, p.y));

            end = new Point(last.x, last.y);
        }

        var path = solver.Solve(m, start, end);

        path.Should().NotBeNull();
        path.IsFound.Should().BeTrue();
        path.Steps[0].Should().Be(start);
        path.Steps[path.Steps.Count - 1].Should().Be(end);
    }

    /// <summary>Генератор корректно работает на маленьких размерах 1x1 и 2x2</summary>
    [Theory]
    [MemberData(nameof(Generators))]
    public void Works_On_Tiny_Sizes(string kind)
    {
        var g = Create(kind);

        var m1 = g.Generate(1, 1);
        m1.Width.Should().Be(1);
        m1.Height.Should().Be(1);

        var m2 = g.Generate(2, 2);
        m2.Width.Should().Be(2);
        m2.Height.Should().Be(2);

        if (m1.IsPassable(0, 0))
        {
            var solver = new AStarSolver(new TraversalCostAccessor { Current = DefaultCostProvider.Instance });
            var p = solver.Solve(m1, new Point(0, 0), new Point(0, 0))!;
            p.IsFound.Should().BeTrue();
        }
    }

    /// <summary>Повторные генерации с одинаковым размером дают разные раскладки в большинстве случаев</summary>
    [Theory]
    [MemberData(nameof(Generators))]
    public void Produces_Different_Layouts_On_Subsequent_Runs(string kind)
    {
        var g = Create(kind);
        var a = g.Generate(31, 31);
        var b = g.Generate(31, 31);

        bool equal = true;
        for (int y = 0; y < a.Height && equal; y++)
        {
            for (int x = 0; x < a.Width && equal; x++)
            {
                if (a[x, y] != b[x, y])
                {
                    equal = false;
                }
            }
        }

        equal.Should().BeFalse();
    }
}
