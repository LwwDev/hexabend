using PhotoHexEditor.Core.ViewModels;

namespace PhotoHexEditor.Tests;

public class PreviewViewModelTests
{
    // 1x1 pixel, 24bpp, uncompressed BMP: 14-byte file header + 40-byte BITMAPINFOHEADER + 4 bytes pixel data.
    private static byte[] BuildMinimalBmp()
    {
        var bytes = new byte[58];
        bytes[0] = (byte)'B';
        bytes[1] = (byte)'M';
        BitConverter.GetBytes(58).CopyTo(bytes, 2);   // file size
        BitConverter.GetBytes(54).CopyTo(bytes, 10);  // pixel data offset

        BitConverter.GetBytes(40).CopyTo(bytes, 14);  // info header size
        BitConverter.GetBytes(1).CopyTo(bytes, 18);   // width
        BitConverter.GetBytes(1).CopyTo(bytes, 22);   // height
        BitConverter.GetBytes((short)1).CopyTo(bytes, 26);  // planes
        BitConverter.GetBytes((short)24).CopyTo(bytes, 28); // bit count
        // compression, image size, ppm, colors: leave zero

        bytes[54] = 0x00; // B
        bytes[55] = 0x00; // G
        bytes[56] = 0xFF; // R
        bytes[57] = 0x00; // row padding

        return bytes;
    }

    [Fact]
    public void TryDecode_ValidBmp_ReturnsTrueWithImage()
    {
        var succeeded = PreviewViewModel.TryDecode(BuildMinimalBmp(), out var image);

        Assert.True(succeeded);
        Assert.NotNull(image);
        Assert.Equal(1, image!.PixelWidth);
        Assert.Equal(1, image.PixelHeight);
    }

    [Fact]
    public void TryDecode_GarbageBytes_ReturnsFalseWithoutThrowing()
    {
        byte[] garbage = [0x00, 0x01, 0x02, 0x03, 0x04];

        var succeeded = PreviewViewModel.TryDecode(garbage, out var image);

        Assert.False(succeeded);
        Assert.Null(image);
    }

    [Fact]
    public void TryDecode_CorruptedBmpHeader_ReturnsFalseWithoutThrowing()
    {
        var bytes = BuildMinimalBmp();
        BitConverter.GetBytes(int.MaxValue).CopyTo(bytes, 18); // absurd width

        var succeeded = PreviewViewModel.TryDecode(bytes, out var image);

        Assert.False(succeeded);
        Assert.Null(image);
    }

    [Fact]
    public void Status_DefaultsToEmpty()
    {
        var vm = new PreviewViewModel();

        Assert.Equal(DecodeStatus.Empty, vm.Status);
        Assert.False(vm.HasDecodeError);
    }
}
