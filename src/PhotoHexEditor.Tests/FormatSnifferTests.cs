using PhotoHexEditor.Core.Formats;

namespace PhotoHexEditor.Tests;

public class FormatSnifferTests
{
    [Fact]
    public void Detect_BmpMagicBytes_ReturnsBmp()
    {
        byte[] data = [(byte)'B', (byte)'M', 0x00, 0x00];

        Assert.Equal(ImageFormatKind.Bmp, FormatSniffer.Detect(data));
    }

    [Fact]
    public void Detect_JpegMagicBytes_ReturnsJpeg()
    {
        byte[] data = [0xFF, 0xD8, 0xFF, 0xE0];

        Assert.Equal(ImageFormatKind.Jpeg, FormatSniffer.Detect(data));
    }

    [Fact]
    public void Detect_PngMagicBytes_ReturnsPng()
    {
        byte[] data = [0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A];

        Assert.Equal(ImageFormatKind.Png, FormatSniffer.Detect(data));
    }

    [Fact]
    public void Detect_UnrecognizedBytes_ReturnsUnknown()
    {
        byte[] data = [0x00, 0x01, 0x02, 0x03];

        Assert.Equal(ImageFormatKind.Unknown, FormatSniffer.Detect(data));
    }

    [Fact]
    public void Detect_EmptyData_ReturnsUnknown()
    {
        Assert.Equal(ImageFormatKind.Unknown, FormatSniffer.Detect([]));
    }
}
