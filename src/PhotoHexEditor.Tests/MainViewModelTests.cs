using System.IO;
using System.Linq;
using PhotoHexEditor.Core.Model;
using PhotoHexEditor.Core.ViewModels;
using PhotoHexEditor.Core.Formats;

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

    [Fact]
    public void SetSelection_NormalizesOrder()
    {
        var vm = CreateWithDocument(new byte[10]);

        var result = vm.SetSelection(5, 2);

        Assert.True(result);
        Assert.Equal(2, vm.SelectionStart);
        Assert.Equal(5, vm.SelectionEnd);
    }

    [Fact]
    public void SetSelection_OutOfBounds_ReturnsFalse()
    {
        var vm = CreateWithDocument(new byte[4]);

        Assert.False(vm.SetSelection(0, 10));
    }

    [Fact]
    public void FillSelection_WritesValueAcrossRangeAndSupportsUndo()
    {
        var vm = CreateWithDocument(new byte[4]);
        vm.SetSelection(1, 2);

        var applied = vm.FillSelection(0xFF);

        Assert.True(applied);
        Assert.Equal([0x00, 0xFF, 0xFF, 0x00], vm.Document!.Buffer.ToArray());

        vm.UndoCommand.Execute(null);
        Assert.Equal([0x00, 0x00, 0x00, 0x00], vm.Document!.Buffer.ToArray());
    }

    [Fact]
    public void FillSelection_NoSelection_ReturnsFalse()
    {
        var vm = CreateWithDocument(new byte[4]);

        Assert.False(vm.FillSelection(0xFF));
    }

    [Fact]
    public void InvertSelection_FlipsBitsInRange()
    {
        var vm = CreateWithDocument([0x00, 0xFF]);
        vm.SetSelection(0, 1);

        vm.InvertSelection();

        Assert.Equal([0xFF, 0x00], vm.Document!.Buffer.ToArray());
    }

    [Fact]
    public void ShiftSelection_AddsDeltaModulo256()
    {
        var vm = CreateWithDocument([0xFE, 0x00]);
        vm.SetSelection(0, 1);

        vm.ShiftSelection(3);

        Assert.Equal([0x01, 0x03], vm.Document!.Buffer.ToArray());
    }

    [Fact]
    public void RandomizeSelection_WithSeededRandom_WritesBytes()
    {
        var vm = CreateWithDocument(new byte[4]);
        vm.SetSelection(0, 3);

        vm.RandomizeSelection(new Random(1));

        Assert.NotEqual(new byte[4], vm.Document!.Buffer.ToArray());
    }

    [Fact]
    public void AvailablePresets_Bmp_ReturnsPixelDataPreset()
    {
        var vm = CreateWithDocument([(byte)'B', (byte)'M']);

        Assert.Single(vm.AvailablePresets);
    }

    [Fact]
    public void AvailablePresets_NoDocument_ReturnsEmpty()
    {
        var vm = new MainViewModel();

        Assert.Empty(vm.AvailablePresets);
    }

    [Fact]
    public void ApplyGlitchPreset_Bmp_RandomizesPixelDataRangeAndSupportsUndo()
    {
        var header = new byte[54];
        header[0] = (byte)'B';
        header[1] = (byte)'M';
        BitConverter.GetBytes((uint)54).CopyTo(header, 10);
        var pixels = new byte[] { 0x00, 0x00, 0x00, 0x00 };
        var data = header.Concat(pixels).ToArray();
        var vm = CreateWithDocument(data);

        var preset = vm.AvailablePresets[0];
        var applied = vm.ApplyGlitchPreset(preset, new Random(42));

        Assert.True(applied);
        Assert.NotEqual(pixels, vm.Document!.Buffer.GetRange(54, 4));

        vm.UndoCommand.Execute(null);
        Assert.Equal(pixels, vm.Document!.Buffer.GetRange(54, 4));
    }

    [Fact]
    public void ApplyGlitchPreset_NoDocument_ReturnsFalse()
    {
        var vm = new MainViewModel();

        Assert.False(vm.ApplyGlitchPreset(new GlitchPreset("x", "y")));
    }
}
