using System.Text;
using Serilog;
using TextConvert.Configuration;
using TextConvert.Models;

namespace TextConvert.Services;

/// <summary>
/// 轉換協調服務。掃描來源目錄、逐檔轉換、鏡像寫出到輸出目錄,
/// 單一檔案失敗不中斷整批,並輸出各階段日誌與最終統計。
/// </summary>
public sealed class ConversionService
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    private readonly FileScanner _scanner;
    private readonly ConverterRegistry _registry;
    private readonly ILogger _logger;

    public ConversionService(FileScanner scanner, ConverterRegistry registry, ILogger logger)
    {
        _scanner = scanner;
        _registry = registry;
        _logger = logger;
    }

    /// <summary>
    /// 執行整批轉換。
    /// </summary>
    /// <returns>各檔案的轉換結果清單。</returns>
    public IReadOnlyList<ConversionResult> Run(ConversionOptions options)
    {
        if (!Directory.Exists(options.SourceDirectory))
        {
            throw new DirectoryNotFoundException($"來源目錄不存在:{options.SourceDirectory}");
        }

        Directory.CreateDirectory(options.OutputDirectory);

        _logger.Information("開始轉換。來源={Source}, 輸出={Output}, 遞迴={Recursive}, 覆寫={Overwrite}",
            options.SourceDirectory, options.OutputDirectory, options.Recursive, options.Overwrite);

        var results = new List<ConversionResult>();

        foreach (var filePath in _scanner.EnumerateFiles(options.SourceDirectory, options.Recursive))
        {
            results.Add(ProcessFile(filePath, options));
        }

        var success = results.Count(r => r.Status == ConversionStatus.Success);
        var skipped = results.Count(r => r.Status == ConversionStatus.Skipped);
        var failed = results.Count(r => r.Status == ConversionStatus.Failed);

        _logger.Information("轉換完成。成功={Success}, 跳過={Skipped}, 失敗={Failed}, 總計={Total}",
            success, skipped, failed, results.Count);

        return results;
    }

    private ConversionResult ProcessFile(string filePath, ConversionOptions options)
    {
        var converter = _registry.Resolve(filePath);
        if (converter is null)
        {
            _logger.Debug("跳過不支援的檔案:{File}", filePath);
            return new ConversionResult(filePath, ConversionStatus.Skipped, "不支援的格式");
        }

        var outputPath = BuildOutputPath(filePath, options);

        if (!options.Overwrite && File.Exists(outputPath))
        {
            _logger.Information("輸出已存在且設定為不覆寫,跳過:{File}", filePath);
            return new ConversionResult(filePath, ConversionStatus.Skipped, "輸出已存在");
        }

        try
        {
            var text = converter.Convert(filePath);

            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.Warning("檔案未擷取到任何文字(可能為影像型內容):{File}", filePath);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            File.WriteAllText(outputPath, text, Utf8NoBom);

            _logger.Information("已轉換:{File} -> {Output}", filePath, outputPath);
            return new ConversionResult(filePath, ConversionStatus.Success);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "轉換失敗:{File}", filePath);
            return new ConversionResult(filePath, ConversionStatus.Failed, ex.Message);
        }
    }

    /// <summary>
    /// 計算鏡像輸出路徑:輸出目錄 + 來源相對子路徑 + 原檔名 + ".txt"。
    /// 保留原副檔名(例如 report.docx -> report.docx.txt),避免同名衝突。
    /// </summary>
    private static string BuildOutputPath(string filePath, ConversionOptions options)
    {
        var relativePath = Path.GetRelativePath(options.SourceDirectory, filePath);
        var outputRelative = relativePath + ".txt";
        return Path.Combine(options.OutputDirectory, outputRelative);
    }
}
