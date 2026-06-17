using CopyX.Utilities;

namespace CopyX.Tests;

public sealed class UtilityTests
{
    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(512, "512 B")]
    [InlineData(1024, "1 KB")]
    [InlineData(1536, "1.5 KB")]
    [InlineData(1048576, "1 MB")]
    public void FormatBytes_ReturnsHumanReadableSize(long bytes, string expected)
    {
        Assert.Equal(expected, SizeFormatter.FormatBytes(bytes));
    }

    [Fact]
    public void FormatSpeed_ReturnsBytesPerSecond()
    {
        Assert.Equal("1 KB/s", SizeFormatter.FormatSpeed(2048, TimeSpan.FromSeconds(2)));
    }

    [Fact]
    public void ResolveSingleFileDestination_WhenDestinationIsFile_ReturnsDestination()
    {
        var destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "out.txt");

        Assert.Equal(destination, PathHelper.ResolveSingleFileDestination("source.txt", destination));
    }
}
