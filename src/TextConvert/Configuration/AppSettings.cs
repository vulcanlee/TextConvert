namespace TextConvert.Configuration;

/// <summary>
/// 對應 appsettings.json 的設定。
/// </summary>
public sealed class AppSettings
{
    /// <summary>應用程式版本(每次建置 patch +1)。</summary>
    public string Version { get; set; } = "0.0.0";

    /// <summary>轉換相關設定。</summary>
    public ConversionOptions Conversion { get; set; } = new();
}

/// <summary>
/// 轉換行為設定。來源/輸出目錄可由命令列參數覆寫。
/// </summary>
public sealed class ConversionOptions
{
    /// <summary>來源目錄。</summary>
    public string SourceDirectory { get; set; } = string.Empty;

    /// <summary>輸出目錄。</summary>
    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>是否遞迴掃描子目錄。</summary>
    public bool Recursive { get; set; } = true;

    /// <summary>輸出檔已存在時是否覆寫。</summary>
    public bool Overwrite { get; set; } = true;
}
