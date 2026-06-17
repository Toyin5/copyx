namespace CopyX.Models;

public sealed class CopyItem
{
    public string SourcePath { get; set; } = string.Empty;

    public string DestinationPath { get; set; } = string.Empty;

    public long SizeInBytes { get; set; }
}
