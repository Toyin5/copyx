using CopyX.Models;
using Spectre.Console;

namespace CopyX.Services;

public sealed class CopyService
{
    private const int BufferSize = 1024 * 1024;

    public async Task<CopyResult> CopyAsync(
        IReadOnlyList<CopyItem> manifest,
        CopyOptions options,
        ICopyProgress progress,
        CancellationToken cancellationToken)
    {
        var result = new CopyResult
        {
            TotalBytes = manifest.Sum(item => item.SizeInBytes),
            DryRun = options.DryRun
        };

        result.Start();
        progress.Start(manifest);

        try
        {
            for (var index = 0; index < manifest.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var item = manifest[index];
                progress.FileStarted(item, index + 1);

                try
                {
                    if (File.Exists(item.DestinationPath))
                    {
                        if (options.SkipExisting)
                        {
                            result.AddSkipped();
                            progress.FileCompleted(item, CopyDisposition.Skipped);
                            WriteVerbose(options, $"Skipped: {item.DestinationPath}");
                            continue;
                        }

                        if (!options.Overwrite)
                        {
                            throw new IOException("Destination file already exists.");
                        }
                    }

                    if (options.DryRun)
                    {
                        result.AddSkipped();
                        progress.FileCompleted(item, CopyDisposition.Planned);
                        WriteVerbose(options, $"Would copy: {item.SourcePath} -> {item.DestinationPath}");
                        continue;
                    }

                    var destinationDirectory = Path.GetDirectoryName(item.DestinationPath);
                    if (!string.IsNullOrWhiteSpace(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }

                    await CopyFileAsync(item, options.Overwrite, progress, cancellationToken);
                    result.AddCopied(item.SizeInBytes);
                    progress.FileCompleted(item, CopyDisposition.Copied);
                    WriteVerbose(options, $"Copied: {item.SourcePath} -> {item.DestinationPath}");
                }
                catch (OperationCanceledException)
                {
                    result.MarkCancelled();
                    throw;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or PathTooLongException)
                {
                    result.AddFailure(new CopyFailure(item.SourcePath, item.DestinationPath, ex.Message));
                    progress.FileCompleted(item, CopyDisposition.Failed);
                    WriteVerbose(options, $"Failed: {item.SourcePath} -> {item.DestinationPath} ({ex.Message})");
                }
            }
        }
        catch (OperationCanceledException)
        {
            result.MarkCancelled();
        }
        finally
        {
            result.Stop();
        }

        return result;
    }

    private static async Task CopyFileAsync(
        CopyItem item,
        bool overwrite,
        ICopyProgress progress,
        CancellationToken cancellationToken)
    {
        var mode = overwrite ? FileMode.Create : FileMode.CreateNew;
        await using var source = new FileStream(
            item.SourcePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        await using var destination = new FileStream(
            item.DestinationPath,
            mode,
            FileAccess.Write,
            FileShare.None,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        var buffer = new byte[BufferSize];
        long currentFileBytes = 0;
        int bytesRead;
        while ((bytesRead = await source.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            currentFileBytes += bytesRead;
            progress.BytesCopied(bytesRead, currentFileBytes);
        }
    }

    private static void WriteVerbose(CopyOptions options, string message)
    {
        if (options.Verbose)
        {
            AnsiConsole.WriteLine(message);
        }
    }
}
