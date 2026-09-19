using System.IO;
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
    public PreviewViewModel Preview { get; } = new();

    [RelayCommand]
    private void Load(string filePath)
    {
        var doc = ImageDocument.Load(filePath);
        Document = doc;
        StatusText = $"{Path.GetFileName(filePath)} — {doc.Format} — {doc.Buffer.Length:N0} bytes";
        HexGrid.Load(doc.Buffer);
        RequestPreviewDecode();
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }

    public void CommitByteEdit(int offset, byte value)
    {
        if (Document is null) return;

        var oldValue = Document.Buffer.ReadByte(offset);
        if (oldValue == value) return;

        Document.History.Execute(new WriteByteCommand(offset, oldValue, value), Document.Buffer);
        HexGrid.RefreshRow(offset, Document.Buffer);
        RequestPreviewDecode();
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        if (Document is null) return;

        Document.History.Undo(Document.Buffer);
        HexGrid.Load(Document.Buffer);
        RequestPreviewDecode();
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }

    private bool CanUndo() => Document?.History.CanUndo ?? false;

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        if (Document is null) return;

        Document.History.Redo(Document.Buffer);
        HexGrid.Load(Document.Buffer);
        RequestPreviewDecode();
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }

    private bool CanRedo() => Document?.History.CanRedo ?? false;

    private void RequestPreviewDecode()
    {
        if (Document is null) return;
        Preview.RequestDecode(Document.Buffer.ToArray());
    }
}
