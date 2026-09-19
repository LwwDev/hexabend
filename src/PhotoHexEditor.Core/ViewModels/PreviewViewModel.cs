using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PhotoHexEditor.Core.ViewModels;

public partial class PreviewViewModel : ObservableObject
{
    private const int DebounceMilliseconds = 300;

    [ObservableProperty]
    private BitmapSource? _currentPreview;

    [ObservableProperty]
    private DecodeStatus _status = DecodeStatus.Empty;

    public bool HasDecodeError => Status == DecodeStatus.Failed;

    private readonly Lock _lock = new();
    private Timer? _debounceTimer;
    private byte[]? _pendingBytes;

    partial void OnStatusChanged(DecodeStatus value) => OnPropertyChanged(nameof(HasDecodeError));

    /// <summary>
    /// Schedules a redecode of <paramref name="bytes"/>, coalescing rapid successive
    /// calls (e.g. fast typing) into a single decode ~<see cref="DebounceMilliseconds"/> after the last call.
    /// </summary>
    public void RequestDecode(byte[] bytes)
    {
        lock (_lock)
        {
            _pendingBytes = bytes;
            _debounceTimer ??= new Timer(OnDebounceElapsed, null, Timeout.Infinite, Timeout.Infinite);
            _debounceTimer.Change(DebounceMilliseconds, Timeout.Infinite);
        }
    }

    private void OnDebounceElapsed(object? state)
    {
        byte[]? bytes;
        lock (_lock)
        {
            bytes = _pendingBytes;
        }

        if (bytes is null) return;

        var decoded = TryDecode(bytes, out var image);
        ApplyResult(decoded, image);
    }

    private void ApplyResult(bool decoded, BitmapSource? image)
    {
        void Apply()
        {
            if (decoded)
            {
                CurrentPreview = image;
                Status = DecodeStatus.Ok;
            }
            else
            {
                // Keep showing the last successfully-decoded bitmap; only the status flips.
                Status = DecodeStatus.Failed;
            }
        }

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            Apply();
        }
        else
        {
            dispatcher.BeginInvoke(Apply);
        }
    }

    public static bool TryDecode(byte[] bytes, out BitmapSource? image)
    {
        try
        {
            using var stream = new MemoryStream(bytes);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            image = bitmap;
            return true;
        }
        catch (Exception)
        {
            // Decoding arbitrary hand-edited bytes can throw almost anything (WIC's
            // NotSupportedException/FileFormatException are the common cases, but not
            // the only ones) — the whole point of this path is to never let a bad edit
            // crash the app, so we deliberately swallow broadly here.
            image = null;
            return false;
        }
    }
}
