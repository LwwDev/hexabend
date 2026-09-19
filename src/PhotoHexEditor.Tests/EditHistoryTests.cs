using PhotoHexEditor.Core.Model;

namespace PhotoHexEditor.Tests;

public class WriteByteCommandTests
{
    [Fact]
    public void Do_WritesNewValueAtOffset()
    {
        var buffer = new ByteBuffer([0x00, 0x00]);
        var command = new WriteByteCommand(1, oldValue: 0x00, newValue: 0xFF);

        command.Do(buffer);

        Assert.Equal(0xFF, buffer.ReadByte(1));
    }

    [Fact]
    public void Undo_RestoresOldValueAtOffset()
    {
        var buffer = new ByteBuffer([0x00, 0xFF]);
        var command = new WriteByteCommand(1, oldValue: 0x00, newValue: 0xFF);

        command.Undo(buffer);

        Assert.Equal(0x00, buffer.ReadByte(1));
    }
}

public class EditHistoryTests
{
    [Fact]
    public void Execute_AppliesCommandAndEnablesUndo()
    {
        var buffer = new ByteBuffer([0x00]);
        var history = new EditHistory();

        history.Execute(new WriteByteCommand(0, 0x00, 0xAB), buffer);

        Assert.Equal(0xAB, buffer.ReadByte(0));
        Assert.True(history.CanUndo);
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void Undo_RevertsLastCommandAndEnablesRedo()
    {
        var buffer = new ByteBuffer([0x00]);
        var history = new EditHistory();
        history.Execute(new WriteByteCommand(0, 0x00, 0xAB), buffer);

        history.Undo(buffer);

        Assert.Equal(0x00, buffer.ReadByte(0));
        Assert.False(history.CanUndo);
        Assert.True(history.CanRedo);
    }

    [Fact]
    public void Redo_ReappliesUndoneCommand()
    {
        var buffer = new ByteBuffer([0x00]);
        var history = new EditHistory();
        history.Execute(new WriteByteCommand(0, 0x00, 0xAB), buffer);
        history.Undo(buffer);

        history.Redo(buffer);

        Assert.Equal(0xAB, buffer.ReadByte(0));
        Assert.True(history.CanUndo);
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void Execute_AfterUndo_ClearsRedoStack()
    {
        var buffer = new ByteBuffer([0x00]);
        var history = new EditHistory();
        history.Execute(new WriteByteCommand(0, 0x00, 0xAB), buffer);
        history.Undo(buffer);

        history.Execute(new WriteByteCommand(0, 0x00, 0xCD), buffer);

        Assert.Equal(0xCD, buffer.ReadByte(0));
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void Undo_WithEmptyHistory_IsNoOp()
    {
        var buffer = new ByteBuffer([0x42]);
        var history = new EditHistory();

        history.Undo(buffer);

        Assert.Equal(0x42, buffer.ReadByte(0));
        Assert.False(history.CanUndo);
    }

    [Fact]
    public void Redo_WithEmptyRedoStack_IsNoOp()
    {
        var buffer = new ByteBuffer([0x42]);
        var history = new EditHistory();

        history.Redo(buffer);

        Assert.Equal(0x42, buffer.ReadByte(0));
        Assert.False(history.CanRedo);
    }
}
