# Installer va paketlash

PDF Reader ikki ko'rinishda tarqatiladi:

| Paket | Fayl | Qachon kerak |
| --- | --- | --- |
| Installer | `PDFReader-Setup-<versiya>.exe` | Oddiy foydalanuvchilar uchun. Start menyu, ixtiyoriy ish stoli yorlig'i, "Open with" va o'chirish (Uninstall) bor. |
| Portativ ZIP | `PDFReader-<versiya>-win-x64-portable.zip` | O'rnatishsiz ishlatish yoki USB'dan. |

Ikkalasi ham **self-contained**: .NET va Windows App SDK ichiga joylangan, foydalanuvchi hech narsa o'rnatmaydi.

## 1. Talablar

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Inno Setup 6](https://jrsoftware.org/isdl.php) (odatiy joyga o'rnating; skript `ISCC.exe` ni o'zi topadi)

## 2. Paketlarni yig'ish

Repo ildizidan:

```powershell
# faqat installer
.\installer\build.ps1

# installer + portativ ZIP
.\installer\build.ps1 -Portable
```

Natija `installer\output\` papkasiga tushadi:

```
installer\output\PDFReader-Setup-1.1.0.exe
installer\output\PDFReader-1.1.0-win-x64-portable.zip
```

Skript ketma-ket quyidagilarni bajaradi:

1. `dotnet publish` — Release, `win-x64`, self-contained, ReadyToRun → `installer\publish\`
2. `ISCC.exe installer\PdfReader.iss` — `installer\publish\` ni bitta `Setup.exe` ga siqadi.
3. (`-Portable`) `installer\publish\` ni ZIP qiladi.

`installer\publish\` va `installer\output\` git'ga kiritilmaydi (`.gitignore`).

## 3. Yangi versiya chiqarish

1. `installer\PdfReader.iss` ichida versiyani o'zgartiring:
   ```
   #define AppVersion "1.2.0"
   ```
2. Paketlarni yig'ing (`.\installer\build.ps1 -Portable`).
3. Git tegi qo'ying:
   ```powershell
   git tag v1.2.0
   git push origin v1.2.0
   ```

`AppId` (GUID) ni **o'zgartirmang** — Windows shu orqali eski versiyani yangisi bilan almashtiradi.

## 4. GitHub Release sifatida e'lon qilish

[GitHub CLI](https://cli.github.com/) bilan:

```powershell
gh release create v1.2.0 `
  installer\output\PDFReader-Setup-1.2.0.exe `
  installer\output\PDFReader-1.2.0-win-x64-portable.zip `
  --title "PDF Reader 1.2.0" `
  --notes "Nimalar o'zgardi..."
```

Yoki veb orqali: GitHub → **Releases** → **Draft a new release** → teg tanlang → ikkala faylni sudrab tashlang → **Publish**.

README'dagi *Releases* havolasi shu sahifaga olib boradi.

## 5. Jim (silent) o'rnatish

IT / skriptlar uchun:

```powershell
# joriy foydalanuvchiga, oynasiz
PDFReader-Setup-1.1.0.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART

# ish stoli yorlig'i va "Open with" bilan
PDFReader-Setup-1.1.0.exe /VERYSILENT /TASKS="desktopicon,pdfassoc"

# boshqa papkaga
PDFReader-Setup-1.1.0.exe /VERYSILENT /DIR="D:\Apps\PDF Reader"

# barcha foydalanuvchilar uchun (administrator kerak)
PDFReader-Setup-1.1.0.exe /VERYSILENT /ALLUSERS
```

O'chirish: **Settings → Apps** orqali yoki `"<o'rnatilgan papka>\unins000.exe" /VERYSILENT`.

## 6. Boshqa arxitekturalar

Hozir faqat `win-x64` yig'iladi. ARM64 uchun `installer\build.ps1` ichidagi `-r win-x64 -p:Platform=x64` ni `-r win-arm64 -p:Platform=ARM64` ga almashtiring va `installer\PdfReader.iss` dagi `ArchitecturesAllowed` / `ArchitecturesInstallIn64BitMode` qiymatlarini mos ravishda o'zgartiring.

## 7. Imzolash (ixtiyoriy)

Imzosiz installer birinchi ishga tushirishda Windows SmartScreen ogohlantirishini ko'rsatishi mumkin. Buni oldini olish uchun kod imzolash sertifikatingiz bilan `Setup.exe` ni imzolang:

```powershell
signtool sign /fd SHA256 /tr http://timestamp.digicert.com /td SHA256 /f cert.pfx /p <parol> installer\output\PDFReader-Setup-1.2.0.exe
```

## Muammolarni hal qilish

| Muammo | Yechim |
| --- | --- |
| `Inno Setup 6 not found` | Inno Setup 6 ni o'rnating. |
| `dotnet publish failed` | `dotnet --list-sdks` da .NET 10 SDK borligini tekshiring. |
| Eski ikonka ko'rinyapti | Windows ikonka keshi: Explorer'ni qayta ishga tushiring. |
| Birinchi ishga tushish sekin | Normal: Windows App SDK birinchi marta yuklanadi. Keyingi safar tez. |
