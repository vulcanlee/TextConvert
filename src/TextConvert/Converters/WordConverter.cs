using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace TextConvert.Converters;

/// <summary>
/// Word(.docx)轉換器。使用 OpenXML SDK 逐段落擷取文字,
/// 表格以每列 Tab 分隔、每格文字串接的方式輸出。
/// </summary>
public sealed class WordConverter : IDocumentConverter
{
    public bool CanHandle(string extension) =>
        string.Equals(extension, ".docx", StringComparison.OrdinalIgnoreCase);

    public string Convert(string filePath)
    {
        using var document = WordprocessingDocument.Open(filePath, false);
        var body = document.MainDocumentPart?.Document?.Body;
        if (body is null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        // 只走訪 Body 的直接子元素,避免表格內段落被重複輸出。
        foreach (var element in body.Elements())
        {
            switch (element)
            {
                case Paragraph paragraph:
                    builder.AppendLine(GetParagraphText(paragraph));
                    break;
                case Table table:
                    AppendTable(table, builder);
                    break;
            }
        }

        return builder.ToString().Trim();
    }

    private static string GetParagraphText(Paragraph paragraph)
    {
        var builder = new StringBuilder();
        foreach (var child in paragraph.Descendants())
        {
            switch (child)
            {
                case Text text:
                    builder.Append(text.Text);
                    break;
                case TabChar:
                    builder.Append('\t');
                    break;
                case Break or CarriageReturn:
                    builder.Append('\n');
                    break;
            }
        }

        return builder.ToString();
    }

    private static void AppendTable(Table table, StringBuilder builder)
    {
        foreach (var row in table.Elements<TableRow>())
        {
            var cells = row.Elements<TableCell>()
                .Select(cell => string.Join(" ",
                    cell.Descendants<Paragraph>().Select(GetParagraphText))
                    .Trim());
            builder.AppendLine(string.Join('\t', cells));
        }

        builder.AppendLine();
    }
}
