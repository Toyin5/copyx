namespace CopyX.Models;

public sealed record CopyOptions
{
    public string SourcePath { get; init; } = string.Empty;

    public string DestinationPath { get; init; } = string.Empty;

    public bool Overwrite { get; init; }

    public bool SkipExisting { get; init; }

    public bool DryRun { get; init; }

    public bool Verbose { get; init; }
}
