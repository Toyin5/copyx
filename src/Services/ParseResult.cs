using CopyX.Models;

namespace CopyX.Services;

public sealed class ParseResult
{
    private ParseResult(bool success, CopyOptions? options, string? error, bool showHelp)
    {
        Success = success;
        Options = options;
        Error = error;
        ShowHelp = showHelp;
    }

    public bool Success { get; }

    public CopyOptions? Options { get; }

    public string? Error { get; }

    public bool ShowHelp { get; }

    public static ParseResult Ok(CopyOptions options) => new(true, options, null, false);

    public static ParseResult Fail(string error) => new(false, null, error, false);

    public static ParseResult Help() => new(true, null, null, true);
}
