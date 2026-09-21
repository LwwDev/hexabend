using System.IO;
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

    [Fact]
    public void SuggestedSaveFileName_AppendsGlitchedSuffixBeforeExtension()
    {
        var vm = new MainViewModel { Document = new ImageDocument(@"C:\photos\cat.bmp", [0x00]) };

        Assert.EndsWith("cat_glitched.bmp", vm.SuggestedSaveFileName);
    }

    [Fact]
    public void SaveAs_WritesCurrentBufferToDisk_AndClearsDirtyFlag()
    {
        var tempSource = Path.GetTempFileName();
        var tempTarget = Path.GetTempFileName();
        try
        {
            var vm = CreateWithDocument([0x00, 0x00]);
            vm.CommitByteEdit(0, 0xAB);

            vm.SaveAs(tempTarget);

            Assert.Equal([0xAB, 0x00], File.ReadAllBytes(tempTarget));
            Assert.False(vm.Document!.IsDirty);
        }
        finally
        {
            File.Delete(tempSource);
            File.Delete(tempTarget);
        }
    }

    [Fact]
    public void SaveAs_DoesNotTouchOriginalSourceFile()
    {
        var tempSource = Path.GetTempFileName();
        var tempTarget = tempSource + "_glitched";
        try
        {
            File.WriteAllBytes(tempSource, [0x11, 0x22]);
            var vm = new MainViewModel();
            vm.LoadCommand.Execute(tempSource);

            vm.CommitByteEdit(0, 0x99);
            vm.SaveAs(tempTarget);

            Assert.Equal([0x11, 0x22], File.ReadAllBytes(tempSource));
            Assert.Equal([0x99, 0x22], File.ReadAllBytes(tempTarget));
        }
        finally
        {
            File.Delete(tempSource);
            if (File.Exists(tempTarget)) File.Delete(tempTarget);
        }
    }

    [Fact]
    public void FindNext_CyclesThroughMatchesAndWraps()
    {
        var vm = CreateWithDocument([0x41, 0x00, 0x41, 0x00, 0x41]);

        var first = vm.FindNext("41", isHex: true);
        var second = vm.FindNext("41", isHex: true);
        var third = vm.FindNext("41", isHex: true);
        var fourth = vm.FindNext("41", isHex: true);

        Assert.Equal(0, first);
        Assert.Equal(2, second);
        Assert.Equal(4, third);
        Assert.Equal(0, fourth);
    }

    [Fact]
    public void FindNext_NoMatches_ReturnsNull()
    {
        var vm = CreateWithDocument([0x00]);

        Assert.Null(vm.FindNext("FF", isHex: true));
    }

    [Fact]
    public void FindNext_AsciiQuery_FindsSubstring()
    {
        var vm = CreateWithDocument("xxABCxx"u8.ToArray());

        Assert.Equal(2, vm.FindNext("ABC", isHex: false));
    }

    [Fact]
    public void GoToOffset_ValidOffset_ReturnsTrue()
    {
        var vm = CreateWithDocument([0x00, 0x00, 0x00]);

        Assert.True(vm.GoToOffset(1));
    }

    [Fact]
    public void GoToOffset_OutOfRange_ReturnsFalse()
    {
        var vm = CreateWithDocument([0x00]);

        Assert.False(vm.GoToOffset(5));
    }
}
