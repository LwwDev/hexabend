namespace PhotoHexEditor.Core.Model;

public interface IEditCommand
{
    void Do(ByteBuffer buffer);
    void Undo(ByteBuffer buffer);
}
