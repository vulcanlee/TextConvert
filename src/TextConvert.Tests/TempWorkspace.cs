namespace TextConvert.Tests;

/// <summary>
/// 測試用暫存目錄,離開範圍時自動清除。
/// </summary>
public sealed class TempWorkspace : IDisposable
{
    public string Root { get; }

    public TempWorkspace()
    {
        Root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "TextConvertTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Root);
    }

    /// <summary>取得暫存目錄下的完整路徑(必要時建立父目錄)。</summary>
    public string Path(string relative)
    {
        var full = System.IO.Path.Combine(Root, relative);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(full)!);
        return full;
    }

    /// <summary>寫入文字檔並回傳其路徑。</summary>
    public string WriteText(string relative, string content)
    {
        var full = Path(relative);
        File.WriteAllText(full, content);
        return full;
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
        catch (IOException)
        {
            // 測試清理失敗不影響結果。
        }
    }
}
