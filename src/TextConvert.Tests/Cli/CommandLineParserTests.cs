using TextConvert.Cli;

namespace TextConvert.Tests.Cli;

public class CommandLineParserTests
{
    [Fact]
    public void Parse_SourceAndOutput_SpaceSeparated()
    {
        var options = CommandLineParser.Parse(["--source", "C:\\來源", "--output", "C:\\輸出"]);

        Assert.Equal("C:\\來源", options.Source);
        Assert.Equal("C:\\輸出", options.Output);
        Assert.False(options.ShowHelp);
        Assert.False(options.ShowVersion);
    }

    [Fact]
    public void Parse_SourceAndOutput_EqualsSeparated_WithShortForms()
    {
        var options = CommandLineParser.Parse(["-s=C:\\來源", "-o=C:\\輸出"]);

        Assert.Equal("C:\\來源", options.Source);
        Assert.Equal("C:\\輸出", options.Output);
    }

    [Theory]
    [InlineData("-h")]
    [InlineData("--help")]
    [InlineData("-?")]
    public void Parse_HelpFlags_SetShowHelp(string flag)
    {
        Assert.True(CommandLineParser.Parse([flag]).ShowHelp);
    }

    [Theory]
    [InlineData("-v")]
    [InlineData("--version")]
    public void Parse_VersionFlags_SetShowVersion(string flag)
    {
        Assert.True(CommandLineParser.Parse([flag]).ShowVersion);
    }

    [Fact]
    public void Parse_NoArgs_AllDefaults()
    {
        var options = CommandLineParser.Parse([]);

        Assert.Null(options.Source);
        Assert.Null(options.Output);
        Assert.False(options.ShowHelp);
        Assert.False(options.ShowVersion);
    }

    [Fact]
    public void Parse_MixedFlags_Combined()
    {
        var options = CommandLineParser.Parse(["-s", "C:\\來源", "--help"]);

        Assert.Equal("C:\\來源", options.Source);
        Assert.True(options.ShowHelp);
    }
}
