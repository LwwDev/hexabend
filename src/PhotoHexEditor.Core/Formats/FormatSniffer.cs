namespace PhotoHexEditor.Core.Formats;

public static class FormatSniffer
{
    public static ImageFormatKind Detect(byte[] data)
    {
        if (data.Length >= 2 && data[0] == (byte)'B' && data[1] == (byte)'M')
            return ImageFormatKind.Bmp;

        if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
            return ImageFormatKind.Jpeg;

        if (data.Length >= 8
            && data[0] == 0x89 && data[1] == (byte)'P' && data[2] == (byte)'N' && data[3] == (byte)'G'
            && data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A)
            return ImageFormatKind.Png;

        return ImageFormatKind.Unknown;
    }
}
