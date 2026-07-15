using Serilog;
using TextConvert.Configuration;
using TextConvert.Converters;
using TextConvert.Models;
using TextConvert.Services;

namespace TextConvert.Tests.Services;

public class ConversionServiceTests
{
    private static readonly ILogger SilentLogger = new LoggerConfiguration().CreateLogger();

    private static ConversionService BuildService(params IDocumentConverter[] converters) =>
        new(new FileScanner(), new ConverterRegistry(converters), SilentLogger);

    private static ConversionOptions Options(string source, string output, bool overwrite = true) =>
        new()
        {
            SourceDirectory = source,
            OutputDirectory = output,
            Recursive = true,
            Overwrite = overwrite
        };

    [Fact]
    public void Run_MirrorsStructure_And_KeepsOriginalExtension()
    {
        using var src = new TempWorkspace();
        using var outDir = new TempWorkspace();
        src.WriteText("a.txt", "頂層內容");
        src.WriteText("sub/b.txt", "子目錄內容");

        var service = BuildService(new TextConverter());
        var results = service.Run(Options(src.Root, outDir.Root));

        Assert.All(results, r => Assert.Equal(ConversionStatus.Success, r.Status));
        Assert.True(File.Exists(Path.Combine(outDir.Root, "a.txt.txt")));
        Assert.True(File.Exists(Path.Combine(outDir.Root, "sub", "b.txt.txt")));
        Assert.Equal("頂層內容", File.ReadAllText(Path.Combine(outDir.Root, "a.txt.txt")));
    }

    [Fact]
    public void Run_SkipsUnsupportedFiles()
    {
        using var src = new TempWorkspace();
        using var outDir = new TempWorkspace();
        src.WriteText("keep.txt", "文字");
        src.WriteText("ignore.zip", "二進位");

        var results = BuildService(new TextConverter()).Run(Options(src.Root, outDir.Root));

        Assert.Equal(ConversionStatus.Skipped, results.Single(r => r.SourcePath.EndsWith("ignore.zip")).Status);
        Assert.False(File.Exists(Path.Combine(outDir.Root, "ignore.zip.txt")));
    }

    [Fact]
    public void Run_ContinuesAfterFailure()
    {
        using var src = new TempWorkspace();
        using var outDir = new TempWorkspace();
        src.WriteText("ok.txt", "正常");
        src.WriteText("boom.bad", "會失敗");

        var results = BuildService(new TextConverter(), new ThrowingConverter()).Run(Options(src.Root, outDir.Root));

        Assert.Equal(ConversionStatus.Success, results.Single(r => r.SourcePath.EndsWith("ok.txt")).Status);
        Assert.Equal(ConversionStatus.Failed, results.Single(r => r.SourcePath.EndsWith("boom.bad")).Status);
        Assert.True(File.Exists(Path.Combine(outDir.Root, "ok.txt.txt")));
    }

    [Fact]
    public void Run_DoesNotOverwrite_WhenDisabled()
    {
        using var src = new TempWorkspace();
        using var outDir = new TempWorkspace();
        src.WriteText("a.txt", "新內容");
        var existing = Path.Combine(outDir.Root, "a.txt.txt");
        File.WriteAllText(existing, "舊內容");

        var results = BuildService(new TextConverter()).Run(Options(src.Root, outDir.Root, overwrite: false));

        Assert.Equal(ConversionStatus.Skipped, results.Single().Status);
        Assert.Equal("舊內容", File.ReadAllText(existing));
    }

    [Fact]
    public void Run_MissingSourceDirectory_Throws()
    {
        using var outDir = new TempWorkspace();
        var service = BuildService(new TextConverter());

        Assert.Throws<DirectoryNotFoundException>(() =>
            service.Run(Options(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), outDir.Root)));
    }

    /// <summary>永遠拋出例外的測試用轉換器,處理 .bad 副檔名。</summary>
    private sealed class ThrowingConverter : IDocumentConverter
    {
        public bool CanHandle(string extension) =>
            string.Equals(extension, ".bad", StringComparison.OrdinalIgnoreCase);

        public string Convert(string filePath) => throw new InvalidOperationException("模擬轉換失敗");
    }
}
