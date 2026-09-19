using PhotoHexEditor.Core.Model;
using PhotoHexEditor.Core.ViewModels;

namespace PhotoHexEditor.Tests;

public class HexRowViewModelTests
{
    [Fact]
    public void FullRow_FormatsAllSixteenBytes()
    {
        var buffer = new ByteBuffer(Enumerable.Range(0, 16).Select(i => (byte)i).ToArray());

        var row = new HexRowViewModel(0, buffer);

        Assert.Equal("00000000", row.OffsetText);
        Assert.Equal("00", row.HexBytes[0].Text);
        Assert.Equal("0F", row.HexBytes[15].Text);
        Assert.True(row.HexBytes[15].IsValid);
        Assert.Equal(16, row.AsciiText.Length);
    }

    [Fact]
    public void PartialRow_PadsRemainingHexCellsEmpty()
    {
        var buffer = new ByteBuffer([0x41, 0x42, 0x43]);

        var row = new HexRowViewModel(0, buffer);

        Assert.Equal("41", row.HexBytes[0].Text);
        Assert.Equal("42", row.HexBytes[1].Text);
        Assert.Equal("43", row.HexBytes[2].Text);
        Assert.Equal(string.Empty, row.HexBytes[3].Text);
        Assert.False(row.HexBytes[3].IsValid);
        Assert.True(row.HexBytes[3].IsReadOnly);
        Assert.Equal("ABC", row.AsciiText);
    }

    [Fact]
    public void NonPrintableBytes_RenderAsDot()
    {
        var buffer = new ByteBuffer([0x00, 0x41, 0x7F]);

        var row = new HexRowViewModel(0, buffer);

        Assert.Equal(".A.", row.AsciiText);
    }
}

public class HexGridViewModelTests
{
    [Fact]
    public void Load_ProducesOneRowPerSixteenBytes()
    {
        var buffer = new ByteBuffer(new byte[35]);
        var vm = new HexGridViewModel();

        vm.Load(buffer);

        Assert.Equal(3, vm.Rows.Count);
        Assert.Equal(0, vm.Rows[0].Offset);
        Assert.Equal(16, vm.Rows[1].Offset);
        Assert.Equal(32, vm.Rows[2].Offset);
    }

    [Fact]
    public void Load_EmptyBuffer_ProducesNoRows()
    {
        var vm = new HexGridViewModel();

        vm.Load(new ByteBuffer([]));

        Assert.Empty(vm.Rows);
    }

    [Fact]
    public void RefreshRow_UpdatesOnlyTheAffectedRow()
    {
        var buffer = new ByteBuffer(new byte[35]);
        var vm = new HexGridViewModel();
        vm.Load(buffer);
        var untouchedRow = vm.Rows[0];

        buffer.WriteByte(17, 0xAB);
        vm.RefreshRow(17, buffer);

        Assert.Same(untouchedRow, vm.Rows[0]);
        Assert.Equal("AB", vm.Rows[1].HexBytes[1].Text);
    }
}
