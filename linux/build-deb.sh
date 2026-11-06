#!/bin/sh
# Builds dist/pdf-reader_<version>_all.deb. Needs only dpkg-deb; run it on Linux or in Docker (see docs/PACKAGING-LINUX.md).
set -eu
cd "$(dirname "$0")"
ROOT=$(pwd)
VERSION=$(sed -n 's/^__version__ = "\(.*\)"/\1/p' pdf_reader/__init__.py)
PKG=pdf-reader
STAGE=$(mktemp -d)
trap 'rm -rf "$STAGE"' EXIT

install -d "$STAGE/usr/bin" "$STAGE/usr/lib/$PKG" "$STAGE/usr/share/applications" \
           "$STAGE/usr/share/doc/$PKG" "$STAGE/DEBIAN"

cp -r pdf_reader "$STAGE/usr/lib/$PKG/"
find "$STAGE" -name __pycache__ -prune -exec rm -rf {} +
# the translations are shared with the Windows app
mkdir -p "$STAGE/usr/lib/$PKG/pdf_reader/strings"
cp ../Localization/Strings/*.json "$STAGE/usr/lib/$PKG/pdf_reader/strings/"

install -m 755 packaging/pdf-reader "$STAGE/usr/bin/pdf-reader"
install -m 644 packaging/pdf-reader.desktop "$STAGE/usr/share/applications/pdf-reader.desktop"
for d in packaging/icons/*; do
  size=$(basename "$d")
  install -D -m 644 "$d/pdf-reader.png" "$STAGE/usr/share/icons/hicolor/$size/apps/pdf-reader.png"
done
cp ../CHANGELOG.md "$STAGE/usr/share/doc/$PKG/CHANGELOG.md"
gzip -9n "$STAGE/usr/share/doc/$PKG/CHANGELOG.md"
cat > "$STAGE/usr/share/doc/$PKG/changelog" <<DCL
$PKG ($VERSION) unstable; urgency=medium

  * Release $VERSION. See CHANGELOG.md.

 -- gaybullayevumid <umidgaybullayev955@gmail.com>  $(date -R)
DCL
gzip -9n "$STAGE/usr/share/doc/$PKG/changelog"
printf 'Copyright (c) gaybullayevumid
Homepage: https://github.com/gaybullayevumid/pdf-reader
' > "$STAGE/usr/share/doc/$PKG/copyright"

find "$STAGE/usr" -type d -exec chmod 755 {} +
find "$STAGE/usr/lib" "$STAGE/usr/share" -type f -exec chmod 644 {} +

SIZE=$(du -sk "$STAGE/usr" | cut -f1)
cat > "$STAGE/DEBIAN/control" <<CTRL
Package: $PKG
Version: $VERSION
Section: graphics
Priority: optional
Architecture: all
Depends: python3 (>= 3.8), python3-gi, python3-gi-cairo, gir1.2-gtk-3.0, gir1.2-poppler-0.18, shared-mime-info
Recommends: adwaita-icon-theme
Installed-Size: $SIZE
Maintainer: gaybullayevumid <umidgaybullayev955@gmail.com>
Homepage: https://github.com/gaybullayevumid/pdf-reader
Description: Fast, simple PDF reader (GTK)
 Continuous scrolling, zoom, password-protected documents, a recent files
 and favorites library, English / Uzbek / Russian interface, full screen and
 light or dark theme. Documents are processed locally.
CTRL
cat > "$STAGE/DEBIAN/postinst" <<'POST'
#!/bin/sh
set -e
command -v update-desktop-database >/dev/null 2>&1 && update-desktop-database -q /usr/share/applications || true
command -v gtk-update-icon-cache >/dev/null 2>&1 && gtk-update-icon-cache -q -t -f /usr/share/icons/hicolor || true
exit 0
POST
chmod 755 "$STAGE/DEBIAN/postinst"

mkdir -p "$ROOT/dist"
OUT="$ROOT/dist/${PKG}_${VERSION}_all.deb"
dpkg-deb --root-owner-group --build "$STAGE" "$OUT" >/dev/null
echo "$OUT"
