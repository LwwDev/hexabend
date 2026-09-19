using PhotoHexEditor.Core.Model;

namespace PhotoHexEditor.Core.ViewModels;

public sealed class HexRowViewModel
{
    public const int BytesPerRow = 16;

    public int Offset { get; }
    public string OffsetText { get; }
    public IReadOnlyList<string> HexBytes { get; }
    public string AsciiText { get; }

    public HexRowViewModel(int offset, ByteBuffer buffer)
    {
        Offset = offset;
        OffsetText = offset.ToString("X8");

        var rowLength = Math.Min(BytesPerRow, buffer.Length - offset);
        var hex = new string[BytesPerRow];
        var ascii = new char[rowLength];

        for (var i = 0; i < BytesPerRow; i++)
        {
            if (i < rowLength)
            {
                var b = buffer.ReadByte(offset + i);
                hex[i] = b.ToString("X2");
                ascii[i] = b is >= 0x20 and < 0x7F ? (char)b : '.';
            }
            else
            {
                hex[i] = string.Empty;
            }
        }

        HexBytes = hex;
        AsciiText = new string(ascii);
    }
}
