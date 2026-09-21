using PhotoHexEditor.Core.Model;
using PhotoHexEditor.Core.Search;

namespace PhotoHexEditor.Tests;

public class ByteSearchTests
{
    [Fact]
    public void FindAll_FindsOverlappingMatches()
    {
        var buffer = new ByteBuffer([0x41, 0x41, 0x41]);

        var matches = ByteSearch.FindAll(buffer, [0x41, 0x41]);

        Assert.Equal([0, 1], matches);
    }

    [Fact]
    public void FindAll_NoMatch_ReturnsEmpty()
    {
        var buffer = new ByteBuffer([0x00, 0x01, 0x02]);

        Assert.Empty(ByteSearch.FindAll(buffer, [0xFF]));
    }

    [Fact]
    public void FindAll_EmptyPattern_ReturnsEmpty()
    {
        var buffer = new ByteBuffer([0x00]);

        Assert.Empty(ByteSearch.FindAll(buffer, []));
    }

    [Theory]
    [InlineData("41 42", new byte[] { 0x41, 0x42 })]
    [InlineData("4142", new byte[] { 0x41, 0x42 })]
    public void ParseHexPattern_ValidInput_ParsesBytes(string input, byte[] expected)
    {
        Assert.Equal(expected, ByteSearch.ParseHexPattern(input));
    }

    [Theory]
    [InlineData("4")]
    [InlineData("ZZ")]
    [InlineData("")]
    public void ParseHexPattern_InvalidInput_ReturnsNull(string input)
    {
        Assert.Null(ByteSearch.ParseHexPattern(input));
    }
}
