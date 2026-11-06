"""Main window and application."""

import os
import sys
import time

import gi

gi.require_version("Gtk", "3.0")
gi.require_version("Gdk", "3.0")
gi.require_version("Poppler", "0.18")
from gi.repository import Gdk, Gio, GLib, Gtk, Poppler  # noqa: E402

from . import APP_ID, APP_NAME, __version__, i18n  # noqa: E402
from .i18n import tr  # noqa: E402
from .store import Library, Settings  # noqa: E402
from .viewer import PdfView  # noqa: E402

CSS = b"""
.empty-title { font-size: 15pt; font-weight: 600; }
.welcome { font-size: 22pt; font-weight: 700; }
.doc-row { padding: 10px 12px; }
.doc-name { font-weight: 600; }
.dim { opacity: 0.65; }
"""


def human_size(n):
    if n < 1024:
        return tr("size.b", n)
    if n < 1024 * 1024:
        return tr("size.kb", round(n / 1024))
    return tr("size.mb", f"{n / 1048576:.1f}")


class MainWindow(Gtk.ApplicationWindow):
    def __init__(self, app, settings, library):
        super().__init__(application=app, title=APP_NAME)
        self.settings, self.library = settings, library
        self.path = None
        self.filter_fav = False
        self._tr = []                    # (setter, key) pairs re-applied when the language changes
        self._save_id = 0
        self._normal = (settings["window"]["width"], settings["window"]["height"])
        self._max = self._full = False

        self.set_default_size(*self._normal)
        self.set_size_request(640, 420)
        self.set_icon_name("pdf-reader")
        self._build()
        self._apply_theme()
        self._retranslate()

        self.connect("key-press-event", self._on_key)
        self.connect("configure-event", self._on_configure)
        self.connect("window-state-event", self._on_state)
        self.connect("delete-event", self._on_close)
        self.drag_dest_set(Gtk.DestDefaults.ALL, [Gtk.TargetEntry.new("text/uri-list", 0, 0)], Gdk.DragAction.COPY)
        self.connect("drag-data-received", self._on_drop)

        win = settings["window"]
        if settings["remember_window"]:
            if win.get("fullscreen"):
                self.fullscreen()
            elif win.get("maximized"):
                self.maximize()

    # ---- helpers --------------------------------------------------------------------------------

    def _t(self, setter, key):
        self._tr.append((setter, key))
        setter(tr(key))

    def _retranslate(self):
        for setter, key in self._tr:
            setter(tr(key))
        self._refresh_list()
        self._update_title()
        self._update_page_label()

    def _icon_button(self, icon, tip_key, handler):
        b = Gtk.Button.new_from_icon_name(icon, Gtk.IconSize.BUTTON)
        b.connect("clicked", lambda *_: handler())
        self._t(b.set_tooltip_text, tip_key)
        return b

    # ---- UI -------------------------------------------------------------------------------------

    def _build(self):
        header = Gtk.HeaderBar(show_close_button=True)
        self.set_titlebar(header)
        self.header = header

        self.back_btn = self._icon_button("go-previous-symbolic", "viewer.back", self.show_home)
        header.pack_start(self.back_btn)
        self.open_btn = self._icon_button("document-open-symbolic", "toolbar.open.tip", self.choose_file)
        header.pack_start(self.open_btn)

        self.title_label = Gtk.Label(ellipsize=3, max_width_chars=48)
        header.set_custom_title(self.title_label)

        menu_btn = Gtk.MenuButton()
        menu_btn.set_image(Gtk.Image.new_from_icon_name("open-menu-symbolic", Gtk.IconSize.BUTTON))
        pop = Gtk.Popover()
        box = Gtk.Box(orientation=Gtk.Orientation.VERTICAL, spacing=2, margin=6)
        self.settings_item = Gtk.ModelButton(halign=Gtk.Align.FILL)
        self.settings_item.connect("clicked", lambda *_: (pop.popdown(), self.show_settings()))
        self._t(lambda s: self.settings_item.set_property("text", s), "nav.settings")
        box.add(self.settings_item)
        box.show_all()
        pop.add(box)
        menu_btn.set_popover(pop)
        header.pack_end(menu_btn)
        self.full_btn = self._icon_button("view-fullscreen-symbolic", "toolbar.fullscreen.tip", self.toggle_fullscreen)
        header.pack_end(self.full_btn)

        # viewer controls
        self.view_controls = Gtk.Box(spacing=4)
        self.page_entry = Gtk.Entry(width_chars=4, max_width_chars=6, xalign=1.0)
        self.page_entry.connect("activate", self._on_page_entry)
        self._t(self.page_entry.set_tooltip_text, "viewer.gotoPage")
        self.page_total = Gtk.Label()
        self.view_controls.pack_start(self.page_entry, False, False, 0)
        self.view_controls.pack_start(self.page_total, False, False, 0)
        self.view_controls.pack_start(self._icon_button("zoom-out-symbolic", "viewer.zoomOut", lambda: self.view.zoom_by(1 / 1.2)), False, False, 6)
        self.zoom_label = Gtk.Label(width_chars=5)
        self.view_controls.pack_start(self.zoom_label, False, False, 0)
        self.view_controls.pack_start(self._icon_button("zoom-in-symbolic", "viewer.zoomIn", lambda: self.view.zoom_by(1.2)), False, False, 0)
        self.view_controls.pack_start(self._icon_button("zoom-fit-best-symbolic", "viewer.fitWidth", lambda: self.view.fit_width()), False, False, 0)
        self.view_controls.pack_start(self._icon_button("view-restore-symbolic", "viewer.fitPage", lambda: self.view.fit_page()), False, False, 0)
        header.pack_start(self.view_controls)

        self.stack = Gtk.Stack(transition_type=Gtk.StackTransitionType.CROSSFADE)
        self.add(self.stack)
        self.stack.add_named(self._build_home(), "home")
        self.view = PdfView()
        self.view.connect("page-changed", self._on_page_changed)
        self.view.connect("zoom-changed", lambda _v, z: self.zoom_label.set_text(f"{round(z * 100)}%"))
        self.stack.add_named(self.view, "viewer")
        self.show_all()
        self.show_home()

    def _build_home(self):
        outer = Gtk.Box(orientation=Gtk.Orientation.VERTICAL, spacing=14, margin=24)
        self.welcome = Gtk.Label(xalign=0, wrap=True)
        self.welcome.get_style_context().add_class("welcome")
        self._t(self.welcome.set_text, "home.welcome")
        sub = Gtk.Label(xalign=0, wrap=True)
        sub.get_style_context().add_class("dim")
        self._t(sub.set_text, "home.subtitle")
        outer.pack_start(self.welcome, False, False, 0)
        outer.pack_start(sub, False, False, 0)

        self.browse_btn = Gtk.Button(halign=Gtk.Align.START)
        self.browse_btn.get_style_context().add_class("suggested-action")
        self.browse_btn.connect("clicked", lambda *_: self.choose_file())
        self._t(self.browse_btn.set_label, "home.browse")
        outer.pack_start(self.browse_btn, False, False, 0)

        tabs = Gtk.Box(spacing=0)
        tabs.get_style_context().add_class("linked")
        self.tab_recent = Gtk.RadioButton()
        self.tab_fav = Gtk.RadioButton(group=self.tab_recent)
        self.tab_recent.set_mode(False)
        self.tab_fav.set_mode(False)
        self._t(self.tab_recent.set_label, "nav.recent")
        self._t(self.tab_fav.set_label, "nav.favorites")
        for b, fav in ((self.tab_recent, False), (self.tab_fav, True)):
            b.connect("toggled", self._on_tab, fav)
            tabs.pack_start(b, False, False, 0)
        outer.pack_start(tabs, False, False, 0)

        self.empty_title = Gtk.Label(xalign=0.5)
        self.empty_title.get_style_context().add_class("empty-title")
        self.empty_desc = Gtk.Label(wrap=True)
        self.empty_desc.get_style_context().add_class("dim")
        self.empty_box = Gtk.Box(orientation=Gtk.Orientation.VERTICAL, spacing=6, valign=Gtk.Align.CENTER, vexpand=True)
        self.empty_box.pack_start(self.empty_title, False, False, 0)
        self.empty_box.pack_start(self.empty_desc, False, False, 0)

        self.listbox = Gtk.ListBox(selection_mode=Gtk.SelectionMode.NONE)
        self.listbox.connect("row-activated", lambda _l, row: self.open_path(row.doc_path))
        scroller = Gtk.ScrolledWindow(vexpand=True, hscrollbar_policy=Gtk.PolicyType.NEVER)
        scroller.add(self.listbox)
        self.list_stack = Gtk.Stack()
        self.list_stack.add_named(scroller, "list")
        self.list_stack.add_named(self.empty_box, "empty")
        outer.pack_start(self.list_stack, True, True, 0)
        return outer

    def _on_tab(self, button, fav):
        if button.get_active():
            self.filter_fav = fav
            self._refresh_list()

    def _refresh_list(self):
        for row in self.listbox.get_children():
            self.listbox.remove(row)
        items = [i for i in self.library.items if not self.filter_fav or i.get("favorite")]
        if not items:
            key = "favorites" if self.filter_fav else "recent"
            self.empty_title.set_text(tr(f"lib.{key}.emptyTitle"))
            self.empty_desc.set_text(tr(f"lib.{key}.emptyDesc"))
            self.list_stack.set_visible_child_name("empty")
            return
        self.list_stack.set_visible_child_name("list")
        for item in items:
            self.listbox.add(self._row(item))
        self.listbox.show_all()

    def _row(self, item):
        path = item["path"]
        row = Gtk.ListBoxRow()
        row.doc_path = path
        box = Gtk.Box(spacing=10)
        box.get_style_context().add_class("doc-row")
        box.pack_start(Gtk.Image.new_from_icon_name("x-office-document-symbolic", Gtk.IconSize.LARGE_TOOLBAR), False, False, 0)
        text = Gtk.Box(orientation=Gtk.Orientation.VERTICAL)
        name = Gtk.Label(label=os.path.basename(path), xalign=0, ellipsize=3)
        name.get_style_context().add_class("doc-name")
        opened = time.strftime("%x", time.localtime(item.get("opened", 0)))
        details = tr("doc.details", human_size(item.get("size", 0)), opened)
        sub = Gtk.Label(label=details, xalign=0, ellipsize=3)
        sub.get_style_context().add_class("dim")
        text.pack_start(name, False, False, 0)
        text.pack_start(sub, False, False, 0)
        box.pack_start(text, True, True, 0)
        fav = Gtk.Button.new_from_icon_name(
            "starred-symbolic" if item.get("favorite") else "non-starred-symbolic", Gtk.IconSize.BUTTON)
        fav.set_relief(Gtk.ReliefStyle.NONE)
        fav.set_tooltip_text(tr("doc.toggleFavorite"))
        fav.connect("clicked", lambda *_: (self.library.toggle_favorite(path), self._refresh_list()))
        rm = Gtk.Button.new_from_icon_name("edit-delete-symbolic", Gtk.IconSize.BUTTON)
        rm.set_relief(Gtk.ReliefStyle.NONE)
        rm.set_tooltip_text(tr("doc.remove"))
        rm.connect("clicked", lambda *_: (self.library.remove(path), self._refresh_list()))
        box.pack_start(fav, False, False, 0)
        box.pack_start(rm, False, False, 0)
        row.add(box)
        return row

    # ---- navigation -----------------------------------------------------------------------------

    def show_home(self):
        if self.path:
            self.library.set_page(self.path, self.view.current)
        self.path = None
        self.view.release()
        self.stack.set_visible_child_name("home")
        self.back_btn.hide()
        self.view_controls.hide()
        self._refresh_list()
        self._update_title()

    def _show_viewer(self):
        self.stack.set_visible_child_name("viewer")
        self.back_btn.show()
        self.view_controls.show()
        self.view.area.grab_focus()

    def _update_title(self):
        name = os.path.basename(self.path) if self.path else APP_NAME
        self.title_label.set_text(name)
        self.set_title(f"{name} - {APP_NAME}" if self.path else APP_NAME)

    def _update_page_label(self):
        if self.path:
            self.page_entry.set_text(str(self.view.current + 1))
            self.page_total.set_text(f"/ {self.view.page_count}")

    def _on_page_changed(self, _v, page):
        self._update_page_label()
        if self.path:
            self.library.set_page(self.path, page)

    def _on_page_entry(self, entry):
        try:
            self.view.goto_page(int(entry.get_text()) - 1)
        except ValueError:
            pass
        self._update_page_label()
        self.view.area.grab_focus()

    # ---- opening --------------------------------------------------------------------------------

    def choose_file(self):
        dlg = Gtk.FileChooserNative.new(tr("toolbar.open"), self, Gtk.FileChooserAction.OPEN, None, None)
        flt = Gtk.FileFilter()
        flt.set_name("PDF")
        flt.add_mime_type("application/pdf")
        flt.add_pattern("*.pdf")
        flt.add_pattern("*.PDF")
        dlg.add_filter(flt)
        if dlg.run() == Gtk.ResponseType.ACCEPT:
            self.open_path(dlg.get_filename())
        dlg.destroy()

    def open_path(self, path):
        path = os.path.abspath(path)
        if not os.path.isfile(path):
            return self._message(tr("opener.onlyPdf"))
        password = None
        while True:
            try:
                doc = Poppler.Document.new_from_file(GLib.filename_to_uri(path, None), password)
                break
            except GLib.Error as e:
                if e.matches(Poppler.error_quark(), Poppler.Error.ENCRYPTED):
                    password = self._ask_password(retry=password is not None)
                    if password is None:
                        return
                    continue
                return self._message(tr("viewer.error.invalid"))
        if doc.get_n_pages() < 1:
            return self._message(tr("viewer.error.invalid"))

        if self.path:
            self.library.set_page(self.path, self.view.current)
        item = self.library.touch(path, os.path.getsize(path))
        self.path = path
        self.view.dark = self._is_dark()
        self._show_viewer()
        self.view.set_document(doc, min(int(item.get("page", 0)), doc.get_n_pages() - 1))
        self._update_title()
        self._update_page_label()

    def _ask_password(self, retry):
        dlg = Gtk.Dialog(title=tr("pw.retryTitle" if retry else "pw.title"), transient_for=self, modal=True)
        entry = Gtk.Entry(visibility=False, placeholder_text=tr("pw.placeholder"), activates_default=True, margin=12)
        dlg.get_content_area().add(entry)
        dlg.add_button(tr("pw.cancel"), Gtk.ResponseType.CANCEL)
        ok = dlg.add_button(tr("pw.open"), Gtk.ResponseType.OK)
        ok.get_style_context().add_class("suggested-action")
        dlg.set_default_response(Gtk.ResponseType.OK)
        dlg.show_all()
        result = entry.get_text() if dlg.run() == Gtk.ResponseType.OK else None
        dlg.destroy()
        return result

    def _message(self, text):
        dlg = Gtk.MessageDialog(transient_for=self, modal=True, message_type=Gtk.MessageType.ERROR,
                                buttons=Gtk.ButtonsType.CLOSE, text=text)
        dlg.run()
        dlg.destroy()

    def _on_drop(self, _w, _ctx, _x, _y, data, _info, _time):
        for uri in data.get_uris():
            path = GLib.filename_from_uri(uri)[0] if uri.startswith("file:") else None
            if path:
                self.open_path(path)
                break

    # ---- keys -----------------------------------------------------------------------------------

    def _on_key(self, _w, ev):
        key, state = ev.keyval, ev.state
        ctrl = bool(state & Gdk.ModifierType.CONTROL_MASK)
        if key == Gdk.KEY_F11:
            self.toggle_fullscreen()
            return True
        if key == Gdk.KEY_Escape and self._full:
            self.toggle_fullscreen()
            return True
        if ctrl and key in (Gdk.KEY_o, Gdk.KEY_O):
            self.choose_file()
            return True
        if not self.path or isinstance(self.get_focus(), Gtk.Entry):
            return False
        v = self.view
        if ctrl and key in (Gdk.KEY_plus, Gdk.KEY_equal, Gdk.KEY_KP_Add):
            v.zoom_by(1.2)
        elif ctrl and key in (Gdk.KEY_minus, Gdk.KEY_KP_Subtract):
            v.zoom_by(1 / 1.2)
        elif ctrl and key in (Gdk.KEY_0, Gdk.KEY_KP_0):
            v.fit_width()
        elif key == Gdk.KEY_Home:
            v.goto_page(0)
        elif key == Gdk.KEY_End:
            v.goto_page(v.page_count - 1)
            v.scroll_by(1e9)
        elif key in (Gdk.KEY_Page_Down, Gdk.KEY_space):
            v.scroll_by(v.vadj.get_page_size() * 0.9)
        elif key == Gdk.KEY_Page_Up:
            v.scroll_by(-v.vadj.get_page_size() * 0.9)
        elif key == Gdk.KEY_Down:
            v.scroll_by(60)
        elif key == Gdk.KEY_Up:
            v.scroll_by(-60)
        elif key == Gdk.KEY_Right:
            v.scroll_by(dx=60)
        elif key == Gdk.KEY_Left:
            v.scroll_by(dx=-60)
        else:
            return False
        return True

    # ---- window state ---------------------------------------------------------------------------

    def toggle_fullscreen(self):
        self.unfullscreen() if self._full else self.fullscreen()

    def _on_state(self, _w, ev):
        st = ev.new_window_state
        self._max = bool(st & Gdk.WindowState.MAXIMIZED)
        self._full = bool(st & Gdk.WindowState.FULLSCREEN)
        icon = "view-restore-symbolic" if self._full else "view-fullscreen-symbolic"
        self.full_btn.set_image(Gtk.Image.new_from_icon_name(icon, Gtk.IconSize.BUTTON))
        self.header.set_visible(not self._full)
        self._schedule_save()

    def _on_configure(self, *_):
        if not (self._max or self._full):
            self._normal = self.get_size()
        self._schedule_save()
        return False

    def _schedule_save(self):
        if self._save_id:
            GLib.source_remove(self._save_id)
        self._save_id = GLib.timeout_add(400, self._save_window)

    def _save_window(self):
        self._save_id = 0
        if self.settings["remember_window"]:
            w, h = self._normal
            self.settings["window"] = {"width": w, "height": h, "maximized": self._max, "fullscreen": self._full}
        return False

    def reset_window(self):
        if self._full:
            self.unfullscreen()
        if self._max:
            self.unmaximize()
        self.resize(1100, 760)

    def _on_close(self, *_):
        if self.path:
            self.library.set_page(self.path, self.view.current)
        if self._save_id:
            GLib.source_remove(self._save_id)
        self._save_window()
        return False

    # ---- settings -------------------------------------------------------------------------------

    def _is_dark(self):
        t = self.settings["theme"]
        if t == "dark":
            return True
        if t == "light":
            return False
        return bool(Gtk.Settings.get_default().get_property("gtk-application-prefer-dark-theme")) or \
            "dark" in (Gtk.Settings.get_default().get_property("gtk-theme-name") or "").lower()

    def _apply_theme(self):
        gs = Gtk.Settings.get_default()
        t = self.settings["theme"]
        if t == "system":
            gs.reset_property("gtk-application-prefer-dark-theme")
        else:
            gs.set_property("gtk-application-prefer-dark-theme", t == "dark")
        self.view.dark = self._is_dark()
        self.view.area.queue_draw()

    def show_settings(self):
        dlg = Gtk.Dialog(title=tr("settings.title"), transient_for=self, modal=True)
        dlg.set_default_size(460, -1)
        area = dlg.get_content_area()
        grid = Gtk.Grid(row_spacing=10, column_spacing=16, margin=18)
        area.add(grid)
        row = [0]

        def section(key):
            lbl = Gtk.Label(label=tr(key), xalign=0)
            lbl.get_style_context().add_class("doc-name")
            grid.attach(lbl, 0, row[0], 2, 1)
            row[0] += 1

        def line(title, desc, widget):
            box = Gtk.Box(orientation=Gtk.Orientation.VERTICAL)
            t = Gtk.Label(label=tr(title), xalign=0)
            d = Gtk.Label(label=tr(desc), xalign=0, wrap=True, max_width_chars=34)
            d.get_style_context().add_class("dim")
            box.pack_start(t, False, False, 0)
            box.pack_start(d, False, False, 0)
            widget.set_valign(Gtk.Align.CENTER)
            grid.attach(box, 0, row[0], 1, 1)
            grid.attach(widget, 1, row[0], 1, 1)
            row[0] += 1

        section("settings.appearance")
        theme = Gtk.ComboBoxText()
        keys = ["system", "light", "dark"]
        for k in keys:
            theme.append_text(tr(f"theme.{k}"))
        theme.set_active(keys.index(self.settings["theme"]) if self.settings["theme"] in keys else 0)

        def on_theme(c):
            self.settings["theme"] = keys[c.get_active()]
            self._apply_theme()
        theme.connect("changed", on_theme)
        line("theme.title", "theme.desc", theme)

        section("settings.language")
        lang = Gtk.ComboBoxText()
        for _code, name in i18n.LANGUAGES:
            lang.append_text(name)
        lang.set_active(i18n.CODES.index(i18n.current()))

        def on_lang(c):
            code = i18n.CODES[c.get_active()]
            self.settings["language"] = code
            i18n.set_language(code)
            self._retranslate()
            dlg.set_title(tr("settings.title"))
        lang.connect("changed", on_lang)
        line("language.title", "language.desc", lang)

        section("settings.window")
        full = Gtk.Switch(active=self._full)
        full.connect("notify::active", lambda s, _p: s.get_active() != self._full and self.toggle_fullscreen())
        line("window.fullscreen.title", "window.fullscreen.desc", full)
        remember = Gtk.Switch(active=bool(self.settings["remember_window"]))
        remember.connect("notify::active", lambda s, _p: self.settings.__setitem__("remember_window", s.get_active()))
        line("window.remember.title", "window.remember.desc", remember)
        reset = Gtk.Button(label=tr("window.reset.button"))
        reset.connect("clicked", lambda *_: self.reset_window())
        line("window.reset.title", "window.reset.desc", reset)

        section("settings.about")
        about = Gtk.Label(label=f"{tr('about.version', __version__)}\n{tr('about.desc')}", xalign=0, wrap=True)
        grid.attach(about, 0, row[0], 2, 1)

        dlg.add_button(Gtk.STOCK_CLOSE, Gtk.ResponseType.CLOSE)
        dlg.show_all()
        dlg.run()
        dlg.destroy()


class Application(Gtk.Application):
    def __init__(self):
        super().__init__(application_id=APP_ID, flags=Gio.ApplicationFlags.HANDLES_OPEN)
        self.window = None

    def _window(self):
        if self.window is None:
            settings, library = Settings(), Library()
            i18n.set_language(settings["language"] or i18n.system_language())
            css = Gtk.CssProvider()
            css.load_from_data(CSS)
            Gtk.StyleContext.add_provider_for_screen(Gdk.Screen.get_default(), css, Gtk.STYLE_PROVIDER_PRIORITY_APPLICATION)
            self.window = MainWindow(self, settings, library)
        return self.window

    def do_activate(self):
        self._window().present()

    def do_open(self, files, _n, _hint):
        win = self._window()
        win.present()
        for f in files[:1]:
            path = f.get_path()
            if path:
                win.open_path(path)


def main(argv=None):
    GLib.set_prgname("pdf-reader")
    GLib.set_application_name(APP_NAME)
    return Application().run(argv if argv is not None else sys.argv)
