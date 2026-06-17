namespace CopyX.Utilities;

public static class SizeFormatter
{
    private static readonly string[] Units = ["B", "KB", "MB", "GB", "TB"];

    public static string FormatBytes(long bytes)
    {
        if (bytes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bytes), "Bytes cannot be negative.");
        }

        double value = bytes;
        var unitIndex = 0;
        while (value >= 1024 && unitIndex < Units.Length - 1)
        {
            value /= 1024;
            unitIndex++;
        }

        return unitIndex == 0 ? $"{bytes} {Units[unitIndex]}" : $"{value:0.##} {Units[unitIndex]}";
    }

    public static string FormatSpeed(long bytes, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            return "0 B/s";
        }

        return $"{FormatBytes((long)(bytes / duration.TotalSeconds))}/s";
    }
}
