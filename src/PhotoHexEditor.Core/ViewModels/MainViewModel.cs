using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhotoHexEditor.Core.Model;
using PhotoHexEditor.Core.Search;

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

    public string SuggestedSaveFileName
    {
        get
        {
            if (Document is null) return "glitched.bin";

            var directory = Path.GetDirectoryName(Document.FilePath) ?? string.Empty;
            var name = Path.GetFileNameWithoutExtension(Document.FilePath);
            var extension = Path.GetExtension(Document.FilePath);
            return Path.Combine(directory, $"{name}_glitched{extension}");
        }
    }

    public void SaveAs(string filePath)
    {
        if (Document is null) return;

        File.WriteAllBytes(filePath, Document.Buffer.ToArray());
        Document.MarkSaved();
    }

    private List<int> _searchMatches = [];
    private int _searchMatchIndex = -1;
    private string? _lastSearchQuery;
    private bool _lastSearchIsHex;

    /// <summary>
    /// Finds the next match for <paramref name="query"/>, cycling back to the first match
    /// once the last one is passed. Re-runs the search only when the query or mode changed
    /// since the last call, so repeated Enter/"Find Next" presses just advance the cursor.
    /// </summary>
    public int? FindNext(string query, bool isHex)
    {
        if (Document is null || string.IsNullOrEmpty(query)) return null;

        if (query != _lastSearchQuery || isHex != _lastSearchIsHex)
        {
            var pattern = isHex ? ByteSearch.ParseHexPattern(query) : Encoding.ASCII.GetBytes(query);
            _searchMatches = pattern is null || pattern.Length == 0 ? [] : ByteSearch.FindAll(Document.Buffer, pattern);
            _searchMatchIndex = -1;
            _lastSearchQuery = query;
            _lastSearchIsHex = isHex;
        }

        if (_searchMatches.Count == 0) return null;

        _searchMatchIndex = (_searchMatchIndex + 1) % _searchMatches.Count;
        var offset = _searchMatches[_searchMatchIndex];
        HexGrid.SetHighlight(offset, Document.Buffer);
        return offset;
    }

    public bool GoToOffset(int offset)
    {
        if (Document is null || offset < 0 || offset >= Document.Buffer.Length) return false;

        HexGrid.SetHighlight(offset, Document.Buffer);
        return true;
    }

    [ObservableProperty]
    private int? _selectionStart;

    [ObservableProperty]
    private int? _selectionEnd;

    public bool SetSelection(int start, int end)
    {
        if (Document is null) return false;

        var length = Document.Buffer.Length;
        if (start < 0 || end < 0 || start >= length || end >= length) return false;

        var (lo, hi) = start <= end ? (start, end) : (end, start);
        SelectionStart = lo;
        SelectionEnd = hi;
        HexGrid.SetSelection((lo, hi), Document.Buffer);
        return true;
    }

    public void ClearSelection()
    {
        SelectionStart = null;
        SelectionEnd = null;
        if (Document is not null) HexGrid.SetSelection(null, Document.Buffer);
    }

    public bool FillSelection(byte value) => ApplyToSelection(old => Enumerable.Repeat(value, old.Length).ToArray());

    public bool InvertSelection() => ApplyToSelection(old => old.Select(b => (byte)~b).ToArray());

    public bool ShiftSelection(int delta) => ApplyToSelection(old => old.Select(b => (byte)(b + delta)).ToArray());

    public bool RandomizeSelection(Random? random = null)
    {
        var rng = random ?? Random.Shared;
        return ApplyToSelection(old =>
        {
            var bytes = new byte[old.Length];
            rng.NextBytes(bytes);
            return bytes;
        });
    }

    private bool ApplyToSelection(Func<byte[], byte[]> transform)
    {
        if (Document is null || SelectionStart is null || SelectionEnd is null) return false;

        var start = SelectionStart.Value;
        var length = SelectionEnd.Value - start + 1;
        var oldBytes = Document.Buffer.GetRange(start, length);
        var newBytes = transform(oldBytes);
        return ApplyRangeEdit(start, oldBytes, newBytes);
    }

    private bool ApplyRangeEdit(int start, byte[] oldBytes, byte[] newBytes)
    {
        if (Document is null) return false;
        if (oldBytes.AsSpan().SequenceEqual(newBytes)) return false;

        Document.History.Execute(new WriteRangeCommand(start, oldBytes, newBytes), Document.Buffer);
        RefreshGridRange(start, newBytes.Length);
        RequestPreviewDecode();
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
        return true;
    }

    private void RefreshGridRange(int start, int length)
    {
        if (Document is null) return;

        var firstRow = start / HexRowViewModel.BytesPerRow;
        var lastRow = (start + length - 1) / HexRowViewModel.BytesPerRow;
        for (var rowIndex = firstRow; rowIndex <= lastRow; rowIndex++)
        {
            HexGrid.RefreshRow(rowIndex * HexRowViewModel.BytesPerRow, Document.Buffer);
        }
    }
}
