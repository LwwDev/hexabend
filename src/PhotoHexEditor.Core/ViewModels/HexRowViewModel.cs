using PhotoHexEditor.Core.Model;

namespace PhotoHexEditor.Core.ViewModels;

public sealed class HexRowViewModel
{
    public const int BytesPerRow = 16;

    public int Offset { get; }
    public string OffsetText { get; }
    public IReadOnlyList<HexByteCellViewModel> HexBytes { get; }
    public string AsciiText { get; }

    public HexRowViewModel(int offset, ByteBuffer buffer, int? highlightOffset = null, (int Start, int End)? selection = null)
    {
        Offset = offset;
        OffsetText = offset.ToString("X8");

        var rowLength = Math.Min(BytesPerRow, buffer.Length - offset);
        var hex = new HexByteCellViewModel[BytesPerRow];
        var ascii = new char[rowLength];

        for (var i = 0; i < BytesPerRow; i++)
        {
            var byteOffset = offset + i;
            if (i < rowLength)
            {
                var b = buffer.ReadByte(byteOffset);
                var isHighlighted = highlightOffset == byteOffset;
                var isSelected = selection is not null && byteOffset >= selection.Value.Start && byteOffset <= selection.Value.End;
                hex[i] = new HexByteCellViewModel(byteOffset, b.ToString("X2"), isValid: true, isHighlighted, isSelected);
                ascii[i] = b is >= 0x20 and < 0x7F ? (char)b : '.';
            }
            else
            {
                hex[i] = new HexByteCellViewModel(byteOffset, string.Empty, isValid: false);
            }
        }

        HexBytes = hex;
        AsciiText = new string(ascii);
    }
}
