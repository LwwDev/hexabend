using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PhotoHexEditor.Core.Model;

namespace PhotoHexEditor.Core.ViewModels;

public partial class HexGridViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<HexRowViewModel> _rows = [];

    public void Load(ByteBuffer buffer)
    {
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

    public void RefreshRow(int offset, ByteBuffer buffer)
    {
        var rowIndex = offset / HexRowViewModel.BytesPerRow;
        if (rowIndex < 0 || rowIndex >= Rows.Count) return;

        var rowOffset = rowIndex * HexRowViewModel.BytesPerRow;
        Rows[rowIndex] = new HexRowViewModel(rowOffset, buffer);
    }
}
