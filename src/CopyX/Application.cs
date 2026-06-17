using CopyX.Models;
using CopyX.Services;
using CopyX.Utilities;
using Spectre.Console;

namespace CopyX;

public sealed class Application
{
    private readonly FileScanner _scanner;
    private readonly CopyService _copyService;

    public Application()
        : this(new FileScanner(), new CopyService())
    {
    }

    public Application(FileScanner scanner, CopyService copyService)
    {
        _scanner = scanner;
        _copyService = copyService;
    }

    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var parsed = ArgumentParser.Parse(args);
        if (!parsed.Success)
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(parsed.Error ?? "Invalid arguments.")}[/]");
            WriteUsage();
            return 1;
        }

        if (parsed.ShowHelp)
        {
            WriteUsage();
            return 0;
        }

        var options = parsed.Options!;
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            if (!linkedCts.IsCancellationRequested)
            {
                AnsiConsole.MarkupLine("[yellow]Cancelling...[/]");
                linkedCts.Cancel();
            }
        };

        try
        {
            var manifest = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("Scanning directory...", _ => Task.FromResult(_scanner.Scan(options)));

            WriteScanSummary(manifest);

            var result = options.DryRun
                ? await _copyService.CopyAsync(manifest, options, new NoopCopyProgress(), linkedCts.Token)
                : await ProgressReporter.RunAsync(manifest, reporter =>
                    _copyService.CopyAsync(manifest, options, reporter, linkedCts.Token));

            WriteSummary(result);
            return result.Cancelled ? 130 : result.FailedFiles > 0 ? 2 : 0;
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("[yellow]Operation stopped safely.[/]");
            return 130;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]");
            return 1;
        }
    }

    private static void WriteUsage()
    {
        AnsiConsole.WriteLine("Usage:");
        AnsiConsole.WriteLine("  copyx <source> <destination> [--overwrite|--skip-existing] [--dry-run] [--verbose]");
    }

    private static void WriteScanSummary(IReadOnlyList<CopyItem> manifest)
    {
        var totalBytes = manifest.Sum(item => item.SizeInBytes);
        AnsiConsole.MarkupLine($"Files Found: [green]{manifest.Count}[/]");
        AnsiConsole.MarkupLine($"Total Size : [green]{SizeFormatter.FormatBytes(totalBytes)}[/]");
    }

    private static void WriteSummary(CopyResult result)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title(result.Cancelled ? "Copy Cancelled" : result.DryRun ? "Dry Run Completed" : "Copy Completed")
            .AddColumn("Metric")
            .AddColumn("Value");

        table.AddRow("Files Copied", result.CopiedFiles.ToString());
        table.AddRow("Files Skipped", result.SkippedFiles.ToString());
        table.AddRow("Files Failed", result.FailedFiles.ToString());
        table.AddRow("Total Size", SizeFormatter.FormatBytes(result.TotalBytes));
        table.AddRow("Duration", result.Duration.ToString(@"hh\:mm\:ss"));
        table.AddRow("Average Speed", SizeFormatter.FormatSpeed(result.CopiedBytes, result.Duration));

        AnsiConsole.Write(table);

        foreach (var failure in result.Failures)
        {
            AnsiConsole.MarkupLine($"[red]Failed:[/] {Markup.Escape(failure.SourcePath)} - {Markup.Escape(failure.Message)}");
        }
    }
}
