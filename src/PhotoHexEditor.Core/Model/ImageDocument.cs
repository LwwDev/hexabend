using System.IO;
using PhotoHexEditor.Core.Formats;

namespace PhotoHexEditor.Core.Model;

public sealed class ImageDocument
{
    public ByteBuffer Buffer { get; }
    public string FilePath { get; }
    public ImageFormatKind Format { get; }
    public EditHistory History { get; } = new();
    public bool IsDirty { get; private set; }

    public ImageDocument(string filePath, byte[] data)
    {
        FilePath = filePath;
        Buffer = new ByteBuffer(data);
        Format = FormatSniffer.Detect(data);
        Buffer.Changed += (_, _) => IsDirty = true;
    }

    public static ImageDocument Load(string filePath)
    {
        var data = File.ReadAllBytes(filePath);
        return new ImageDocument(filePath, data);
    }
}
