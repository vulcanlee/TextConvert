namespace TextConvert.Converters;

/// <summary>
/// 純文字型檔案轉換器。處理 .txt / .text / .json / .xml,
/// 這些檔案本身即為文字,直接讀取原始內容輸出,不做結構解析。
/// </summary>
public sealed class TextConverter : IDocumentConverter
{
    private static readonly string[] Extensions = [".txt", ".text", ".json", ".xml"];

    public bool CanHandle(string extension) =>
        Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase);

    public string Convert(string filePath) => File.ReadAllText(filePath);
}
