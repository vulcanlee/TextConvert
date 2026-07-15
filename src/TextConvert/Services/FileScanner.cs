namespace TextConvert.Services;

/// <summary>
/// 檔案掃描器。列舉來源目錄下的檔案(可選擇是否遞迴)。
/// </summary>
public sealed class FileScanner
{
    /// <summary>
    /// 列舉來源目錄中的所有檔案。
    /// </summary>
    /// <param name="sourceDirectory">來源目錄。</param>
    /// <param name="recursive">是否包含子目錄。</param>
    public IEnumerable<string> EnumerateFiles(string sourceDirectory, bool recursive)
    {
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        return Directory.EnumerateFiles(sourceDirectory, "*", searchOption);
    }
}
