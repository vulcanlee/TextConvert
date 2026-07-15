using TextConvert.Services;

namespace TextConvert.Tests.Services;

public class FileScannerTests
{
    [Fact]
    public void EnumerateFiles_Recursive_IncludesSubdirectories()
    {
        using var ws = new TempWorkspace();
        ws.WriteText("a.txt", "a");
        ws.WriteText("sub/b.txt", "b");
        ws.WriteText("sub/deep/c.txt", "c");

        var files = new FileScanner().EnumerateFiles(ws.Root, recursive: true).ToList();

        Assert.Equal(3, files.Count);
    }

    [Fact]
    public void EnumerateFiles_NonRecursive_OnlyTopLevel()
    {
        using var ws = new TempWorkspace();
        ws.WriteText("a.txt", "a");
        ws.WriteText("sub/b.txt", "b");

        var files = new FileScanner().EnumerateFiles(ws.Root, recursive: false).ToList();

        Assert.Single(files);
        Assert.EndsWith("a.txt", files[0]);
    }
}
