using TextConvert.Converters;

namespace TextConvert.Tests.Converters;

public class OfficeConvertersTests
{
    [Fact]
    public void WordConverter_ExtractsParagraphs()
    {
        using var ws = new TempWorkspace();
        var path = ws.Path("doc.docx");
        DocumentFixtures.BuildDocx(path, "第一段內容", "第二段內容");

        var result = new WordConverter().Convert(path);

        Assert.Contains("第一段內容", result);
        Assert.Contains("第二段內容", result);
    }

    [Fact]
    public void ExcelConverter_ExtractsSheetAndCells()
    {
        using var ws = new TempWorkspace();
        var path = ws.Path("book.xlsx");
        DocumentFixtures.BuildXlsx(path, "工作表A",
            [["姓名", "城市"], ["小明", "台北"]]);

        var result = new ExcelConverter().Convert(path);

        Assert.Contains("# 工作表A", result);
        Assert.Contains("姓名\t城市", result);
        Assert.Contains("小明\t台北", result);
    }

    [Fact]
    public void PowerPointConverter_ExtractsSlidesInOrder()
    {
        using var ws = new TempWorkspace();
        var path = ws.Path("deck.pptx");
        DocumentFixtures.BuildPptx(path, "投影片一文字", "投影片二文字");

        var result = new PowerPointConverter().Convert(path);

        Assert.Contains("投影片一文字", result);
        Assert.Contains("投影片二文字", result);
        Assert.True(
            result.IndexOf("投影片一文字", StringComparison.Ordinal)
            < result.IndexOf("投影片二文字", StringComparison.Ordinal));
    }

    [Fact]
    public void PdfConverter_ExtractsEmbeddedText()
    {
        using var ws = new TempWorkspace();
        var path = ws.Path("file.pdf");
        DocumentFixtures.BuildPdf(path, "HelloKnowledgeBase");

        var result = new PdfConverter().Convert(path);

        Assert.Contains("HelloKnowledgeBase", result.Replace(" ", string.Empty));
    }
}
