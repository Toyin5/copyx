using CopyX.Models;
using CopyX.Utilities;
using Spectre.Console;

namespace CopyX.Services;

public sealed class ProgressReporter : ICopyProgress
{
    private readonly ProgressTask _filesTask;
    private readonly ProgressTask _bytesTask;
    private readonly ProgressTask _currentFileTask;
    private readonly long _totalBytes;
    private long _copiedBytes;

    private ProgressReporter(ProgressContext context, IReadOnlyList<CopyItem> manifest)
    {
        _totalBytes = manifest.Sum(item => item.SizeInBytes);
        _filesTask = context.AddTask("Files", maxValue: Math.Max(manifest.Count, 1));
        _bytesTask = context.AddTask("Data", maxValue: Math.Max(_totalBytes, 1));
        _currentFileTask = context.AddTask("Current file", maxValue: 1);
    }

    public static Task<CopyResult> RunAsync(
        IReadOnlyList<CopyItem> manifest,
        Func<ProgressReporter, Task<CopyResult>> copyAction)
    {
        return AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(
            [
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn()
            ])
            .StartAsync(async context =>
            {
                var reporter = new ProgressReporter(context, manifest);
                return await copyAction(reporter);
            });
    }

    public void Start(IReadOnlyList<CopyItem> manifest)
    {
    }

    public void FileStarted(CopyItem item, int fileNumber)
    {
        _currentFileTask.Description = Path.GetFileName(item.SourcePath);
        _currentFileTask.MaxValue = Math.Max(item.SizeInBytes, 1);
        _currentFileTask.Value = 0;
    }

    public void BytesCopied(long bytesCopied, long currentFileBytes)
    {
        _copiedBytes += bytesCopied;
        _bytesTask.Value = Math.Min(_copiedBytes, Math.Max(_totalBytes, 1));
        _bytesTask.Description = $"Data ({SizeFormatter.FormatBytes(_copiedBytes)} / {SizeFormatter.FormatBytes(_totalBytes)})";
        _currentFileTask.Value = Math.Min(currentFileBytes, _currentFileTask.MaxValue);
    }

    public void FileCompleted(CopyItem item, CopyDisposition disposition)
    {
        _filesTask.Increment(1);
        if (disposition is CopyDisposition.Skipped or CopyDisposition.Planned or CopyDisposition.Failed)
        {
            _currentFileTask.Value = _currentFileTask.MaxValue;
        }
    }
}
