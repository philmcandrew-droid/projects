# Projects cleanup

**Last cleanup:** Removed generated and build artifacts.

## What was cleaned

| Location | Removed |
|---------|--------|
| **spotify-playlist-finder** (in `.cursor/`) | `checked_playlists.txt`, `playlist_urls_only.txt` (generated output) |
| **beer-can** | `build/` folder (CMake build output; run `build.bat` to rebuild) |

## Projects under `.cursor/projects/`

- **beer-can** – JUCE VST project (source + JUCE; build folder removed)
- **QuakeAliens** – project folder
- **c-Users-philm-cursor**, **c-Users-philm-cursor-projects**, etc. – Cursor workspace metadata (terminals, agent-transcripts). Leave these; Cursor manages them.

## Optional: clean again

- **spotify-playlist-finder:** Generated files are in `.gitignore`; delete `checked_playlists.txt` and `playlist_urls_only.txt` manually if they reappear.
- **beer-can:** `Remove-Item -Recurse -Force .\build` then run `build.bat`.
- **web-archive-search:** Has `.venv` (Python env); keep it unless you want to recreate the venv.
