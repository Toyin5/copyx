using CopyX.Services;

namespace CopyX.Tests;

public sealed class ArgumentParserTests
{
    [Fact]
    public void Parse_WithSourceAndDestination_ReturnsOptions()
    {
        var result = ArgumentParser.Parse(["source.txt", "dest.txt"]);

        Assert.True(result.Success);
        Assert.Equal("source.txt", result.Options!.SourcePath);
        Assert.Equal("dest.txt", result.Options.DestinationPath);
    }

    [Fact]
    public void Parse_WithFlags_ReturnsEnabledOptions()
    {
        var result = ArgumentParser.Parse(["source", "dest", "--overwrite", "--dry-run", "--verbose"]);

        Assert.True(result.Success);
        Assert.True(result.Options!.Overwrite);
        Assert.True(result.Options.DryRun);
        Assert.True(result.Options.Verbose);
    }

    [Fact]
    public void Parse_WithMissingDestination_Fails()
    {
        var result = ArgumentParser.Parse(["source"]);

        Assert.False(result.Success);
        Assert.Contains("exactly one source", result.Error);
    }

    [Fact]
    public void Parse_WithConflictingExistingFileFlags_Fails()
    {
        var result = ArgumentParser.Parse(["source", "dest", "--overwrite", "--skip-existing"]);

        Assert.False(result.Success);
        Assert.Contains("cannot be used together", result.Error);
    }
}
