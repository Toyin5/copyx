using CopyX.Models;

namespace CopyX.Services;

public enum CopyDisposition
{
    Copied,
    Skipped,
    Planned,
    Failed
}

public interface ICopyProgress
{
    void Start(IReadOnlyList<CopyItem> manifest);

    void FileStarted(CopyItem item, int fileNumber);

    void BytesCopied(long bytesCopied, long currentFileBytes);

    void FileCompleted(CopyItem item, CopyDisposition disposition);
}
