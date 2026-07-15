using TextConvert.Converters;

namespace TextConvert.Tests.Converters;

public class PlainTextConvertersTests
{
    [Theory]
    [InlineData(".txt")]
    [InlineData(".text")]
    [InlineData(".json")]
    [InlineData(".xml")]
    public void TextConverter_Handles_TextLikeExtensions(string extension)
    {
        Assert.True(new TextConverter().CanHandle(extension));
    }

    [Fact]
    public void TextConverter_ReturnsRawContent()
    {
        using var ws = new TempWorkspace();
        var path = ws.WriteText("data.json", "{\"名稱\": \"知識庫\"}");

        var result = new TextConverter().Convert(path);

        Assert.Equal("{\"名稱\": \"知識庫\"}", result);
    }

    [Fact]
    public void MarkdownConverter_StripsSyntax()
    {
        using var ws = new TempWorkspace();
        var path = ws.WriteText("doc.md", "# 標題\n\n這是**粗體**與 `程式碼`。");

        var result = new MarkdownConverter().Convert(path);

        Assert.Contains("標題", result);
        Assert.Contains("粗體", result);
        Assert.DoesNotContain("**", result);
        Assert.DoesNotContain("#", result);
    }

    [Fact]
    public void HtmlConverter_RemovesTagsAndScript()
    {
        using var ws = new TempWorkspace();
        var html = "<html><head><style>.x{}</style></head><body>" +
                   "<h1>標題</h1><p>第一段</p><script>ignore()</script><p>第二段</p></body></html>";
        var path = ws.WriteText("page.html", html);

        var result = new HtmlConverter().Convert(path);

        Assert.Contains("標題", result);
        Assert.Contains("第一段", result);
        Assert.Contains("第二段", result);
        Assert.DoesNotContain("ignore", result);
        Assert.DoesNotContain("<", result);
    }
}
