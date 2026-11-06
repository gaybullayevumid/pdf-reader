# Changelog

All notable changes to PDF Reader are documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [Semantic Versioning](https://semver.org/).

## [1.2.0] - 2026-11-06

### Added
- **Languages**: English, Uzbek (O‘zbekcha) and Russian (Русский). Every user-facing string is translated, including
  navigation, toolbars, tooltips, dialogs, notifications, error messages, empty states and accessibility names.
  The language is chosen in Settings, applies instantly without a restart and is remembered. On first start the
  Windows display language is used when it is supported, otherwise English. Dates and sizes follow the language.
- **Window state**: the window's size, position, maximized and full screen state are saved and restored on the next
  start. State is written shortly after every change, so it also survives a crash. Saved bounds are validated
  against the connected displays, so a window never opens off-screen.
- **Full screen**: `F11` or the toolbar toggles it, `Esc` leaves it; a switch in Settings stays in sync.
- **Settings** reorganised into *Appearance*, *Language*, *Window & Display* and *About*, with options to remember
  the window state and to reset the window layout.
- **In-app updates**: checks GitHub Releases on start (can be turned off) and from Settings → About, shows an update
  bar, then downloads the installer, verifies its SHA-256 checksum, installs it silently and restarts the app.
- Minimum window size (640 × 420) so the layout never breaks.
- Unit tests (`tests/PdfReader.Tests`) for language resolution, translation completeness, window placement,
  settings persistence and release parsing.

### Changed
- Pages use the full available width instead of a fixed centered column, and the sidebar collapses automatically
  in narrow windows. Settings cards place their control under the text when there is not enough room.
- The reader hides the empty application toolbar and shows its own bar with *Open PDF* and *Full screen*.
- Version is defined once, in `PdfReader.csproj`; the installer script receives it from `installer/build.ps1`.

### Fixed
- Full screen (and maximized) state was not saved, so the app always started in a normal window.
- The sidebar *Settings* entry kept the Windows display language, and its accessible name was not translated.
- The password dialog could fail on a very fast start when the page was not yet attached to the window.

## [1.1.0] - 2026-11-06

### Added
- Working PDF viewer: continuous scrolling, lazy page rendering, zoom, page jump, keyboard navigation,
  password-protected PDFs and last-page restore. Handles very long documents and very large pages.
- Opening PDFs from the file picker, drag and drop, the library, the command line and *Open with*.
- New application icon and an Inno Setup installer with an optional portable ZIP.

### Changed
- Project renamed to **PDF Reader** and moved to the repository root.

## [1.0.0] - 2026-11-06

### Added
- First release: application shell with sidebar navigation, library (recent files, favorites), light/dark theme
  and settings.
