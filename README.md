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

Nothing autosaves — edits only live in memory until you hit "Save As...", which writes the glitched bytes out to a new file (defaults to `{name}_glitched{ext}`, never overwrites your original). Close the app without saving and your edits are gone.

### finding your way around the bytes

- **Go to (hex)** jumps straight to a byte offset and highlights it.
- **Find** searches for a byte pattern — hex (e.g. `FF D8`) or ASCII — and "Find Next" cycles through matches, wrapping around.

### selection + bulk glitch ops

Enter a hex start/end offset and hit "Select" to mark a byte range (it highlights blue in the grid), then:

- **Randomize** — overwrites the range with random bytes
- **Invert** — flips every bit in the range
- **Fill** — overwrites the range with one repeated byte value
- **Shift** — adds a delta (can be negative) to every byte in the range, mod 256

All of these go through undo/redo like a normal edit.

### glitch presets

For BMP and JPEG, the "Apply Preset" dropdown finds the actual pixel/scan data range for you (BMP: past the `bfOffBits` header offset; JPEG: past the first SOS scan marker) and randomizes it — skips the header bytes that would just break decoding. Not offered for PNG/unknown formats since there isn't a reliably glitchable range to target.

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
