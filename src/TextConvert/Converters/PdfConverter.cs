using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace TextConvert.Converters;

/// <summary>
/// PDF 轉換器。使用 PdfPig 逐頁擷取內嵌文字(不做 OCR)。
/// 影像型 PDF 無內嵌文字時會回傳空字串,由呼叫端記錄警告。
/// </summary>
public sealed class PdfConverter : IDocumentConverter
{
    public bool CanHandle(string extension) =>
        string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase);

    public string Convert(string filePath)
    {
        var builder = new StringBuilder();
        using var document = PdfDocument.Open(filePath);

        foreach (var page in document.GetPages())
        {
            var text = ExtractPageText(page);
            if (!string.IsNullOrWhiteSpace(text))
            {
                builder.AppendLine(text);
                builder.AppendLine();
            }
        }

        return builder.ToString().Trim();
    }

    /// <summary>
    /// 依單字的垂直位置(基線)分行、水平位置排序,還原較自然的閱讀文字。
    /// </summary>
    private static string ExtractPageText(Page page)
    {
        var words = page.GetWords().ToList();
        if (words.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        // 依基線分群為「列」,同列內以左緣位置排序。
        var lines = words
            .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 1))
            .OrderByDescending(g => g.Key);

        foreach (var line in lines)
        {
            var ordered = line
                .OrderBy(w => w.BoundingBox.Left)
                .Select(w => w.Text);
            builder.AppendLine(string.Join(' ', ordered));
        }

        return builder.ToString().Trim();
    }
}
