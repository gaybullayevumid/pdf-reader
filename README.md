<p align="center">
  <img src="Assets/icon-1024.png" alt="PDF Reader" width="128" />
</p>

<h1 align="center">PDF Reader</h1>

<p align="center">
  Windows 11 uchun tez, sodda va chiroyli PDF o'quvchi.<br />
  WinUI 3 · .NET 10 · Windows App SDK
</p>

---

## Imkoniyatlar

- **PDF ochish** — fayl tanlash (`Ctrl+O`), oynaga sudrab tashlash, ikki marta bosish yoki *Open with → PDF Reader*.
- **Uzluksiz o'qish** — sahifalar faqat kerak bo'lganda chiziladi, shuning uchun minglab sahifali hujjatlar ham tez ochiladi.
- **Zoom** — `Ctrl` + sichqoncha g'ildiragi, `Ctrl` + `+` / `-`, `Ctrl+0` (kenglikka moslash), "Fit page" va 50–300% presetlar (maksimum 500%).
- **Sahifaga o'tish** — sahifa raqamini yozing; `Home` / `End` / `PageUp` / `PageDown` / strelkalar / `Space` ishlaydi.
- **Parolli PDF** — ochishda parol so'raladi.
- **Kutubxona** — *Recent*, *All PDFs*, *Favorites*; yulduzcha bilan sevimlilarga qo'shish.
- **Oxirgi sahifani eslab qoladi** — hujjat keyingi safar to'xtagan joyingizdan ochiladi.
- **Yorug' / qorong'i mavzu** (Settings'da).
- **Maxfiylik** — hujjatlar faqat kompyuteringizda qayta ishlanadi, internetga hech narsa yuborilmaydi.

Hozircha yo'q: matn bo'yicha qidirish va chop etish.

## O'rnatish

[Releases](../../releases) sahifasidan oxirgi versiyani yuklab oling:

| Fayl | Nima |
| --- | --- |
| `PDFReader-Setup-<versiya>.exe` | O'rnatuvchi (tavsiya etiladi). Administrator huquqi kerak emas. |
| `PDFReader-<versiya>-win-x64-portable.zip` | O'rnatishsiz: arxivni oching va `PdfReader.exe` ni ishga tushiring. |

Talablar: Windows 10 (1809) yoki yangiroq, 64-bit. .NET yoki boshqa komponentni alohida o'rnatish shart emas.

> Windows 11 default dasturni jimgina almashtirishga ruxsat bermaydi. PDF fayllarni doim shu ilovada ochish uchun:
> PDF'ga o'ng tugma → **Open with** → **Choose another app** → **PDF Reader** → **Always**.

## Loyihani yig'ish (dasturchilar uchun)

Kerak bo'ladi:

- Windows 10/11
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- *(installer uchun)* [Inno Setup 6](https://jrsoftware.org/isdl.php)

```powershell
# yig'ish
dotnet build -p:Platform=x64

# ishga tushirish
dotnet run -p:Platform=x64

# PDF bilan ishga tushirish
.\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\PdfReader.exe "C:\path\file.pdf"
```

Installer va portativ paket yaratish: [docs/PACKAGING.md](docs/PACKAGING.md).

## Tuzilma

```
.
├── PdfReader.csproj        # loyiha (unpackaged, self-contained WinUI 3)
├── App.xaml(.cs)           # kirish nuqtasi, DI konteyneri, ishga tushirish argumentlari
├── MainWindow.xaml(.cs)    # oyna, sidebar, drag & drop
├── Views/                  # HomePage, LibraryPage, SettingsPage, ViewerPage (PDF ko'rgich)
├── ViewModels/             # MVVM (CommunityToolkit.Mvvm)
├── Services/               # fayl ochish, navigatsiya, mavzu, sozlamalar, kutubxona
├── Database/               # SQLite (oxirgi fayllar, sevimlilar, oxirgi sahifa)
├── Models/ · Converters/
├── Assets/                 # ikonkalar
├── installer/              # Inno Setup skripti va build.ps1
└── docs/                   # hujjatlar
```

PDF render qilish uchun Windows'ning o'rnatilgan `Windows.Data.Pdf` moduli ishlatiladi, shuning uchun qo'shimcha native kutubxona kerak emas.

Foydalanuvchi ma'lumotlari: `%LOCALAPPDATA%\PdfReader` (baza, sozlamalar, loglar).

## Ikonka

Ikonka `installer/make_icon.py` bilan chiziladi (Pillow kerak):

```powershell
pip install pillow
python installer\make_icon.py
```
