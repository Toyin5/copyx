using CopyX.Models;
using CopyX.Services;

namespace CopyX.Tests;

public sealed class CopyServiceTests
{
    [Fact]
    public async Task CopyAsync_CopiesSingleFile()
    {
        using var temp = TemporaryWorkspace.Create();
        var source = temp.WriteFile("source.txt", "hello");
        var destination = temp.PathFor("dest.txt");
        var manifest = new FileScanner().Scan(new CopyOptions { SourcePath = source, DestinationPath = destination });

        var result = await new CopyService().CopyAsync(manifest, Options(source, destination), new NoopCopyProgress(), CancellationToken.None);

        Assert.Equal(1, result.CopiedFiles);
        Assert.Equal("hello", File.ReadAllText(destination));
    }

    [Fact]
    public async Task CopyAsync_CopiesDirectoryTree()
    {
        using var temp = TemporaryWorkspace.Create();
        var sourceRoot = temp.CreateDirectory("source");
        Directory.CreateDirectory(Path.Combine(sourceRoot, "nested"));
        File.WriteAllText(Path.Combine(sourceRoot, "nested", "file.txt"), "payload");
        var destinationRoot = temp.PathFor("destination");
        var manifest = new FileScanner().Scan(new CopyOptions { SourcePath = sourceRoot, DestinationPath = destinationRoot });

        var result = await new CopyService().CopyAsync(manifest, Options(sourceRoot, destinationRoot), new NoopCopyProgress(), CancellationToken.None);

        Assert.Equal(1, result.CopiedFiles);
        Assert.Equal("payload", File.ReadAllText(Path.Combine(destinationRoot, "nested", "file.txt")));
    }

    [Fact]
    public async Task CopyAsync_WhenDestinationExistsWithoutFlag_RecordsFailure()
    {
        using var temp = TemporaryWorkspace.Create();
        var source = temp.WriteFile("source.txt", "new");
        var destination = temp.WriteFile("dest.txt", "old");
        var manifest = new FileScanner().Scan(new CopyOptions { SourcePath = source, DestinationPath = destination });

        var result = await new CopyService().CopyAsync(manifest, Options(source, destination), new NoopCopyProgress(), CancellationToken.None);

        Assert.Equal(1, result.FailedFiles);
        Assert.Equal("old", File.ReadAllText(destination));
    }

    [Fact]
    public async Task CopyAsync_WithOverwrite_ReplacesExistingFile()
    {
        using var temp = TemporaryWorkspace.Create();
        var source = temp.WriteFile("source.txt", "new");
        var destination = temp.WriteFile("dest.txt", "old");
        var manifest = new FileScanner().Scan(new CopyOptions { SourcePath = source, DestinationPath = destination });

        var result = await new CopyService().CopyAsync(
            manifest,
            Options(source, destination) with { Overwrite = true },
            new NoopCopyProgress(),
            CancellationToken.None);

        Assert.Equal(1, result.CopiedFiles);
        Assert.Equal("new", File.ReadAllText(destination));
    }

    [Fact]
    public async Task CopyAsync_WithSkipExisting_LeavesExistingFile()
    {
        using var temp = TemporaryWorkspace.Create();
        var source = temp.WriteFile("source.txt", "new");
        var destination = temp.WriteFile("dest.txt", "old");
        var manifest = new FileScanner().Scan(new CopyOptions { SourcePath = source, DestinationPath = destination });

        var result = await new CopyService().CopyAsync(
            manifest,
            Options(source, destination) with { SkipExisting = true },
            new NoopCopyProgress(),
            CancellationToken.None);

        Assert.Equal(1, result.SkippedFiles);
        Assert.Equal("old", File.ReadAllText(destination));
    }

    [Fact]
    public async Task CopyAsync_WithDryRun_WritesNothing()
    {
        using var temp = TemporaryWorkspace.Create();
        var source = temp.WriteFile("source.txt", "hello");
        var destination = temp.PathFor("dest.txt");
        var manifest = new FileScanner().Scan(new CopyOptions { SourcePath = source, DestinationPath = destination });

        var result = await new CopyService().CopyAsync(
            manifest,
            Options(source, destination) with { DryRun = true },
            new NoopCopyProgress(),
            CancellationToken.None);

        Assert.Equal(1, result.SkippedFiles);
        Assert.False(File.Exists(destination));
    }

    [Fact]
    public async Task CopyAsync_ContinuesAfterAFileFailure()
    {
        using var temp = TemporaryWorkspace.Create();
        var sourceRoot = temp.CreateDirectory("source");
        File.WriteAllText(Path.Combine(sourceRoot, "a.txt"), "a");
        File.WriteAllText(Path.Combine(sourceRoot, "b.txt"), "b");
        var destinationRoot = temp.CreateDirectory("destination");
        File.WriteAllText(Path.Combine(destinationRoot, "a.txt"), "existing");
        var manifest = new FileScanner().Scan(new CopyOptions { SourcePath = sourceRoot, DestinationPath = destinationRoot });

        var result = await new CopyService().CopyAsync(manifest, Options(sourceRoot, destinationRoot), new NoopCopyProgress(), CancellationToken.None);

        Assert.Equal(1, result.FailedFiles);
        Assert.Equal(1, result.CopiedFiles);
        Assert.Equal("b", File.ReadAllText(Path.Combine(destinationRoot, "b.txt")));
    }

    private static CopyOptions Options(string source, string destination) => new()
    {
        SourcePath = source,
        DestinationPath = destination
    };
}
