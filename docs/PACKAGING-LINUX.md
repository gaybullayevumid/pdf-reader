# Linux (.deb) paketi

Linux versiyasi `linux/` papkasida: **GTK 3 + Poppler** (Python). U Windows ilovasi bilan bir xil tarjima
fayllarini (`Localization/Strings/*.json`) ishlatadi.

## O'rnatish (Debian, Ubuntu, Mint ...)

```bash
sudo apt install ./pdf-reader_1.2.0_all.deb
pdf-reader file.pdf
```

`apt` kerakli paketlarni (`python3-gi`, `gir1.2-gtk-3.0`, `gir1.2-poppler-0.18`, ...) o'zi o'rnatadi.
O'chirish: `sudo apt remove pdf-reader`.

## Paketni yig'ish

Faqat `dpkg-deb` kerak. Windows'da Docker bilan:

```powershell
docker run --rm -v "${PWD}:/src" ubuntu:24.04 sh /src/linux/build-deb.sh
```

Linux'da: `sh linux/build-deb.sh`. Natija: `linux/dist/pdf-reader_<versiya>_all.deb`.

Versiya `linux/pdf_reader/__init__.py` dagi `__version__` dan olinadi (Windows versiyasi bilan bir xil saqlang).

## Sinab ko'rish

Paketni toza Ubuntu konteynerida o'rnatib, Xvfb ichida ishga tushirish mumkin:

```bash
docker run --rm -v "$PWD:/src" ubuntu:24.04 bash -c \
  "apt-get update -qq && apt-get install -y -qq dpkg-dev && sh /src/linux/build-deb.sh && \
   apt-get install -y -qq /src/linux/dist/*.deb xvfb && \
   xvfb-run -a pdf-reader /src/some.pdf"
```

## Release'ga qo'shish

```bash
gh release upload v1.2.0 linux/dist/pdf-reader_1.2.0_all.deb
```

Windows yangilagichi faqat `PDFReader-Setup-*.exe` faylini qidiradi, shuning uchun `.deb` uni buzmaydi.
Linux'da ilovaning o'zi yangilanmaydi; yangi `.deb` ni qayta o'rnating.
