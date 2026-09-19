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

    public HexGridViewModel HexGrid { get; } = new();

    [RelayCommand]
    private void Load(string filePath)
    {
        var doc = ImageDocument.Load(filePath);
        Document = doc;
        StatusText = $"{Path.GetFileName(filePath)} — {doc.Format} — {doc.Buffer.Length:N0} bytes";
        HexGrid.Load(doc.Buffer);
    }
}
