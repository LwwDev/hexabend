using CommunityToolkit.Mvvm.ComponentModel;
using PhotoHexEditor.Core.Model;

namespace PhotoHexEditor.Core.ViewModels;

public partial class HexGridViewModel : ObservableObject
{
    [ObservableProperty]
    private IReadOnlyList<HexRowViewModel> _rows = [];

    public void Load(ByteBuffer buffer)
    {
        var rowCount = (buffer.Length + HexRowViewModel.BytesPerRow - 1) / HexRowViewModel.BytesPerRow;
        var rows = new List<HexRowViewModel>(rowCount);

        for (var offset = 0; offset < buffer.Length; offset += HexRowViewModel.BytesPerRow)
        {
            rows.Add(new HexRowViewModel(offset, buffer));
        }

        Rows = rows;
    }
}
