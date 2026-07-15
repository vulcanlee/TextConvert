namespace TextConvert.Cli;

/// <summary>
/// 命令列覆寫值。未提供的項目為 null,表示採用 appsettings.json 的設定。
/// </summary>
/// <param name="Source">--source 覆寫的來源目錄。</param>
/// <param name="Output">--output 覆寫的輸出目錄。</param>
public sealed record CommandLineOverrides(string? Source, string? Output);

/// <summary>
/// 輕量命令列解析。支援 --source/-s 與 --output/-o(值以空白或 = 分隔)。
/// </summary>
public static class CommandLineParser
{
    public static CommandLineOverrides Parse(string[] args)
    {
        string? source = null;
        string? output = null;

        for (var i = 0; i < args.Length; i++)
        {
            var (key, inlineValue) = SplitArg(args[i]);

            switch (key)
            {
                case "--source" or "-s":
                    source = inlineValue ?? TakeNext(args, ref i);
                    break;
                case "--output" or "-o":
                    output = inlineValue ?? TakeNext(args, ref i);
                    break;
            }
        }

        return new CommandLineOverrides(source, output);
    }

    private static (string Key, string? InlineValue) SplitArg(string arg)
    {
        var equalsIndex = arg.IndexOf('=');
        return equalsIndex >= 0
            ? (arg[..equalsIndex], arg[(equalsIndex + 1)..])
            : (arg, null);
    }

    private static string? TakeNext(string[] args, ref int index)
    {
        if (index + 1 < args.Length && !args[index + 1].StartsWith('-'))
        {
            return args[++index];
        }

        return null;
    }
}
