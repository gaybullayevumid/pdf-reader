"""Settings and library, stored as JSON under the XDG directories. Writes are atomic."""

import json
import os
import time


def _dir(env, fallback):
    base = os.environ.get("PDFREADER_DATA_DIR")
    if base:
        return base
    root = os.environ.get(env) or os.path.expanduser(fallback)
    return os.path.join(root, "pdf-reader")


def config_dir():
    return _dir("XDG_CONFIG_HOME", "~/.config")


def data_dir():
    return _dir("XDG_DATA_HOME", "~/.local/share")


def _read(path, default):
    try:
        with open(path, encoding="utf-8") as f:
            return json.load(f)
    except (OSError, ValueError):
        return default


def _write(path, data):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    tmp = path + ".tmp"
    with open(tmp, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
    os.replace(tmp, path)


class Settings:
    DEFAULTS = {"theme": "system", "language": "", "remember_window": True,
                "window": {"width": 1100, "height": 760, "maximized": False, "fullscreen": False}}

    def __init__(self):
        self.path = os.path.join(config_dir(), "settings.json")
        data = _read(self.path, {})
        self.data = {**self.DEFAULTS, **(data if isinstance(data, dict) else {})}
        window = {**self.DEFAULTS["window"], **(self.data.get("window") or {})}
        window["width"] = min(max(int(window["width"] or 0), 640), 10000)
        window["height"] = min(max(int(window["height"] or 0), 420), 10000)
        self.data["window"] = window

    def __getitem__(self, key):
        return self.data[key]

    def __setitem__(self, key, value):
        self.data[key] = value
        self.save()

    def save(self):
        try:
            _write(self.path, self.data)
        except OSError:
            pass


class Library:
    """Recent documents, favorites and the last page read."""

    MAX = 200

    def __init__(self):
        self.path = os.path.join(data_dir(), "library.json")
        items = _read(self.path, [])
        self.items = [i for i in items if isinstance(i, dict) and i.get("path")]

    def save(self):
        try:
            _write(self.path, self.items)
        except OSError:
            pass

    def get(self, path):
        return next((i for i in self.items if i["path"] == path), None)

    def touch(self, path, size):
        item = self.get(path)
        if item is None:
            item = {"path": path, "favorite": False, "page": 0}
            self.items.append(item)
        item["opened"] = time.time()
        item["size"] = size
        self.items.sort(key=lambda i: i.get("opened", 0), reverse=True)
        del self.items[self.MAX:]
        self.save()
        return item

    def set_page(self, path, page):
        item = self.get(path)
        if item is not None and item.get("page") != page:
            item["page"] = page
            self.save()

    def toggle_favorite(self, path):
        item = self.get(path)
        if item is not None:
            item["favorite"] = not item.get("favorite", False)
            self.save()

    def remove(self, path):
        self.items = [i for i in self.items if i["path"] != path]
        self.save()
