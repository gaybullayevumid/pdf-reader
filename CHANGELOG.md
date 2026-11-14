# Changelog

All notable changes to PDF Reader are documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [Semantic Versioning](https://semver.org/).

## [1.4.1] - 2026-11-14

### Fixed
- **In-app updates could fail silently.** A copy of the app that hung while exiting (no window, files still
  locked) made the installer's "close the applications" step fail; in a silent update that prompt defaults to
  *Abort*, so the update rolled back and was offered again. The installer now closes the app itself: during an
  in-app update it asks every running copy to close (open tabs and window state are saved) and ends any that is
  still running after 10 seconds; an interactive install only ends copies stuck without a window.
- The app now ends for certain after starting an update, instead of relying on the window framework's exit.
- The installer writes a log (`%TEMP%\PdfReader-Update\install.log`) so a failed update can be diagnosed.
- The update check uses the repository's current address (`pdf-reader`) instead of a redirect from the old name.

## [1.4.0] - 2026-11-14

### Added
- **Tabs**: every opened PDF gets a tab under the title bar (file name, PDF icon, close button; `+` opens more).
  Opening a file that is already open switches to its tab. Switching is instant and each tab keeps its page,
  zoom and scroll position. The sidebar pages (Home, Recent, All PDFs, Favorites, Bookmarks, Settings) never
  close tabs: clicking a tab brings its document back. Closing the active tab shows its neighbour, like a browser.
  Tabs scroll when there are many and can be reordered by dragging. Shortcuts: `Ctrl+W` / `Ctrl+F4` close,
  `Ctrl+Tab` / `Ctrl+Shift+Tab` switch.
- **Session restore**: open tabs, the tab in front and each tab's reading position are saved
  (`session.json` next to the database) and come back on the next start. Only the tab in front loads right away;
  the others load when first shown. Files that no longer exist are skipped.

### Changed
- Opening several files at once opens all of them (previously only the first). Hidden tabs free their page
  bitmaps; at most 5 documents stay loaded, older ones reopen at their saved position when shown again.

## [1.3.0] - 2026-11-14

### Added
- **Text selection**: drag over a page with the mouse or pen to select text, across pages too (the view scrolls
  when the pointer leaves it); double-click selects a word, Shift+click extends the selection. `Ctrl+C` or the
  right-click menu copies, `Ctrl+A` selects all. The cursor turns into an I-beam over text. Text is read with
  PdfPig on demand, off the UI thread; scanned pages without a text layer cannot be selected.

### Changed
- Very long documents scroll 1:1 with the mouse wheel and keyboard: the page canvas is a window that slides along
  the document instead of a compressed scroll range, with a scroll bar that spans the whole document. Page frames
  are recycled, so memory stays flat however many pages are scrolled past.
- The page number box widens for documents with many pages.

### Fixed
- Shortcut tooltips (`Ctrl++`, `F11`) no longer pop up over the pages while scrolling.
- Home, End, Page Up and Page Down jump correctly in very long documents.

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
