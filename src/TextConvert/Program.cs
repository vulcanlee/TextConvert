using Microsoft.Extensions.Configuration;
using Serilog;
using TextConvert.Cli;
using TextConvert.Configuration;
using TextConvert.Converters;
using TextConvert.Services;

// 設定檔:以執行檔所在目錄為基準,確保從任何工作目錄執行都能找到 appsettings.json。
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

// 由設定檔初始化 Serilog(Console + 每日滾動檔案)。
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    var settings = configuration.Get<AppSettings>() ?? new AppSettings();
    Log.Information("TextConvert 版本 {Version} 啟動。", settings.Version);

    // 命令列參數覆寫設定檔的來源/輸出目錄。
    var overrides = CommandLineParser.Parse(args);
    if (!string.IsNullOrWhiteSpace(overrides.Source))
    {
        settings.Conversion.SourceDirectory = overrides.Source;
    }

    if (!string.IsNullOrWhiteSpace(overrides.Output))
    {
        settings.Conversion.OutputDirectory = overrides.Output;
    }

    if (string.IsNullOrWhiteSpace(settings.Conversion.SourceDirectory)
        || string.IsNullOrWhiteSpace(settings.Conversion.OutputDirectory))
    {
        Log.Error("尚未指定來源或輸出目錄。請於 appsettings.json 設定,或使用 --source 與 --output 參數。");
        return 1;
    }

    // 組合各格式轉換器(YAGNI:手動組合,不使用 DI 容器)。
    var registry = new ConverterRegistry(
    [
        new WordConverter(),
        new ExcelConverter(),
        new PowerPointConverter(),
        new PdfConverter(),
        new HtmlConverter(),
        new MarkdownConverter(),
        new CsvConverter(),
        new TextConverter()
    ]);

    var service = new ConversionService(new FileScanner(), registry, Log.Logger);
    service.Run(settings.Conversion);
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "程式發生未預期的例外而終止。");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
