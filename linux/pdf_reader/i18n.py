"""Translations. The JSON files are the same ones the Windows app uses (Localization/Strings)."""

import json
import locale
import os

LANGUAGES = [("en", "English"), ("uz", "O‘zbekcha"), ("ru", "Русский")]
CODES = [c for c, _ in LANGUAGES]

_HERE = os.path.dirname(os.path.abspath(__file__))
_catalogs = {}
_current = "en"


def _strings_dir():
    for d in (os.environ.get("PDFREADER_STRINGS"),
              os.path.join(_HERE, "strings"),
              os.path.join(_HERE, "..", "..", "Localization", "Strings")):
        if d and os.path.isdir(d):
            return d
    return os.path.join(_HERE, "strings")


def _load(code):
    if code not in _catalogs:
        path = os.path.join(_strings_dir(), f"{code}.json")
        try:
            with open(path, encoding="utf-8-sig") as f:
                _catalogs[code] = json.load(f)
        except (OSError, ValueError):
            _catalogs[code] = {}
    return _catalogs[code]


def system_language():
    """The desktop language when supported, otherwise English."""
    for var in ("LC_ALL", "LC_MESSAGES", "LANGUAGE", "LANG"):
        value = os.environ.get(var)
        if value:
            prefix = value.split(":")[0].replace("-", "_").split("_")[0].lower()
            if prefix in CODES:
                return prefix
            if prefix not in ("", "c", "posix"):
                return "en"
    try:
        prefix = (locale.getlocale()[0] or "").split("_")[0].lower()
    except ValueError:
        prefix = ""
    return prefix if prefix in CODES else "en"


def resolve(code):
    return code if code in CODES else system_language()


def set_language(code):
    global _current
    _current = resolve(code)
    return _current


def current():
    return _current


def tr(key, *args):
    text = _load(_current).get(key) or _load("en").get(key) or key
    for i, a in enumerate(args):
        text = text.replace("{%d}" % i, str(a))
    return text
