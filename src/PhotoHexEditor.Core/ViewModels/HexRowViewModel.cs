using PhotoHexEditor.Core.Model;

namespace PhotoHexEditor.Core.ViewModels;

public sealed class HexRowViewModel
{
    public const int BytesPerRow = 16;

    public int Offset { get; }
    public string OffsetText { get; }
    public IReadOnlyList<HexByteCellViewModel> HexBytes { get; }
    public string AsciiText { get; }

    public HexRowViewModel(int offset, ByteBuffer buffer)
    {
        Offset = offset;
        OffsetText = offset.ToString("X8");

        var rowLength = Math.Min(BytesPerRow, buffer.Length - offset);
        var hex = new HexByteCellViewModel[BytesPerRow];
        var ascii = new char[rowLength];

        for (var i = 0; i < BytesPerRow; i++)
        {
            if (i < rowLength)
            {
                var b = buffer.ReadByte(offset + i);
                hex[i] = new HexByteCellViewModel(offset + i, b.ToString("X2"), isValid: true);
                ascii[i] = b is >= 0x20 and < 0x7F ? (char)b : '.';
            }
            else
            {
                hex[i] = new HexByteCellViewModel(offset + i, string.Empty, isValid: false);
            }
        }

        HexBytes = hex;
        AsciiText = new string(ascii);
    }
}
