using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace TextConvert.Converters;

/// <summary>
/// HTML 轉換器。使用 HtmlAgilityPack 去除標籤,保留可讀文字與段落換行。
/// </summary>
public sealed class HtmlConverter : IDocumentConverter
{
    private static readonly string[] Extensions = [".html", ".htm"];

    // 這些節點不含可讀內容,整段略過。
    private static readonly HashSet<string> SkipNodes =
        new(StringComparer.OrdinalIgnoreCase) { "script", "style", "head", "#comment" };

    // 這些區塊元素在其文字後補一個換行,維持段落結構。
    private static readonly HashSet<string> BlockNodes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "p", "div", "br", "li", "tr", "h1", "h2", "h3", "h4", "h5", "h6",
            "section", "article", "header", "footer", "table", "ul", "ol", "blockquote"
        };

    public bool CanHandle(string extension) =>
        Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase);

    public string Convert(string filePath)
    {
        var doc = new HtmlDocument();
        doc.Load(filePath, Encoding.UTF8);

        var builder = new StringBuilder();
        AppendText(doc.DocumentNode, builder);

        // 正規化多餘空白行:連續換行壓成最多兩個。
        var lines = builder.ToString()
            .Replace("\r\n", "\n")
            .Split('\n')
            .Select(line => line.Trim());

        var result = new StringBuilder();
        var blankRun = 0;
        foreach (var line in lines)
        {
            if (line.Length == 0)
            {
                if (++blankRun <= 1)
                {
                    result.AppendLine();
                }
                continue;
            }

            blankRun = 0;
            result.AppendLine(line);
        }

        return result.ToString().Trim();
    }

    private static void AppendText(HtmlNode node, StringBuilder builder)
    {
        if (SkipNodes.Contains(node.Name))
        {
            return;
        }

        if (node.NodeType == HtmlNodeType.Text)
        {
            var text = WebUtility.HtmlDecode(node.InnerText);
            builder.Append(text);
            return;
        }

        foreach (var child in node.ChildNodes)
        {
            AppendText(child, builder);
        }

        if (BlockNodes.Contains(node.Name))
        {
            builder.Append('\n');
        }
    }
}
