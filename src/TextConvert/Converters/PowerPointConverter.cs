using System.Text;
using DocumentFormat.OpenXml.Packaging;
using A = DocumentFormat.OpenXml.Drawing;

namespace TextConvert.Converters;

/// <summary>
/// PowerPoint(.pptx)轉換器。使用 OpenXML SDK 依投影片順序擷取各文字方塊內容。
/// </summary>
public sealed class PowerPointConverter : IDocumentConverter
{
    public bool CanHandle(string extension) =>
        string.Equals(extension, ".pptx", StringComparison.OrdinalIgnoreCase);

    public string Convert(string filePath)
    {
        using var document = PresentationDocument.Open(filePath, false);
        var presentationPart = document.PresentationPart;
        if (presentationPart is null)
        {
            return string.Empty;
        }

        var slideIdList = presentationPart.Presentation?.SlideIdList;
        if (slideIdList is null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        var slideNumber = 1;

        // 依 SlideIdList 的順序走訪,確保投影片順序正確。
        foreach (var slideId in slideIdList.Elements<DocumentFormat.OpenXml.Presentation.SlideId>())
        {
            if (slideId.RelationshipId?.Value is not { } relationshipId)
            {
                continue;
            }

            var slidePart = (SlidePart)presentationPart.GetPartById(relationshipId);
            builder.AppendLine($"# 投影片 {slideNumber}");

            foreach (var paragraph in slidePart.Slide?.Descendants<A.Paragraph>() ?? [])
            {
                var text = string.Concat(paragraph.Descendants<A.Text>().Select(t => t.Text));
                builder.AppendLine(text);
            }

            builder.AppendLine();
            slideNumber++;
        }

        return builder.ToString().Trim();
    }
}
