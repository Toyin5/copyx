namespace CopyX.Models;

public sealed record CopyFailure(string SourcePath, string DestinationPath, string Message);
