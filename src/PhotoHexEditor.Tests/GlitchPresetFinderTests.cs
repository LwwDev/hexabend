using PhotoHexEditor.Core.Formats;

namespace PhotoHexEditor.Tests;

public class GlitchPresetFinderTests
{
    [Fact]
    public void GetPresets_Bmp_ReturnsPixelDataPreset()
    {
        var presets = GlitchPresetFinder.GetPresets(ImageFormatKind.Bmp);

        Assert.Single(presets);
        Assert.Contains("pixel", presets[0].Name, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetPresets_Jpeg_ReturnsScanDataPreset()
    {
        var presets = GlitchPresetFinder.GetPresets(ImageFormatKind.Jpeg);

        Assert.Single(presets);
        Assert.Contains("scan", presets[0].Name, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetPresets_UnknownOrPng_ReturnsNoPresets()
    {
        Assert.Empty(GlitchPresetFinder.GetPresets(ImageFormatKind.Png));
        Assert.Empty(GlitchPresetFinder.GetPresets(ImageFormatKind.Unknown));
    }

    [Fact]
    public void FindTargetRange_Bmp_UsesPixelDataOffsetFromHeader()
    {
        var data = new byte[60];
        data[0] = (byte)'B';
        data[1] = (byte)'M';
        BitConverter.GetBytes((uint)54).CopyTo(data, 10);

        var range = GlitchPresetFinder.FindTargetRange(ImageFormatKind.Bmp, data);

        Assert.Equal((54, 59), range);
    }

    [Fact]
    public void FindTargetRange_Bmp_TruncatedHeader_ReturnsNull()
    {
        Assert.Null(GlitchPresetFinder.FindTargetRange(ImageFormatKind.Bmp, new byte[10]));
    }

    [Fact]
    public void FindTargetRange_Jpeg_FindsFirstScanDataStart()
    {
        byte[] data =
        [
            0xFF, 0xD8, // SOI
            0xFF, 0xDA, 0x00, 0x04, 0xAA, 0xBB, // SOS marker, segment length 4
            0x11, 0x22, 0x33 // scan data
        ];

        var range = GlitchPresetFinder.FindTargetRange(ImageFormatKind.Jpeg, data);

        Assert.Equal((8, data.Length - 1), range);
    }

    [Fact]
    public void FindTargetRange_Jpeg_NoScanMarker_ReturnsNull()
    {
        Assert.Null(GlitchPresetFinder.FindTargetRange(ImageFormatKind.Jpeg, [0xFF, 0xD8, 0x00, 0x00]));
    }

    [Fact]
    public void FindTargetRange_UnknownFormat_ReturnsNull()
    {
        Assert.Null(GlitchPresetFinder.FindTargetRange(ImageFormatKind.Unknown, [0x00]));
    }
}
