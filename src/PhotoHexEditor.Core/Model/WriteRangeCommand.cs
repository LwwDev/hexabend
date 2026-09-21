namespace PhotoHexEditor.Core.Model;

public sealed class WriteRangeCommand(int start, byte[] oldBytes, byte[] newBytes) : IEditCommand
{
    public int Start { get; } = start;

    public void Do(ByteBuffer buffer) => buffer.WriteRange(Start, newBytes);

    public void Undo(ByteBuffer buffer) => buffer.WriteRange(Start, oldBytes);
}
