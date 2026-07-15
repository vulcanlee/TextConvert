using TextConvert.Converters;

namespace TextConvert.Tests.Converters;

public class CsvConverterTests
{
    [Fact]
    public void CanHandle_Csv()
    {
        Assert.True(new CsvConverter().CanHandle(".csv"));
        Assert.True(new CsvConverter().CanHandle(".CSV"));
        Assert.False(new CsvConverter().CanHandle(".txt"));
    }

    [Fact]
    public void Convert_ParsesRowsTabSeparated()
    {
        using var ws = new TempWorkspace();
        var path = ws.WriteText("data.csv", "姓名,年齡\n小明,18\n小華,20");

        var result = new CsvConverter().Convert(path);
        var lines = result.Split(Environment.NewLine);

        Assert.Equal("姓名\t年齡", lines[0]);
        Assert.Equal("小明\t18", lines[1]);
        Assert.Equal("小華\t20", lines[2]);
    }

    [Fact]
    public void Convert_HandlesQuotedFieldsWithCommaAndQuote()
    {
        using var ws = new TempWorkspace();
        // 欄位含逗號與跳脫引號:"台北, 台灣" 與 "他說 ""你好"""
        var path = ws.WriteText("q.csv", "地點,備註\n\"台北, 台灣\",\"他說 \"\"你好\"\"\"");

        var result = new CsvConverter().Convert(path);
        var lines = result.Split(Environment.NewLine);

        Assert.Equal("地點\t備註", lines[0]);
        Assert.Equal("台北, 台灣\t他說 \"你好\"", lines[1]);
    }

    [Fact]
    public void Convert_HandlesQuotedNewlineWithinField()
    {
        using var ws = new TempWorkspace();
        var path = ws.WriteText("n.csv", "標題,內容\n第一則,\"第一行\n第二行\"");

        var result = new CsvConverter().Convert(path);
        var rows = result.Split(Environment.NewLine);

        // 欄位內的換行被保留於同一欄位,不會被誤判為列分隔:總共只有兩列。
        Assert.Equal(2, rows.Length);
        Assert.Equal("標題\t內容", rows[0]);
        Assert.Contains("第一行\n第二行", result);
    }
}
