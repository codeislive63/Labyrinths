namespace Labyrinths.Shell;

/// <summary>Парсер аргументов командной строки</summary>
public sealed class CommandParser
{
    /// <summary>Разбирает аргументы и создаёт контекст команды</summary>
    public static CommandContext Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length == 0)
        {
            return new CommandContext(string.Empty, new Dictionary<string, string>());
        }

        var command = (args[0] ?? string.Empty).Trim();
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 1; i < args.Length; i++)
        {
            var raw = args[i];

            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            var s = raw.Trim();

            if (!s.StartsWith("--"))
            {
                continue;
            }

            var body = s[2..].Trim();
            if (body.Length == 0)
            {
                continue;
            }

            var parts = body.Split('=', 2, StringSplitOptions.None);
            var key = parts[0].Trim();

            if (parts.Length == 2)
            {
                options[key] = parts[1].Trim();
                continue;
            }

            if (i + 1 < args.Length)
            {
                var next = args[i + 1]?.Trim();
                if (!string.IsNullOrEmpty(next) && !next.StartsWith("--"))
                {
                    options[key] = next;
                    i++;
                    continue;
                }
            }

            options[key] = bool.TrueString;
        }

        return new CommandContext(command, options);
    }
}
