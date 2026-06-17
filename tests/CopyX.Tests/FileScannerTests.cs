using CopyX.Models;
using CopyX.Services;

namespace CopyX.Tests;

public sealed class FileScannerTests
{
    [Fact]
    public void Scan_WithSingleFile_CreatesSingleManifestItem()
    {
        using var temp = TemporaryWorkspace.Create();
        var source = temp.WriteFile("source.txt", "hello");
        var destination = temp.PathFor("destination.txt");

        var manifest = new FileScanner().Scan(new CopyOptions { SourcePath = source, DestinationPath = destination });

        var item = Assert.Single(manifest);
        Assert.Equal(source, item.SourcePath);
        Assert.Equal(destination, item.DestinationPath);
        Assert.Equal(5, item.SizeInBytes);
    }

    [Fact]
    public void Scan_WithDirectory_CreatesRecursiveManifest()
    {
        using var temp = TemporaryWorkspace.Create();
        var sourceRoot = temp.CreateDirectory("source");
        Directory.CreateDirectory(Path.Combine(sourceRoot, "nested"));
        File.WriteAllText(Path.Combine(sourceRoot, "root.txt"), "root");
        File.WriteAllText(Path.Combine(sourceRoot, "nested", "child.txt"), "child");
        var destinationRoot = temp.PathFor("destination");

        var manifest = new FileScanner().Scan(new CopyOptions { SourcePath = sourceRoot, DestinationPath = destinationRoot });

        Assert.Equal(2, manifest.Count);
        Assert.Contains(manifest, item => item.DestinationPath == Path.Combine(destinationRoot, "root.txt"));
        Assert.Contains(manifest, item => item.DestinationPath == Path.Combine(destinationRoot, "nested", "child.txt"));
    }

    [Fact]
    public void Scan_WithSingleFileAndExistingDirectory_UsesDirectoryAsDestinationRoot()
    {
        using var temp = TemporaryWorkspace.Create();
        var source = temp.WriteFile("source.txt", "hello");
        var destinationDirectory = temp.CreateDirectory("destination");

        var manifest = new FileScanner().Scan(new CopyOptions { SourcePath = source, DestinationPath = destinationDirectory });

        var item = Assert.Single(manifest);
        Assert.Equal(Path.Combine(destinationDirectory, "source.txt"), item.DestinationPath);
    }

    [Fact]
    public void Scan_WithEmptyDirectory_ReturnsEmptyManifest()
    {
        using var temp = TemporaryWorkspace.Create();
        var sourceRoot = temp.CreateDirectory("source");

        var manifest = new FileScanner().Scan(new CopyOptions { SourcePath = sourceRoot, DestinationPath = temp.PathFor("destination") });

        Assert.Empty(manifest);
    }
}
