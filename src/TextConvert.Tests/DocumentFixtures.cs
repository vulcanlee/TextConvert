using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;
using D = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;
using S = DocumentFormat.OpenXml.Spreadsheet;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace TextConvert.Tests;

/// <summary>
/// 於測試時動態產生各格式的最小樣本檔,避免在版控中提交二進位檔。
/// </summary>
public static class DocumentFixtures
{
    /// <summary>建立含指定段落文字的 .docx。</summary>
    public static void BuildDocx(string path, params string[] paragraphs)
    {
        using var document = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = document.AddMainDocumentPart();
        var body = new W.Body();

        foreach (var text in paragraphs)
        {
            body.AppendChild(new W.Paragraph(new W.Run(new W.Text(text) { Space = SpaceProcessingModeValues.Preserve })));
        }

        mainPart.Document = new W.Document(body);
        mainPart.Document.Save();
    }

    /// <summary>建立單一工作表、以共用字串儲存內容的 .xlsx。</summary>
    public static void BuildXlsx(string path, string sheetName, string[][] rows)
    {
        using var document = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook);
        var workbookPart = document.AddWorkbookPart();
        workbookPart.Workbook = new S.Workbook();

        var sharedStringPart = workbookPart.AddNewPart<SharedStringTablePart>();
        sharedStringPart.SharedStringTable = new S.SharedStringTable();
        var stringIndex = new Dictionary<string, int>();

        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        var sheetData = new S.SheetData();

        foreach (var row in rows)
        {
            var newRow = new S.Row();
            foreach (var value in row)
            {
                var index = ResolveSharedString(sharedStringPart, stringIndex, value);
                newRow.AppendChild(new S.Cell
                {
                    DataType = S.CellValues.SharedString,
                    CellValue = new S.CellValue(index.ToString())
                });
            }

            sheetData.AppendChild(newRow);
        }

        worksheetPart.Worksheet = new S.Worksheet(sheetData);

        var sheets = workbookPart.Workbook.AppendChild(new S.Sheets());
        sheets.AppendChild(new S.Sheet
        {
            Id = workbookPart.GetIdOfPart(worksheetPart),
            SheetId = 1,
            Name = sheetName
        });

        workbookPart.Workbook.Save();
    }

    /// <summary>建立每張投影片含指定文字的 .pptx(最小可讀結構)。</summary>
    public static void BuildPptx(string path, params string[] slideTexts)
    {
        using var document = PresentationDocument.Create(path, PresentationDocumentType.Presentation);
        var presentationPart = document.AddPresentationPart();
        presentationPart.Presentation = new P.Presentation();

        var slideIdList = new P.SlideIdList();
        uint slideId = 256;

        foreach (var text in slideTexts)
        {
            var slidePart = presentationPart.AddNewPart<SlidePart>();
            slidePart.Slide = new P.Slide(
                new P.CommonSlideData(
                    new P.ShapeTree(
                        new P.Shape(
                            new P.TextBody(
                                new D.BodyProperties(),
                                new D.ListStyle(),
                                new D.Paragraph(new D.Run(new D.Text(text))))))));

            slideIdList.AppendChild(new P.SlideId
            {
                Id = slideId++,
                RelationshipId = presentationPart.GetIdOfPart(slidePart)
            });
        }

        presentationPart.Presentation.AppendChild(slideIdList);
        presentationPart.Presentation.Save();
    }

    /// <summary>建立含指定文字的單頁 .pdf。</summary>
    public static void BuildPdf(string path, string text)
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        var page = builder.AddPage(595, 842);
        page.AddText(text, 12, new UglyToad.PdfPig.Core.PdfPoint(50, 700), font);
        File.WriteAllBytes(path, builder.Build());
    }

    private static int ResolveSharedString(
        SharedStringTablePart part, Dictionary<string, int> index, string value)
    {
        if (index.TryGetValue(value, out var existing))
        {
            return existing;
        }

        part.SharedStringTable!.AppendChild(new S.SharedStringItem(new S.Text(value)));
        var newIndex = index.Count;
        index[value] = newIndex;
        return newIndex;
    }
}
