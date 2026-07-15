namespace TextConvert.Cli;

/// <summary>
/// 命令列解析結果。來源/輸出為 null 時表示採用 appsettings.json 的設定。
/// </summary>
/// <param name="Source">--source 覆寫的來源目錄。</param>
/// <param name="Output">--output 覆寫的輸出目錄。</param>
/// <param name="ShowHelp">是否要求顯示說明(-h/--help/-?)。</param>
/// <param name="ShowVersion">是否要求顯示版本(--version/-v)。</param>
public sealed record CommandLineOptions(string? Source, string? Output, bool ShowHelp, bool ShowVersion);

/// <summary>
/// 輕量命令列解析。支援 --source/-s、--output/-o(值以空白或 = 分隔)、
/// -h/--help/-? 與 --version/-v。
/// </summary>
public static class CommandLineParser
{
    public static CommandLineOptions Parse(string[] args)
    {
        string? source = null;
        string? output = null;
        var showHelp = false;
        var showVersion = false;

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
                case "-h" or "--help" or "-?" or "/?":
                    showHelp = true;
                    break;
                case "--version" or "-v":
                    showVersion = true;
                    break;
            }
        }

        return new CommandLineOptions(source, output, showHelp, showVersion);
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
