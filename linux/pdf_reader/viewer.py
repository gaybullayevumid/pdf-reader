"""Continuous-scroll PDF view. Scrolling is virtual (own adjustments), so documents of any length work."""

import bisect
import math
from collections import OrderedDict

import cairo
import gi

gi.require_version("Gtk", "3.0")
gi.require_version("Gdk", "3.0")
from gi.repository import Gdk, GObject, Gtk  # noqa: E402

PX_PER_PT = 96.0 / 72.0
GAP = 12
MARGIN = 16
MIN_ZOOM, MAX_ZOOM = 0.1, 5.0
MAX_PAGE_PIXELS = 24_000_000
CACHE_BYTES = 320 * 1024 * 1024
WHEEL_STEP = 70


class PdfView(Gtk.Box):
    __gsignals__ = {
        "page-changed": (GObject.SignalFlags.RUN_FIRST, None, (int,)),
        "zoom-changed": (GObject.SignalFlags.RUN_FIRST, None, (float,)),
    }

    def __init__(self):
        super().__init__(orientation=Gtk.Orientation.HORIZONTAL)
        self.doc = None
        self.sizes = []          # (width, height) in points
        self.tops = []           # y of each page in px
        self.scale = PX_PER_PT   # px per point
        self.total_h = 0.0
        self.content_w = 0.0
        self.dark = False
        self.current = 0
        self._cache = OrderedDict()
        self._cache_bytes = 0
        self._fit = "width"      # re-fit on resize while True

        self.vadj = Gtk.Adjustment()
        self.hadj = Gtk.Adjustment()
        self.vadj.connect("value-changed", self._on_scrolled)
        self.hadj.connect("value-changed", lambda *_: self.area.queue_draw())

        self.area = Gtk.DrawingArea(hexpand=True, vexpand=True, can_focus=True)
        self.area.add_events(Gdk.EventMask.SCROLL_MASK | Gdk.EventMask.SMOOTH_SCROLL_MASK |
                             Gdk.EventMask.BUTTON_PRESS_MASK | Gdk.EventMask.BUTTON1_MOTION_MASK |
                             Gdk.EventMask.BUTTON_RELEASE_MASK)
        self.area.connect("draw", self._on_draw)
        self.area.connect("size-allocate", self._on_size)
        self.area.connect("scroll-event", self._on_scroll)
        self.area.connect("button-press-event", self._on_press)
        self.area.connect("motion-notify-event", self._on_drag)

        left = Gtk.Box(orientation=Gtk.Orientation.VERTICAL)
        left.pack_start(self.area, True, True, 0)
        self.hbar = Gtk.Scrollbar(orientation=Gtk.Orientation.HORIZONTAL, adjustment=self.hadj)
        left.pack_start(self.hbar, False, False, 0)
        self.pack_start(left, True, True, 0)
        self.vbar = Gtk.Scrollbar(orientation=Gtk.Orientation.VERTICAL, adjustment=self.vadj)
        self.pack_start(self.vbar, False, False, 0)
        self._drag = None

    # ---- document -------------------------------------------------------------------------------

    def set_document(self, doc, page=0):
        self._cache.clear()
        self._cache_bytes = 0
        self.doc = doc
        self.sizes = []
        if doc is not None:
            for i in range(doc.get_n_pages()):
                w, h = doc.get_page(i).get_size()
                self.sizes.append((max(w, 1.0), max(h, 1.0)))
        self._fit = "width"
        self.current = 0
        self._relayout()
        self.fit_width()
        self.goto_page(page)

    @property
    def page_count(self):
        return len(self.sizes)

    @property
    def zoom(self):
        return self.scale / PX_PER_PT

    # ---- layout ---------------------------------------------------------------------------------

    def _view(self):
        return max(self.area.get_allocated_width(), 1), max(self.area.get_allocated_height(), 1)

    def _relayout(self, keep_fraction=None):
        scale = self.scale
        self.tops, y = [], float(GAP)
        for _, h in self.sizes:
            self.tops.append(y)
            y += h * scale + GAP
        self.total_h = y
        widest = max((w for w, _ in self.sizes), default=0.0)
        self.content_w = widest * scale + 2 * MARGIN
        vw, vh = self._view()
        self.vadj.configure(self.vadj.get_value(), 0, self.total_h, 40, vh * 0.9, vh)
        self.hadj.configure(self.hadj.get_value(), 0, max(self.content_w, vw), 40, vw * 0.9, vw)
        self.hbar.set_visible(self.content_w > vw + 1)
        if keep_fraction is not None:
            self.vadj.set_value(keep_fraction * self.total_h)
        self.area.queue_draw()

    def set_zoom(self, zoom, fit=None):
        if not self.sizes:
            return
        zoom = min(max(zoom, MIN_ZOOM), MAX_ZOOM)
        self._fit = fit
        fraction = self.vadj.get_value() / self.total_h if self.total_h else 0
        self.scale = zoom * PX_PER_PT
        self._relayout(keep_fraction=fraction)
        self.emit("zoom-changed", self.zoom)

    def zoom_by(self, factor):
        self.set_zoom(self.zoom * factor)

    def fit_width(self):
        if not self.sizes:
            return
        vw, _ = self._view()
        widest = max(w for w, _ in self.sizes)
        avail = vw - 2 * MARGIN
        self.set_zoom(avail / widest / PX_PER_PT, fit="width")

    def fit_page(self):
        if not self.sizes:
            return
        vw, vh = self._view()
        w, h = self.sizes[self.current]
        z = min((vw - 2 * MARGIN) / w, (vh - 2 * GAP) / h) / PX_PER_PT
        self.set_zoom(z, fit="page")
        self.goto_page(self.current)

    def _on_size(self, *_):
        if not self.sizes:
            return
        if self._fit == "width":
            self.fit_width()
        elif self._fit == "page":
            self.fit_page()
        else:
            self._relayout(self.vadj.get_value() / self.total_h if self.total_h else None)

    # ---- navigation -----------------------------------------------------------------------------

    def goto_page(self, index):
        if not self.sizes:
            return
        index = min(max(index, 0), len(self.sizes) - 1)
        self.vadj.set_value(min(self.tops[index] - GAP, self.total_h - self.vadj.get_page_size()))
        self._update_current()

    def scroll_by(self, dy=0.0, dx=0.0):
        if dy:
            self.vadj.set_value(min(max(self.vadj.get_value() + dy, 0),
                                    max(self.total_h - self.vadj.get_page_size(), 0)))
        if dx:
            self.hadj.set_value(min(max(self.hadj.get_value() + dx, 0),
                                    max(self.hadj.get_upper() - self.hadj.get_page_size(), 0)))

    def page_at(self, y):
        return min(max(bisect.bisect_right(self.tops, y) - 1, 0), max(len(self.tops) - 1, 0))

    def _update_current(self):
        if not self.sizes:
            return
        probe = self.vadj.get_value() + self.vadj.get_page_size() / 3
        page = self.page_at(probe)
        if page != self.current:
            self.current = page
            self.emit("page-changed", page)

    def _on_scrolled(self, *_):
        self._update_current()
        self.area.queue_draw()

    # ---- input ----------------------------------------------------------------------------------

    def _on_scroll(self, _w, ev):
        if ev.state & Gdk.ModifierType.CONTROL_MASK:
            ok, _dx, dy = ev.get_scroll_deltas()
            if not ok:
                dy = {Gdk.ScrollDirection.UP: -1, Gdk.ScrollDirection.DOWN: 1}.get(ev.direction, 0)
            if dy:
                self.zoom_by(1.1 ** (-dy))
            return True
        ok, dx, dy = ev.get_scroll_deltas()
        if not ok:
            dx = {Gdk.ScrollDirection.LEFT: -1, Gdk.ScrollDirection.RIGHT: 1}.get(ev.direction, 0)
            dy = {Gdk.ScrollDirection.UP: -1, Gdk.ScrollDirection.DOWN: 1}.get(ev.direction, 0)
        if ev.state & Gdk.ModifierType.SHIFT_MASK and dy and not dx:
            dx, dy = dy, 0
        self.scroll_by(dy * WHEEL_STEP, dx * WHEEL_STEP)
        return True

    def _on_press(self, _w, ev):
        self.area.grab_focus()
        self._drag = (ev.x, ev.y, self.hadj.get_value(), self.vadj.get_value())
        return False

    def _on_drag(self, _w, ev):
        if self._drag:
            x, y, hx, vy = self._drag
            self.hadj.set_value(min(max(hx - (ev.x - x), 0), max(self.hadj.get_upper() - self.hadj.get_page_size(), 0)))
            self.vadj.set_value(min(max(vy - (ev.y - y), 0), max(self.total_h - self.vadj.get_page_size(), 0)))
        return True

    # ---- drawing --------------------------------------------------------------------------------

    def _surface(self, index, dpr):
        w, h = self.sizes[index]
        key = (index, round(self.scale, 4), dpr)
        hit = self._cache.get(key)
        if hit is not None:
            self._cache.move_to_end(key)
            return hit
        pw, ph = w * self.scale * dpr, h * self.scale * dpr
        shrink = min(1.0, math.sqrt(MAX_PAGE_PIXELS / max(pw * ph, 1)))
        pw, ph = max(int(math.ceil(pw * shrink)), 1), max(int(math.ceil(ph * shrink)), 1)
        surf = cairo.ImageSurface(cairo.FORMAT_ARGB32, pw, ph)
        # device scale lets us draw the surface at its logical size
        surf.set_device_scale(pw / (w * self.scale), ph / (h * self.scale))
        cr = cairo.Context(surf)
        cr.set_source_rgb(1, 1, 1)
        cr.paint()
        cr.scale(pw / w, ph / h)
        self.doc.get_page(index).render(cr)
        surf.flush()
        self._cache[key] = surf
        self._cache_bytes += pw * ph * 4
        while self._cache_bytes > CACHE_BYTES and len(self._cache) > 3:
            _, old = self._cache.popitem(last=False)
            self._cache_bytes -= old.get_width() * old.get_height() * 4
        return surf

    def _on_draw(self, widget, cr):
        vw, vh = self._view()
        bg = (0.13, 0.13, 0.14) if self.dark else (0.88, 0.89, 0.90)
        cr.set_source_rgb(*bg)
        cr.paint()
        if not self.sizes or self.doc is None:
            return False

        top = self.vadj.get_value()
        left = self.hadj.get_value()
        dpr = widget.get_scale_factor()
        first = self.page_at(top)
        for i in range(first, len(self.sizes)):
            y = self.tops[i] - top
            if y > vh:
                break
            w, h = self.sizes[i][0] * self.scale, self.sizes[i][1] * self.scale
            x = (max(self.content_w, vw) - w) / 2 - left
            # soft shadow, then the page
            cr.set_source_rgba(0, 0, 0, 0.25 if not self.dark else 0.5)
            cr.rectangle(x + 1, y + 2, w, h)
            cr.fill()
            try:
                surf = self._surface(i, dpr)
            except Exception:  # a broken page must not break the others
                cr.set_source_rgb(1, 1, 1)
                cr.rectangle(x, y, w, h)
                cr.fill()
                continue
            cr.set_source_surface(surf, round(x), round(y))
            cr.paint()
        return False

    def release(self):
        self.doc = None
        self.sizes = []
        self._cache.clear()
        self._cache_bytes = 0
        self.area.queue_draw()
