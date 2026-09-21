using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PhotoHexEditor.Core.Model;

namespace PhotoHexEditor.Core.ViewModels;

public partial class HexGridViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<HexRowViewModel> _rows = [];

    private int? _highlightOffset;
    private (int Start, int End)? _selection;

    public void Load(ByteBuffer buffer)
    {
        _highlightOffset = null;
        _selection = null;

        var rowCount = (buffer.Length + HexRowViewModel.BytesPerRow - 1) / HexRowViewModel.BytesPerRow;
        var rows = new List<HexRowViewModel>(rowCount);

        for (var offset = 0; offset < buffer.Length; offset += HexRowViewModel.BytesPerRow)
        {
            rows.Add(new HexRowViewModel(offset, buffer));
        }

        // Construct via the IEnumerable<T> constructor so this is a single assignment
        // (one PropertyChanged) rather than N individual CollectionChanged events.
        Rows = new ObservableCollection<HexRowViewModel>(rows);
    }

    public void RefreshRow(int offset, ByteBuffer buffer) => RebuildRow(offset / HexRowViewModel.BytesPerRow, buffer);

    public void SetHighlight(int? offset, ByteBuffer buffer)
    {
        var previous = _highlightOffset;
        _highlightOffset = offset;

        if (previous is not null) RebuildRow(previous.Value / HexRowViewModel.BytesPerRow, buffer);
        if (offset is not null) RebuildRow(offset.Value / HexRowViewModel.BytesPerRow, buffer);
    }

    public void SetSelection((int Start, int End)? selection, ByteBuffer buffer)
    {
        var previous = _selection;
        _selection = selection;

        RebuildRowRange(previous, buffer);
        RebuildRowRange(selection, buffer);
    }

    private void RebuildRowRange((int Start, int End)? range, ByteBuffer buffer)
    {
        if (range is null) return;

        var firstRow = range.Value.Start / HexRowViewModel.BytesPerRow;
        var lastRow = range.Value.End / HexRowViewModel.BytesPerRow;
        for (var rowIndex = firstRow; rowIndex <= lastRow; rowIndex++)
        {
            RebuildRow(rowIndex, buffer);
        }
    }

    private void RebuildRow(int rowIndex, ByteBuffer buffer)
    {
        if (rowIndex < 0 || rowIndex >= Rows.Count) return;

        var rowOffset = rowIndex * HexRowViewModel.BytesPerRow;
        Rows[rowIndex] = new HexRowViewModel(rowOffset, buffer, _highlightOffset, _selection);
    }
}
