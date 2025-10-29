using FluentAssertions;
using Labyrinths.Shell;

namespace Labyrinths.Tests.App;

/// <summary>Тесты парсера аргументов командной строки</summary>
public class CommandParserTests
{
    /// <summary>Парсер должен корректно читать команду и параметры в смешанном синтаксисе</summary>
    [Fact]
    public void Parse_Should_Read_Command_And_Options_Mixed_Syntax()
    {
        var args = new[]
        {
            "generate",
            "--algorithm=dfs",
            "--width", "10",
            "--height=20",
            "--output", "out.txt",
            "--unicode",
            "--frame"
        };

        var ctx = CommandParser.Parse(args);

        ctx.Command.Should().Be("generate");
        ctx.Options["algorithm"].Should().Be("dfs");
        ctx.Options["width"].Should().Be("10");
        ctx.Options["height"].Should().Be("20");
        ctx.Options["output"].Should().Be("out.txt");
        ctx.Options.ContainsKey("unicode").Should().BeTrue();
        ctx.Options.ContainsKey("frame").Should().BeTrue();
    }

    /// <summary>Должен обрезать пробелы и оставлять последнее значение при дублировании ключей</summary>
    [Fact]
    public void Parse_Should_Trim_And_Keep_Last_Duplicate_Key()
    {
        var args = new[]
        {
            "solve",
            "  --file  ", "maze1.txt",
            "--file=maze2.txt",
            "--start", "0,0",
            "--end=9,9"
        };

        var ctx = CommandParser.Parse(args);

        ctx.Command.Should().Be("solve");
        ctx.Options["file"].Should().Be("maze2.txt");
        ctx.Options["start"].Should().Be("0,0");
        ctx.Options["end"].Should().Be("9,9");
    }

    /// <summary>При пустом наборе аргументов должен возвращать пустую команду и параметры</summary>
    [Fact]
    public void Parse_Should_Handle_Empty_Args()
    {
        var ctx = CommandParser.Parse([]);
        ctx.Command.Should().BeNullOrEmpty();
        ctx.Options.Should().BeEmpty();
    }
}
