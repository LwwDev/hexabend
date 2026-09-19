using PhotoHexEditor.Core.Model;
using PhotoHexEditor.Core.ViewModels;

namespace PhotoHexEditor.Tests;

public class MainViewModelTests
{
    private static MainViewModel CreateWithDocument(byte[] data)
    {
        var vm = new MainViewModel();
        vm.Document = new ImageDocument("test.bin", data);
        vm.HexGrid.Load(vm.Document.Buffer);
        return vm;
    }

    [Fact]
    public void CommitByteEdit_UpdatesBufferAndGrid()
    {
        var vm = CreateWithDocument([0x00, 0x00]);

        vm.CommitByteEdit(1, 0xFF);

        Assert.Equal(0xFF, vm.Document!.Buffer.ReadByte(1));
        Assert.Equal("FF", vm.HexGrid.Rows[0].HexBytes[1].Text);
    }

    [Fact]
    public void UndoCommand_RevertsLastEdit()
    {
        var vm = CreateWithDocument([0x00, 0x00]);
        vm.CommitByteEdit(1, 0xFF);

        vm.UndoCommand.Execute(null);

        Assert.Equal(0x00, vm.Document!.Buffer.ReadByte(1));
        Assert.False(vm.UndoCommand.CanExecute(null));
        Assert.True(vm.RedoCommand.CanExecute(null));
    }

    [Fact]
    public void RedoCommand_ReappliesUndoneEdit()
    {
        var vm = CreateWithDocument([0x00, 0x00]);
        vm.CommitByteEdit(1, 0xFF);
        vm.UndoCommand.Execute(null);

        vm.RedoCommand.Execute(null);

        Assert.Equal(0xFF, vm.Document!.Buffer.ReadByte(1));
    }

    [Fact]
    public void UndoCommand_CanExecute_FalseWhenNoDocument()
    {
        var vm = new MainViewModel();

        Assert.False(vm.UndoCommand.CanExecute(null));
    }

    [Fact]
    public void CommitByteEdit_SameValue_DoesNotPushHistoryEntry()
    {
        var vm = CreateWithDocument([0x42]);

        vm.CommitByteEdit(0, 0x42);

        Assert.False(vm.UndoCommand.CanExecute(null));
    }
}
