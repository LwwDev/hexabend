# hexabend

hxd, but for glitching images. open a photo, edit its raw bytes in a hex grid, watch the preview corrupt live.

## running it

```
dotnet run --project src/PhotoHexEditor.App
```

Needs the .NET 10 SDK, Windows only (WPF).

## how it works

Open an image, edit hex bytes in the grid, the preview updates as you type. Bad edits just show a "can't decode" badge instead of crashing.

**BMP glitches best** — uncompressed, so edits stay visible without breaking the file. JPEG works too if you edit past the header. PNG usually just fails to decode on edit.

- `Ctrl+Z` / `Ctrl+Y` — undo/redo
- **Go to** / **Find** — jump to an offset or search for a hex/ASCII pattern
- **Select a range + Randomize/Invert/Fill/Shift** — bulk-edit a chunk of bytes at once
- **Apply Preset** — auto-targets the pixel/scan data for BMP or JPEG and randomizes it

Nothing autosaves — "Save As..." writes to a new file and never touches the original.

## project layout

```
src/
  PhotoHexEditor.Core/   models, view models — testable, no WPF
  PhotoHexEditor.App/    the WPF window
  PhotoHexEditor.Tests/  xunit tests
samples/                 sample.bmp for trying it out
```

## running tests

```
dotnet test
```
