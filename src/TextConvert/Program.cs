using System.Text;
using Microsoft.Extensions.Configuration;
using Serilog;
using TextConvert.Cli;
using TextConvert.Configuration;
using TextConvert.Converters;
using TextConvert.Services;

// 確保主控台以 UTF-8 輸出,避免繁體中文在 Windows 主控台顯示為亂碼
//(同時影響 Console 輸出與 Serilog 主控台 sink)。
try
{
    Console.OutputEncoding = Encoding.UTF8;
}
catch (IOException)
{
    // 輸出被重導向等特殊情況下無法設定編碼,可忽略。
}

var options = CommandLineParser.Parse(args);

// 先處理 -h/--help:在載入設定與初始化 Serilog 之前,確保說明永不受設定影響。
if (options.ShowHelp)
{
    Console.WriteLine(UsageText.Value);
    return 0;
}

// 設定檔:以執行檔所在目錄為基準,確保從任何工作目錄執行都能找到 appsettings.json。
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var settings = configuration.Get<AppSettings>() ?? new AppSettings();

// 處理 --version/-v:顯示版本後結束(不需初始化 Serilog)。
if (options.ShowVersion)
{
    Console.WriteLine($"TextConvert 版本 {settings.Version}");
    return 0;
}

// 由設定檔初始化 Serilog(Console + 每日滾動檔案)。
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("TextConvert 版本 {Version} 啟動。", settings.Version);

    // 命令列參數覆寫設定檔的來源/輸出目錄。
    if (!string.IsNullOrWhiteSpace(options.Source))
    {
        settings.Conversion.SourceDirectory = options.Source;
    }

    if (!string.IsNullOrWhiteSpace(options.Output))
    {
        settings.Conversion.OutputDirectory = options.Output;
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
