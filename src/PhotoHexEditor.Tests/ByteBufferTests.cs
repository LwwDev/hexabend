using PhotoHexEditor.Core.Model;

namespace PhotoHexEditor.Tests;

public class ByteBufferTests
{
    [Fact]
    public void ReadByte_ReturnsValueAtOffset()
    {
        var buffer = new ByteBuffer([0x01, 0x02, 0x03]);

        Assert.Equal(0x02, buffer.ReadByte(1));
    }

    [Fact]
    public void WriteByte_UpdatesValueAndRaisesChanged()
    {
        var buffer = new ByteBuffer([0x00, 0x00]);
        var raised = false;
        buffer.Changed += (_, _) => raised = true;

        buffer.WriteByte(0, 0xFF);

        Assert.Equal(0xFF, buffer.ReadByte(0));
        Assert.True(raised);
    }

    [Fact]
    public void GetRange_ReturnsCopyOfRequestedBytes()
    {
        var buffer = new ByteBuffer([0x10, 0x20, 0x30, 0x40]);

        var range = buffer.GetRange(1, 2);

        Assert.Equal([0x20, 0x30], range);
    }

    [Fact]
    public void WriteRange_UpdatesBytesAndRaisesChanged()
    {
        var buffer = new ByteBuffer([0x00, 0x00, 0x00, 0x00]);
        var raised = false;
        buffer.Changed += (_, _) => raised = true;

        buffer.WriteRange(1, [0xAA, 0xBB]);

        Assert.Equal([0x00, 0xAA, 0xBB, 0x00], buffer.ToArray());
        Assert.True(raised);
    }

    [Fact]
    public void ToArray_ReturnsIndependentCopy()
    {
        var buffer = new ByteBuffer([0x01, 0x02]);

        var copy = buffer.ToArray();
        copy[0] = 0xFF;

        Assert.Equal(0x01, buffer.ReadByte(0));
    }
}
