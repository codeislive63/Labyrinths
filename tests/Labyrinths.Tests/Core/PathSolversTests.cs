using FluentAssertions;
using Labyrinths.Core.Abstractions;
using Labyrinths.Core.Models;
using Labyrinths.Core.Traversal;
using Labyrinths.Core.Algorithms.Solving;
using Path = Labyrinths.Core.Models.Path;


namespace Labyrinths.Tests.Core;

/// <summary>Общие тесты для A* и Дейкстры</summary>
public class PathSolversTests
{
    public static IEnumerable<object[]> Algorithms =>
    [
        ["astar"],
        ["dijkstra"]
    ];

    /// <summary>Создать солвер по имени алгоритма</summary>
    private static IPathSolver CreateSolver(string algo, ITraversalCostProvider provider)
    {
        var accessor = new TraversalCostAccessor { Current = provider };
        return algo.Equals("astar", StringComparison.OrdinalIgnoreCase)
            ? new AStarSolver(accessor)
            : new DijkstraSolver(accessor);
    }

    private static Maze EmptyMaze(int w, int h)
    {
        var m = new Maze(w, h);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                m[x, y] = CellType.Empty;
            }
        }

        return m;
    }

    private static double PathCost(Path p, TerrainCostProvider cost, Maze maze)
    {
        double sum = 0.0;

        for (int i = 1; i < p.Steps.Count; i++)
        {
            var s = p.Steps[i];
            sum += cost.GetCost(maze, s.X, s.Y);
        }

        return sum;
    }

    private static int ExpectedStepsCount(Point a, Point b) =>
        Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + 1;

    // ------------------------------------------------------------
    // Тесты
    // ------------------------------------------------------------

    /// <summary>Должен находить корректный путь в простом лабиринте</summary>
    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Should_Find_Path_In_Simple_Maze(string algo)
    {
        var maze = EmptyMaze(5, 5);
        var start = new Point(0, 0);
        var end = new Point(4, 4);

        var solver = CreateSolver(algo, DefaultCostProvider.Instance);
        var result = solver.Solve(maze, start, end);

        result.Should().NotBeNull();
        result.IsFound.Should().BeTrue();
        result.Steps.Should().Contain(end);
        result.Steps.Count.Should().Be(ExpectedStepsCount(start, end));
    }

    /// <summary>Должен возвращать null если путь невозможен</summary>
    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Should_Return_Null_When_No_Path(string algo)
    {
        var maze = EmptyMaze(3, 3);
        maze[1, 0] = maze[1, 1] = maze[1, 2] = CellType.Wall;

        var solver = CreateSolver(algo, DefaultCostProvider.Instance);
        var result = solver.Solve(maze, new Point(0, 0), new Point(2, 2));

        result.Should().BeNull();
    }

    /// <summary>Если старт равен финишу — путь из одной клетки</summary>
    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Start_Equals_End_Should_Return_Single_Step_Path(string algo)
    {
        var maze = EmptyMaze(4, 4);
        var s = new Point(2, 2);

        var solver = CreateSolver(algo, DefaultCostProvider.Instance);
        var result = solver.Solve(maze, s, s);

        result.Should().NotBeNull();
        result!.IsFound.Should().BeTrue();
        result.Steps.Should().Equal([s]);
    }

    /// <summary>Финиш на стене делает путь невозможным</summary>
    [Theory]
    [MemberData(nameof(Algorithms))]
    public void End_On_Wall_Should_Return_Null(string algo)
    {
        var maze = EmptyMaze(5, 5);
        maze[4, 4] = CellType.Wall;

        var solver = CreateSolver(algo, DefaultCostProvider.Instance);
        var result = solver.Solve(maze, new Point(0, 0), new Point(4, 4));

        result.Should().BeNull();
    }

    /// <summary>Алгоритм обходит болото даже если это длиннее но дешевле</summary>
    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Should_Prefer_Road_Over_Mud_Even_If_Longer(string algo)
    {
        var maze = EmptyMaze(4, 2);

        var map = new TerrainMap(4, 2);
        map[1, 0] = map[2, 0] = TerrainType.Mud;
        map[1, 1] = map[2, 1] = TerrainType.Road;
        var cost = new TerrainCostProvider(map);

        var solver = CreateSolver(algo, cost);

        var start = new Point(0, 0);
        var end = new Point(3, 0);
        var path = solver.Solve(maze, start, end)!;

        path.IsFound.Should().BeTrue();
        path.Steps.Should().Contain(new Point(1, 1));
        PathCost(path, cost, maze).Should().BeLessThan(3.0 + 3.0 + 1.0);
    }

    /// <summary>Алгоритм выбирает бонусные клетки когда это возможно</summary>
    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Should_Prefer_Bonus_Path_When_Available(string algo)
    {
        var maze = EmptyMaze(5, 3);
        var map = new TerrainMap(5, 3);
        map[1, 1] = map[2, 1] = map[3, 1] = TerrainType.Bonus;
        var cost = new TerrainCostProvider(map);

        var solver = CreateSolver(algo, cost);

        var start = new Point(0, 1);
        var end = new Point(4, 1);
        var path = solver.Solve(maze, start, end)!;

        path.IsFound.Should().BeTrue();
        path.Steps.Should().Contain(new Point(2, 1));
        PathCost(path, cost, maze).Should().BeApproximately(0.3 * 3 + 1, 1e-6);
    }

    /// <summary>Алгоритм уважает стены по периметру и использует открытый проход</summary>
    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Should_Respect_Borders_And_Use_Corridor(string algo)
    {
        var maze = EmptyMaze(5, 5);

        for (int x = 0; x < 5; x++)
        {
            maze[x, 0] = CellType.Wall;
            maze[x, 4] = CellType.Wall;
        }

        for (int y = 0; y < 5; y++)
        {
            maze[0, y] = CellType.Wall;
            maze[4, y] = CellType.Wall;
        }

        maze[0, 2] = CellType.Empty;

        var start = new Point(0, 2);
        var end = new Point(3, 2);

        var solver = CreateSolver(algo, DefaultCostProvider.Instance);
        var path = solver.Solve(maze, start, end)!;

        path.IsFound.Should().BeTrue();
        path.Steps[0].Should().Be(start);
        path.Steps[path.Steps.Count - 1].Should().Be(end);
    }

    /// <summary>Алгоритм корректно находит проход в коридоре с препятствиями</summary>
    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Should_Detour_Around_Walls_Corridor(string algo)
    {
        var maze = EmptyMaze(4, 3);
        maze[2, 0] = maze[3, 0] = CellType.Wall;
        maze[0, 2] = maze[1, 2] = CellType.Wall;

        var start = new Point(0, 0);
        var end = new Point(3, 2);

        var solver = CreateSolver(algo, DefaultCostProvider.Instance);
        var path = solver.Solve(maze, start, end)!;

        path.IsFound.Should().BeTrue();
        path.Steps.Should().Contain(new Point(2, 1));
    }

    /// <summary>Выход за границы карты покрытий не ломает расчёт стоимости</summary>
    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Terrain_Map_OutOfBounds_Cells_Must_Not_Break_Costing(string algo)
    {
        var maze = EmptyMaze(6, 6);
        var small = new TerrainMap(3, 3);
        small[2, 2] = TerrainType.Mud;
        var provider = new TerrainCostProvider(small);

        var solver = CreateSolver(algo, provider);
        var path = solver.Solve(maze, new Point(0, 0), new Point(5, 5))!;

        path.IsFound.Should().BeTrue();
    }

    /// <summary>На большой пустой карте алгоритм работает быстро и без отклонений</summary>
    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Large_Empty_Maze_Is_Fast_And_Straight(string algo)
    {
        var maze = EmptyMaze(50, 50);
        var start = new Point(0, 0);
        var end = new Point(49, 49);

        var solver = CreateSolver(algo, DefaultCostProvider.Instance);

        var t0 = DateTime.UtcNow;
        var path = solver.Solve(maze, start, end)!;
        var dt = DateTime.UtcNow - t0;

        path.IsFound.Should().BeTrue();
        path.Steps.Count.Should().Be(ExpectedStepsCount(start, end));

        dt.TotalSeconds.Should().BeLessThan(2.5);
    }

    // ------------------------------------------------------------
    // Специальный тест сравнения оптимальности A* и Дейкстры
    // ------------------------------------------------------------

    /// <summary>A* и Дейкстра находят пути одинаковой минимальной стоимости на неоднородной карте</summary>
    [Fact]
    public void AStar_And_Dijkstra_Should_Have_Same_Optimal_Cost_On_Terrain()
    {
        var maze = EmptyMaze(6, 3);
        var map = new TerrainMap(6, 3);
        map[2, 1] = TerrainType.Mud;
        map[3, 1] = TerrainType.Road;

        var provider = new TerrainCostProvider(map);

        var a = CreateSolver("astar", provider).Solve(maze, new Point(0, 1), new Point(5, 1))!;
        var d = CreateSolver("dijkstra", provider).Solve(maze, new Point(0, 1), new Point(5, 1))!;

        a.IsFound.Should().BeTrue();
        d.IsFound.Should().BeTrue();

        PathCost(a, provider, maze).Should().Be(PathCost(d, provider, maze));
    }
}
