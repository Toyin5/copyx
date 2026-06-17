using System.Diagnostics;

namespace CopyX.Models;

public sealed class CopyResult
{
    private readonly Stopwatch _stopwatch = new();

    public int CopiedFiles { get; private set; }

    public int SkippedFiles { get; private set; }

    public int FailedFiles => Failures.Count;

    public long TotalBytes { get; init; }

    public long CopiedBytes { get; private set; }

    public bool Cancelled { get; private set; }

    public bool DryRun { get; init; }

    public TimeSpan Duration => _stopwatch.Elapsed;

    public List<CopyFailure> Failures { get; } = [];

    public void Start() => _stopwatch.Start();

    public void Stop() => _stopwatch.Stop();

    public void AddCopied(long bytes)
    {
        CopiedFiles++;
        CopiedBytes += bytes;
    }

    public void AddSkipped() => SkippedFiles++;

    public void AddFailure(CopyFailure failure) => Failures.Add(failure);

    public void MarkCancelled() => Cancelled = true;
}
