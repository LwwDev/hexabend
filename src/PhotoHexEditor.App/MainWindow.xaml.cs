using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using PhotoHexEditor.Core.Formats;
using PhotoHexEditor.Core.ViewModels;

namespace PhotoHexEditor.App;

public partial class MainWindow : Window
{
    private static readonly Regex HexDigit = new("^[0-9A-Fa-f]$");

    private readonly MainViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void OpenButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image files (*.bmp;*.jpg;*.jpeg;*.png)|*.bmp;*.jpg;*.jpeg;*.png|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.LoadCommand.Execute(dialog.FileName);
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Document is null) return;

        var suggested = _viewModel.SuggestedSaveFileName;
        var dialog = new SaveFileDialog
        {
            FileName = Path.GetFileName(suggested),
            InitialDirectory = Path.GetDirectoryName(suggested),
            Filter = "Image files (*.bmp;*.jpg;*.jpeg;*.png)|*.bmp;*.jpg;*.jpeg;*.png|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.SaveAs(dialog.FileName);
        }
    }

    private void HexCell_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !HexDigit.IsMatch(e.Text);
    }

    private void HexCell_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitHexCellEdit(sender as TextBox);
            e.Handled = true;
        }
    }

    private void HexCell_LostFocus(object sender, RoutedEventArgs e)
    {
        CommitHexCellEdit(sender as TextBox);
    }

    private void CommitHexCellEdit(TextBox? textBox)
    {
        if (textBox?.DataContext is not HexByteCellViewModel cell || !cell.IsValid) return;

        if (byte.TryParse(textBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
        {
            _viewModel.CommitByteEdit(cell.Offset, value);
        }
        else
        {
            textBox.Text = cell.Text;
        }
    }

    private void GoToOffsetButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryParseOffset(GoToOffsetBox.Text, out var offset)) return;

        if (_viewModel.GoToOffset(offset))
        {
            ScrollToOffset(offset);
        }
    }

    private void FindNextButton_Click(object sender, RoutedEventArgs e)
    {
        var query = SearchBox.Text;
        if (string.IsNullOrEmpty(query)) return;

        var offset = _viewModel.FindNext(query, SearchHexCheckBox.IsChecked == true);
        if (offset is null)
        {
            SearchStatusText.Text = "No matches";
            return;
        }

        SearchStatusText.Text = $"@ 0x{offset:X}";
        ScrollToOffset(offset.Value);
    }

    private void ScrollToOffset(int offset)
    {
        var rowIndex = offset / HexRowViewModel.BytesPerRow;
        if (rowIndex < 0 || rowIndex >= _viewModel.HexGrid.Rows.Count) return;

        HexRowsList.ScrollIntoView(_viewModel.HexGrid.Rows[rowIndex]);
    }

    private static bool TryParseOffset(string text, out int offset)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) trimmed = trimmed[2..];

        return int.TryParse(trimmed, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offset);
    }

    private void SelectRangeButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryParseOffset(SelectionStartBox.Text, out var start)) return;
        if (!TryParseOffset(SelectionEndBox.Text, out var end)) return;

        _viewModel.SetSelection(start, end);
    }

    private void RandomizeButton_Click(object sender, RoutedEventArgs e) => _viewModel.RandomizeSelection();

    private void InvertButton_Click(object sender, RoutedEventArgs e) => _viewModel.InvertSelection();

    private void FillButton_Click(object sender, RoutedEventArgs e)
    {
        if (byte.TryParse(FillValueBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
        {
            _viewModel.FillSelection(value);
        }
    }

    private void ShiftButton_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(ShiftDeltaBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var delta))
        {
            _viewModel.ShiftSelection(delta);
        }
    }

    private void ApplyPresetButton_Click(object sender, RoutedEventArgs e)
    {
        if (PresetComboBox.SelectedItem is GlitchPreset preset)
        {
            _viewModel.ApplyGlitchPreset(preset);
        }
    }
}
