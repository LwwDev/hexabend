namespace PhotoHexEditor.Core.Formats;

/// <summary>
/// Locates the byte range worth corrupting for a given format — the parts that glitch
/// without breaking the file outright (pixel data, scan data), as opposed to headers
/// that usually just make the file fail to decode.
/// </summary>
public static class GlitchPresetFinder
{
    public static IReadOnlyList<GlitchPreset> GetPresets(ImageFormatKind format) => format switch
    {
        ImageFormatKind.Bmp => [new GlitchPreset("Noise pixel data", "Randomizes bytes after the BMP pixel array offset")],
        ImageFormatKind.Jpeg => [new GlitchPreset("Noise scan data", "Randomizes bytes after the first JPEG scan header")],
        _ => []
    };

    public static (int Start, int End)? FindTargetRange(ImageFormatKind format, byte[] data) => format switch
    {
        ImageFormatKind.Bmp => FindBmpPixelRange(data),
        ImageFormatKind.Jpeg => FindJpegScanRange(data),
        _ => null
    };

    private static (int Start, int End)? FindBmpPixelRange(byte[] data)
    {
        // Bytes 10-13 of the BMP file header (bfOffBits) give the little-endian offset
        // where pixel data starts, regardless of DIB header variant or color table size.
        if (data.Length < 14) return null;

        var pixelOffset = BitConverter.ToUInt32(data, 10);
        if (pixelOffset >= data.Length) return null;

        return ((int)pixelOffset, data.Length - 1);
    }

    private static (int Start, int End)? FindJpegScanRange(byte[] data)
    {
        for (var i = 0; i < data.Length - 3; i++)
        {
            if (data[i] != 0xFF || data[i + 1] != 0xDA) continue;

            // SOS (Start Of Scan) marker: 2-byte big-endian segment length follows,
            // counting itself but not the marker; scan data begins right after it.
            var segmentLength = (data[i + 2] << 8) | data[i + 3];
            var scanStart = i + 2 + segmentLength;
            if (scanStart >= data.Length) return null;

            return (scanStart, data.Length - 1);
        }

        return null;
    }
}
