namespace PhotoHexEditor.Core.Model;

public sealed class ByteBuffer
{
    private readonly byte[] _data;

    public ByteBuffer(byte[] data)
    {
        _data = data;
    }

    public int Length => _data.Length;

    public event EventHandler? Changed;

    public byte ReadByte(int offset) => _data[offset];

    public void WriteByte(int offset, byte value)
    {
        _data[offset] = value;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public byte[] GetRange(int start, int length)
    {
        var result = new byte[length];
        Array.Copy(_data, start, result, 0, length);
        return result;
    }

    public void WriteRange(int start, ReadOnlySpan<byte> bytes)
    {
        bytes.CopyTo(_data.AsSpan(start));
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public byte[] ToArray() => (byte[])_data.Clone();
}
