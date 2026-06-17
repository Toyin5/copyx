using CopyX.Models;

namespace CopyX.Services;

public sealed class NoopCopyProgress : ICopyProgress
{
    public void Start(IReadOnlyList<CopyItem> manifest)
    {
    }

    public void FileStarted(CopyItem item, int fileNumber)
    {
    }

    public void BytesCopied(long bytesCopied, long currentFileBytes)
    {
    }

    public void FileCompleted(CopyItem item, CopyDisposition disposition)
    {
    }
}
