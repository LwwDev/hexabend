using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
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
}
