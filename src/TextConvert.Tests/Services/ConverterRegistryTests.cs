using TextConvert.Converters;
using TextConvert.Services;

namespace TextConvert.Tests.Services;

public class ConverterRegistryTests
{
    private static ConverterRegistry BuildRegistry() => new(
    [
        new WordConverter(),
        new ExcelConverter(),
        new PowerPointConverter(),
        new PdfConverter(),
        new HtmlConverter(),
        new MarkdownConverter(),
        new CsvConverter(),
        new TextConverter()
    ]);

    [Theory]
    [InlineData("report.docx", typeof(WordConverter))]
    [InlineData("data.xlsx", typeof(ExcelConverter))]
    [InlineData("deck.pptx", typeof(PowerPointConverter))]
    [InlineData("file.pdf", typeof(PdfConverter))]
    [InlineData("page.html", typeof(HtmlConverter))]
    [InlineData("note.md", typeof(MarkdownConverter))]
    [InlineData("table.csv", typeof(CsvConverter))]
    [InlineData("data.json", typeof(TextConverter))]
    public void Resolve_ReturnsExpectedConverter(string fileName, Type expected)
    {
        var converter = BuildRegistry().Resolve(fileName);
        Assert.IsType(expected, converter);
    }

    [Fact]
    public void Resolve_IsCaseInsensitive()
    {
        Assert.IsType<WordConverter>(BuildRegistry().Resolve("REPORT.DOCX"));
    }

    [Fact]
    public void Resolve_UnsupportedExtension_ReturnsNull()
    {
        Assert.Null(BuildRegistry().Resolve("archive.zip"));
        Assert.False(BuildRegistry().IsSupported("archive.zip"));
    }
}
