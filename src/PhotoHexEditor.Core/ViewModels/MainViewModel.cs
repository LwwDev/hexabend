using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhotoHexEditor.Core.Model;

namespace PhotoHexEditor.Core.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ImageDocument? _document;

    [ObservableProperty]
    private string _statusText = "No file open";

    [ObservableProperty]
    private string _hexDump = string.Empty;

    [RelayCommand]
    private void Load(string filePath)
    {
        var doc = ImageDocument.Load(filePath);
        Document = doc;
        StatusText = $"{Path.GetFileName(filePath)} — {doc.Format} — {doc.Buffer.Length:N0} bytes";
        HexDump = BuildHexDump(doc.Buffer);
    }

    private static string BuildHexDump(ByteBuffer buffer)
    {
        var sb = new StringBuilder();
        var length = buffer.Length;

        for (var offset = 0; offset < length; offset += 16)
        {
            var rowLength = Math.Min(16, length - offset);

            sb.Append(offset.ToString("X8")).Append("  ");

            for (var i = 0; i < 16; i++)
            {
                sb.Append(i < rowLength ? buffer.ReadByte(offset + i).ToString("X2") : "  ").Append(' ');
            }

            sb.Append(' ');

            for (var i = 0; i < rowLength; i++)
            {
                var b = buffer.ReadByte(offset + i);
                sb.Append(b is >= 0x20 and < 0x7F ? (char)b : '.');
            }

            sb.Append('\n');
        }

        return sb.ToString();
    }
}
