namespace TextConvert.Models;

/// <summary>
/// 單一檔案的轉換結果狀態。
/// </summary>
public enum ConversionStatus
{
    /// <summary>成功轉換並寫出。</summary>
    Success,

    /// <summary>已跳過(不支援格式,或設定為不覆寫且輸出已存在)。</summary>
    Skipped,

    /// <summary>轉換過程發生例外而失敗。</summary>
    Failed
}

/// <summary>
/// 單一檔案的轉換結果。
/// </summary>
/// <param name="SourcePath">來源檔案路徑。</param>
/// <param name="Status">結果狀態。</param>
/// <param name="Message">附加說明(跳過原因或錯誤訊息)。</param>
public sealed record ConversionResult(string SourcePath, ConversionStatus Status, string? Message = null);
