using CopyX.Models;

namespace CopyX.Services;

public static class ArgumentParser
{
    private static readonly HashSet<string> KnownFlags = new(StringComparer.OrdinalIgnoreCase)
    {
        "--overwrite",
        "--skip-existing",
        "--dry-run",
        "--verbose",
        "--help",
        "-h"
    };

    public static ParseResult Parse(IReadOnlyList<string> args)
    {
        if (args.Count is 1 && IsHelp(args[0]))
        {
            return ParseResult.Help();
        }

        var positional = new List<string>();
        var overwrite = false;
        var skipExisting = false;
        var dryRun = false;
        var verbose = false;

        foreach (var arg in args)
        {
            if (arg.StartsWith('-'))
            {
                if (!KnownFlags.Contains(arg))
                {
                    return ParseResult.Fail($"Unknown option: {arg}");
                }

                overwrite |= arg.Equals("--overwrite", StringComparison.OrdinalIgnoreCase);
                skipExisting |= arg.Equals("--skip-existing", StringComparison.OrdinalIgnoreCase);
                dryRun |= arg.Equals("--dry-run", StringComparison.OrdinalIgnoreCase);
                verbose |= arg.Equals("--verbose", StringComparison.OrdinalIgnoreCase);

                if (IsHelp(arg))
                {
                    return ParseResult.Help();
                }
            }
            else
            {
                positional.Add(arg);
            }
        }

        if (positional.Count != 2)
        {
            return ParseResult.Fail("Provide exactly one source and one destination.");
        }

        if (overwrite && skipExisting)
        {
            return ParseResult.Fail("--overwrite and --skip-existing cannot be used together.");
        }

        return ParseResult.Ok(new CopyOptions
        {
            SourcePath = positional[0],
            DestinationPath = positional[1],
            Overwrite = overwrite,
            SkipExisting = skipExisting,
            DryRun = dryRun,
            Verbose = verbose
        });
    }

    private static bool IsHelp(string arg) =>
        arg.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
        arg.Equals("-h", StringComparison.OrdinalIgnoreCase);
}
