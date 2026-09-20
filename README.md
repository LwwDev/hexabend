# hexabend

hxd, but for glitching images. open a photo, edit its raw bytes in a hex grid, watch the preview corrupt live.

## running it

```
dotnet run --project src/PhotoHexEditor.App
```

or build once and just run the exe:

```
dotnet build
src\PhotoHexEditor.App\bin\Debug\net10.0-windows\PhotoHexEditor.App.exe
```

Needs the .NET 10 SDK, Windows only (WPF).

## how it works

Click "Open...", pick an image, edit hex bytes directly in the grid. The preview pane redecodes ~300ms after you stop typing and shows the glitched result. If your edit makes the file undecodable it just keeps showing the last good frame with a badge instead of crashing.

**BMP is the format to use for glitching** — uncompressed, so edits reliably produce visible corruption without breaking the file. JPEG mostly works too (edit past the header, into the scan data). PNG will usually just fail to decode on edits (chunk CRCs + compressed pixel data) — sniffed and openable, but not a good glitch target yet.

`Ctrl+Z` / `Ctrl+Y` to undo/redo edits.

## project layout

```
src/
  PhotoHexEditor.Core/   models, view models — no WPF UI code, unit testable
  PhotoHexEditor.App/    the actual WPF window
  PhotoHexEditor.Tests/  xunit tests for Core
samples/                 sample.bmp for trying it out
```

## running tests

```
dotnet test
```
