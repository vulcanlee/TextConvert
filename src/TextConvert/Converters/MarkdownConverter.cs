using Markdig;

namespace TextConvert.Converters;

/// <summary>
/// Markdown 轉換器。使用 Markdig 將 Markdown 轉為純文字(去除標記語法,保留段落文字)。
/// </summary>
public sealed class MarkdownConverter : IDocumentConverter
{
    private static readonly string[] Extensions = [".md", ".markdown"];

    public bool CanHandle(string extension) =>
        Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase);

    public string Convert(string filePath)
    {
        var markdown = File.ReadAllText(filePath);
        return Markdown.ToPlainText(markdown).Trim();
    }
}
