namespace PhotoHexEditor.Core.Model;

public sealed class WriteByteCommand(int offset, byte oldValue, byte newValue) : IEditCommand
{
    public int Offset { get; } = offset;

    public void Do(ByteBuffer buffer) => buffer.WriteByte(Offset, newValue);

    public void Undo(ByteBuffer buffer) => buffer.WriteByte(Offset, oldValue);
}
