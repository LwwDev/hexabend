namespace PhotoHexEditor.Core.ViewModels;

public sealed class HexByteCellViewModel(int offset, string text, bool isValid, bool isHighlighted = false, bool isSelected = false)
{
    public int Offset { get; } = offset;
    public string Text { get; } = text;
    public bool IsValid { get; } = isValid;
    public bool IsReadOnly => !IsValid;
    public bool IsHighlighted { get; } = isHighlighted;
    public bool IsSelected { get; } = isSelected;
}
