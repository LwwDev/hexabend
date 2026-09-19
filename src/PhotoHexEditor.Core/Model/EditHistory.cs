namespace PhotoHexEditor.Core.Model;

public sealed class EditHistory
{
    private readonly Stack<IEditCommand> _undo = new();
    private readonly Stack<IEditCommand> _redo = new();

    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;

    public event EventHandler? Changed;

    public void Execute(IEditCommand command, ByteBuffer buffer)
    {
        command.Do(buffer);
        _undo.Push(command);
        _redo.Clear();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Undo(ByteBuffer buffer)
    {
        if (_undo.Count == 0) return;

        var command = _undo.Pop();
        command.Undo(buffer);
        _redo.Push(command);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Redo(ByteBuffer buffer)
    {
        if (_redo.Count == 0) return;

        var command = _redo.Pop();
        command.Do(buffer);
        _undo.Push(command);
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
