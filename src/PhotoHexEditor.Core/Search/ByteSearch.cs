using System.Globalization;
using PhotoHexEditor.Core.Model;

namespace PhotoHexEditor.Core.Search;

public static class ByteSearch
{
    public static List<int> FindAll(ByteBuffer buffer, byte[] pattern)
    {
        var results = new List<int>();
        if (pattern.Length == 0) return results;

        var data = buffer.AsSpan();
        var searchStart = 0;
        while (searchStart <= data.Length - pattern.Length)
        {
            var relativeIndex = data[searchStart..].IndexOf(pattern);
            if (relativeIndex < 0) break;

            results.Add(searchStart + relativeIndex);
            searchStart += relativeIndex + 1;
        }

        return results;
    }

    public static byte[]? ParseHexPattern(string text)
    {
        var cleaned = text.Replace(" ", "");
        if (cleaned.Length == 0 || cleaned.Length % 2 != 0) return null;

        var bytes = new byte[cleaned.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            if (!byte.TryParse(cleaned.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bytes[i]))
                return null;
        }

        return bytes;
    }
}
