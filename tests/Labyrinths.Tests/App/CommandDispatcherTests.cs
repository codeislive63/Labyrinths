using FluentAssertions;
using Labyrinths.CLI;
using Labyrinths.Shell;

namespace Labyrinths.Tests.App;

public class CommandDispatcherTests
{
    private sealed class FakeCommand(string name, int code = 0) : IConsoleCommand
    {
        public string Name { get; } = name;

        public static string Description => "fake";

        public CommandContext? Received { get; private set; }

        private readonly int _code = code;

        public Task<int> ExecuteAsync(CommandContext context)
        {
            Received = context;
            return Task.FromResult(_code);
        }
    }

    [Fact]
    public async Task Dispatch_Should_Invoke_Command_By_Name()
    {
        var gen = new FakeCommand("generate", 7);
        var solve = new FakeCommand("solve", 9);
        var help = new FakeCommand("help", 1);

        var dispatcher = new CommandDispatcher([gen, solve, help]);

        var ctx = new CommandContext("solve", new Dictionary<string, string>
        {
            ["file"] = "maze.txt"
        });

        var code = await dispatcher.DispatchAsync(ctx);

        code.Should().Be(9);
        solve.Received.Should().NotBeNull();
        solve.Received!.Options["file"].Should().Be("maze.txt");
    }

    [Fact]
    public async Task Dispatch_Should_Fallback_To_Help_When_Unknown()
    {
        var help = new FakeCommand("help", 42);
        var dispatcher = new CommandDispatcher([help]);
        var ctx = new CommandContext("unknown", new Dictionary<string, string>());

        var code = await dispatcher.DispatchAsync(ctx);

        code.Should().Be(42);
        help.Received.Should().NotBeNull();
    }
}
