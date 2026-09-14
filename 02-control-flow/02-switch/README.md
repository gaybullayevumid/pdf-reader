# Switch (Tanlash operatori)

Dasturlashda ko'pincha bitta o'zgaruvchining aniq bir qiymatiga qarab bir nechta yo'llardan birini tanlash kerak bo'ladi. Masalan:
- **Svetofor chirog'iga qarab:** qizil — to'xtash, sariq — tayyorlanish, yashil — harakatlanish;
- **Bankomat yoki USSD menyusida:** 1 — Balansni tekshirish, 2 — Pul yechish, 3 — Parolni o'zgartirish;
- **Hafta kuniga qarab:** dushanba — ish haftasi boshi, juma — hafta yakuni, yakshanba — dam olish kuni;
- **Foydalanuvchi roliga qarab:** "admin", "moderator", "mijoz" yoki "mehmon".

Bunday holatlarda ketma-ket bir nechta `if...else if...else if` yozish kodni uzun, chalkash va o'qishga noqulay qilib yuboradi. Bitta o'zgaruvchini aniq variantlar bilan solishtirish uchun maxsus **`switch...case`** operatori ishlatiladi.

`switch` so'zi inglizchadan tarjima qilinganda **"ulagich"**, **"yo'nalishni o'zgartirish"**, `case` esa **"holat"**, **"vaziyat"** degan ma'nolarni anglatadi.

---

## switch operatorining tuzilishi

`switch` operatori qavs ichidagi qiymatni oladi va uni tartib bilan `case`larda ko'rsatilgan qiymatlar bilan qat'iy tenglik (`===`) orqali solishtirib chiqadi. Mos kelgan birinchi `case`dagi kod bajariladi.

**Sintaksis:**

```js
switch (ifoda) {
  case qiymat1:
    // ifoda === qiymat1 bo'lsa, shu yerdagi kod bajariladi
    break;

  case qiymat2:
    // ifoda === qiymat2 bo'lsa, shu yerdagi kod bajariladi
    break;

  case qiymat3:
    // ifoda === qiymat3 bo'lsa, shu yerdagi kod bajariladi
    break;

  default:
    // Hech bir case mos kelmasa, shu yerdagi kod bajariladi
}
```

---

## Qadamma-qadam ishlash sxemasi

`switch` qanday ishlashini quyidagi sxema orqali yaqqol ko'rish mumkin:

```text
[Boshlanish] ───► [Ifoda hisoblanadi: switch(qiymat)]
                            │
        ┌───────────────────┼───────────────────┐
        ▼                   ▼                   ▼
  case 1 bilan        case 2 bilan        Moslik yo'qmi?
  solishtiriladi      solishtiriladi            │
   (=== rostmi?)       (=== rostmi?)            ▼
        │                   │             [default bloki]
        ▼                   ▼                   │
  [1-kod bloki]       [2-kod bloki]             ▼
        │                   │             [Tugallanadi]
        ▼                   ▼
     [break]             [break]
        │                   │
        └───────────────────┴───────────► [switch yakunlanadi!]
```

1. Avval `switch(ifoda)` qavs ichidagi ifodaning qiymati hisoblanadi.
2. Dastur yuqoridan pastga qarab har bir `case` qiymatini tekshiradi (`ifoda === case_qiymat`).
3. Mos kelgan `case` topilishi bilan o'sha blokdagi kod bajariladi.
4. `break` buyrug'i kelganda, `switch` o'z ishini to'xtatadi va qolgan tekshiruvlarga kirmasdan tashqariga chiqadi.
5. Agar birorta ham `case` mos kelmasa, oxiridagi `default` bloki (agar yozilgan bo'lsa) ishga tushadi.

---

## switch operatorining 4 ta oltin ustuni

Har bir to'g'ri tashkil qilingan `switch` konstruksiyasi 4 ta asosiy qismga tayanadi:

1. **Ifoda (`switch(qiymat)`):** Taqqoslash uchun tekshirilayotgan asosiy o'zgaruvchi yoki ifoda.
2. **Variantlar (`case qiymat:`):** O'zgaruvchi teng bo'lishi mumkin bo'lgan aniq qiymat.
3. **To'xtatuvchi (`break;`):** Kerakli kod bajarilgach, dasturni `switch`dan darhol olib chiqib ketuvchi kalit so'z.
4. **Zaxira bloki (`default:`):** Agar hech qaysi variant mos kelmasa, xatolikning oldini oluvchi yoki umumiy holatni bildiruvchi blok (xuddi `else` kabi).

---

## Oddiy misol: Hafta kunini aniqlash

```js
let kun = 3; // 1. Tekshiriladigan qiymat

switch (kun) {
  case 1:
    console.log("Dushanba");
    break;
  case 2:
    console.log("Seshanba");
    break;
  case 3:
    console.log("Chorshanba");
    break; // 3. Moslik topildi, kod bajariladi va chiqib ketadi
  case 4:
    console.log("Payshanba");
    break;
  case 5:
    console.log("Juma");
    break;
  default: // 4. Zaxira varianti
    console.log("Dam olish kuni yoki noto'g'ri raqam");
}

console.log("Dastur davom etmoqda...");
```

**Natija:**
```
Chorshanba
Dastur davom etmoqda...
```

### Kod qadamlari tahlili:

| Qadam | Tekshirilayotgan holat | Natija | Dasturning harakati |
|---|---|---|---|
| 1 | `kun === 1` (`3 === 1`) | `false` | Keyingi case ga o'tadi |
| 2 | `kun === 2` (`3 === 2`) | `false` | Keyingi case ga o'tadi |
| 3 | `kun === 3` (`3 === 3`) | `true` | `"Chorshanba"` konsolga chiqariladi |
| 4 | `break;` | — | `switch` blokidan butunlay chiqib ketadi |

---

## Eng xavfli xato: break ni unutish (Fall-through effekti)

`switch` bilan ishlashda eng keng tarqalgan xato — har bir `case` oxiriga `break` yozishni unutib qoldirishdir.

Agar `break` yozilmasa, dastur mos kelgan `case`ni bajargach to'xtamaydi, balki keyingi `case`larning shartini **tekshirib ham o'tirmasdan**, ularning ichidagi kodlarni ham ketma-ket bajarib ketaveradi. Bunga dasturlashda **Fall-through** (pastga tushib ketish) deyiladi.

**Xato kod (break tashlab ketilgan):**

```js
let buyruq = "start";

switch (buyruq) {
  case "start":
    console.log("Tizim ishga tushdi.");
    // break YO'Q!
  case "stop":
    console.log("Tizim to'xtatildi.");
    // break YO'Q!
  default:
    console.log("Noma'lum buyruq.");
}
```

**Natija:**
```
Tizim ishga tushdi.
Tizim to'xtatildi.
Noma'lum buyruq.
```

> **Eslatma:** Dastur `"start"` ni topdi, lekin `break` bo'lmagani sababli pastga qarab oqib, `"stop"` va `default` dagi kodlarni ham keraksiz ravishda bajardi. Shuning uchun har doim `case` so'ngida `break` qo'yishni unutmang!

---

## switch faqat qat'iy tenglik (`===`) bilan tekshiradi!

`switch` taqqoslashda doimo qat'iy tenglik (`===`) ishlatadi. Bu shuni anglatadiki, nafaqat qiymat, balki **ma'lumot turi (type)** ham aynan bir xil bo'lishi shart!

```js
let tanlov = "2"; // Matn (String) ko'rinishida kelgan

switch (tanlov) {
  case 2: // Son (Number) ko'rinishida yozilgan
    console.log("Ikki raqami tanlandi.");
    break;
  default:
    console.log("Mos kelmadi!");
}
```

**Natija:**
```
Mos kelmadi!
```

Chunki `"2" === 2` sharti yolg'on (`false`). Agar foydalanuvchidan matn kelsa, uni avval songa o'tkazish (`Number(tanlov)`) yoki `case`da `"2"` ko'rinishida yozish kerak.

---

## Bir nechta case uchun bitta umumiy blok (Guruhlash)

Ba'zida bir nechta turli xil qiymatlar uchun aynan bir xil vazifani bajarish talab etiladi. Buning uchun `case`lar orasiga `break` qo'ymasdan ketma-ket yozish (guruhlash) juda qulaydir:

```js
let oy = "iyul";

switch (oy) {
  case "dekabr":
  case "yanvar":
  case "fevral":
    console.log("Fasl: Qish ❄️");
    break;

  case "mart":
  case "aprel":
  case "may":
    console.log("Fasl: Bahor 🌸");
    break;

  case "iyun":
  case "iyul":
  case "avgust":
    console.log("Fasl: Yoz ☀️");
    break;

  case "sentyabr":
  case "oktyabr":
  case "noyabr":
    console.log("Fasl: Kuz 🍂");
    break;

  default:
    console.log("Bunday oy mavjud emas!");
}
```

**Natija:**
```
Fasl: Yoz ☀️
```

---

## switch (true) uslubi: Oraliqlar va shartlarni tekshirish

Odatda `switch` aniq bitta qiymatni tekshirish uchun mo'ljallangan. Ammo ko'pchilik bilmaydigan ajoyib bir usul bor: `switch` ga `true` berish orqali unda oraliqlarni (`>`, `<`, `>=`, `<=`) ham chiroyli tekshirish mumkin:

```js
let ball = 85;

switch (true) {
  case ball >= 90:
    console.log("Baho: 5 (A'lo)");
    break;
  case ball >= 70:
    console.log("Baho: 4 (Yaxshi)");
    break;
  case ball >= 60:
    console.log("Baho: 3 (Qoniqarli)");
    break;
  default:
    console.log("Baho: 2 (Qoniqarsiz)");
}
```

**Natija:**
```
Baho: 4 (Yaxshi)
```

Bu usulda dastur qaysi `case`dagi ifoda `true` bo'lsa, o'sha blokni ishga tushiradi.

---

## switch ichida o'zgaruvchi e'lon qilish va blok `{}` chegarasi

Agar biror `case` ichida `let` yoki `const` orqali yangi o'zgaruvchi ochmoqchi bo'lsangiz, albatta jingalak qavs `{}` (blok) ishlatishingiz zarur. Aks holda nomlar to'qnashuvi yuzaga keladi:

```js
let amal = "qo'shish";

switch (amal) {
  case "qo'shish": {
    let natija = 10 + 20; // {} bo'lgani uchun natija faqat shu blokda mavjud
    console.log("Natija: " + natija);
    break;
  }
  case "ayirish": {
    let natija = 30 - 10; // Xatolik bermaydi, chunki o'z blokida
    console.log("Natija: " + natija);
    break;
  }
}
```

---

## switch va if-else taqqoslashi

Ikkala vosita ham shart tekshirish uchun ishlatiladi. Lekin qaysi birini qachon tanlash kerak?

| Xususiyat | `switch` | `if-else` |
|---|---|---|
| **Taqqoslanadigan ifoda** | Odatda bitta o'zgaruvchining aniq qiymatlari | Har xil murakkab mantiqiy ifodalar (`&&`, `\|\|`, `<`, `>`) |
| **Tekshirish turi** | Qat'iy tenglik (`===`) | Istalgan shart natijasi (`true` / `false`) |
| **O'qilish qulayligi** | Variantlar 3 tadan ko'p bo'lsa juda toza va tartibli | Ko'p shartlarda chuqur chalkashlik (spaghetti kod) yuzaga keladi |
| **Qachon tanlash kerak?** | Aniq menyular, rollar, holatlar, kunlar, buyruqlarda | Chegaralar, oraliqlar yoki bir nechta turli o'zgaruvchilar solishtirilganda |

---

## Amaliy misol 1: Bankomat (USSD menyu) simulyatsiyasi

Foydalanuvchi menyudan raqam tanlaydi va shunga mos xizmat ko'rsatiladi:

```js
let buyruqRaqami = 2;
let hisobdagiMablag = 500000;

switch (buyruqRaqami) {
  case 1:
    console.log("Sizning balansingiz: " + hisobdagiMablag + " so'm.");
    break;

  case 2: {
    let yechiladiganPul = 100000;
    if (yechiladiganPul <= hisobdagiMablag) {
      hisobdagiMablag -= yechiladiganPul;
      console.log(yechiladiganPul + " so'm yechildi. Qolgan mablag': " + hisobdagiMablag + " so'm.");
    } else {
      console.log("Mablag' yetarli emas!");
    }
    break;
  }

  case 3:
    console.log("SMS-xabarnoma xizmati muvaffaqiyatli yoqildi.");
    break;

  case 0:
    console.log("Dasturdan chiqildi. Salomat bo'ling!");
    break;

  default:
    console.log("Noto'g'ri menyu tanlandi. Qaytadan urinib ko'ring.");
}
```

**Natija:**
```
100000 so'm yechildi. Qolgan mablag': 400000 so'm.
```

---

## Amaliy misol 2: Foydalanuvchi roli bo'yicha ruxsat berish tizimi

Turli xil tizimlarda foydalanuvchilar o'z darajasiga qarab tizimdan foydalana oladi:

```js
let foydalanuvchiRoli = "moderator";

switch (foydalanuvchiRoli) {
  case "admin":
    console.log("To'liq ruxsat: Tizimni o'chirish, tahrirlash va ko'rish mumkin.");
    break;

  case "moderator":
    console.log("Cheklangan ruxsat: Faqat postlarni tahrirlash va tasdiqlash mumkin.");
    break;

  case "foydalanuvchi":
    console.log("Oddiy ruxsat: Faqat ma'lumotlarni o'qish va izoh qoldirish mumkin.");
    break;

  case "mehmon":
    console.log("Faqat o'qish huquqi: Tizimdan to'liq foydalanish uchun ro'yxatdan o'ting.");
    break;

  default:
    console.log("Noma'lum foydalanuvchi turi. Kirish taqiqlanadi!");
}
```

**Natija:**
```
Cheklangan ruxsat: Faqat postlarni tahrirlash va tasdiqlash mumkin.
```

---

## Amaliy misol 3: Do'kon promokodi va chegirma hisoblagich

Har xil promokodlar kiritilganda tovarning yakuniy narxini hisoblaymiz:

```js
let mahsulotNarxi = 200000;
let promokod = "BAHOR20";
let chegirmaFoizi = 0;

switch (promokod) {
  case "START10":
    chegirmaFoizi = 10;
    break;

  case "BAHOR20":
    chegirmaFoizi = 20;
    break;

  case "SUPERVIP":
    chegirmaFoizi = 30;
    break;

  default:
    chegirmaFoizi = 0;
    console.log("Bunday promokod mavjud emas yoki muddati o'tgan.");
}

let chegirmaSummasi = (mahsulotNarxi * chegirmaFoizi) / 100;
let yakuniyNarx = mahsulotNarxi - chegirmaSummasi;

console.log("Chegirma foizi: " + chegirmaFoizi + "%");
console.log("To'lanadigan yakuniy summa: " + yakuniyNarx + " so'm.");
```

**Natija:**
```
Chegirma foizi: 20%
To'lanadigan yakuniy summa: 160000 so'm.
```

---

## Amaliy misol 4: Mini kalkulyator

```js
let a = 12;
let b = 4;
let amal = "/";

switch (amal) {
  case "+":
    console.log("Natija: " + (a + b));
    break;

  case "-":
    console.log("Natija: " + (a - b));
    break;

  case "*":
    console.log("Natija: " + (a * b));
    break;

  case "/":
    if (b === 0) {
      console.log("Xato: Nolga bo'lish mumkin emas!");
    } else {
      console.log("Natija: " + (a / b));
    }
    break;

  default:
    console.log("Noma'lum arifmetik amal kiritildi!");
}
```

**Natija:**
```
Natija: 3
```

---

## Xulosa

| Konstruksiya | Vazifasi | E'tibor berish kerak bo'lgan jihati |
|---|---|---|
| `switch (ifoda)` | Taqqoslash uchun asosiy ifodani qabul qiladi | Faqat qat'iy tenglik (`===`) orqali taqqoslaydi |
| `case qiymat:` | Tekshirilayotgan aniq variant | Qiymat turi ifoda turi bilan bir xil bo'lishi kerak |
| `break;` | Mos blok bajarilgach, switch'dan chiqaradi | Agar yozilmasa, keyingi case'lar ham avtomatik ishlab ketadi (Fall-through) |
| `default:` | Hech qaysi case to'g'ri kelmaganda ishlaydi | Oxirida yoziladi va unga `break` shart emas |
| Guruhlangan `case`lar | Bir nechta holat uchun bitta kod bajarish | `case`lar ketma-ket yozilib, eng oxirgisiga bitta umumiy `break` qo'yiladi |

---

## Mashqlar

**Oson darajali**

1. `kun` o'zgaruvchisi berilgan (`"dushanba"`, `"seshanba"` va hokazo). `switch` yordamida hafta kunining tartib raqamini chiqaring (dushanba — 1, seshanba — 2, ..., yakshanba — 7).
2. `fasl` o'zgaruvchisi berilgan (`"bahor"`, `"yoz"`, `"kuz"`, `"qish"`). Har bir faslga mos o'rtacha haroratni chiqaring.
3. `rang` o'zgaruvchisi berilgan. `switch` yordamida rangning inglizcha nomini chiqaring (`"qizil"` → `"red"`, `"yashil"` → `"green"`, `"ko'k"` → `"blue"`).
4. `toifa` o'zgaruvchisi berilgan (`"A"`, `"B"`, `"C"`, `"D"`, `"F"`). Har bir toifaga mos baho tavsifini chiqaring.
5. `oy` raqami berilgan (1–12). `switch` yordamida oyning nomini chiqaring.
6. Svetofor (`"yashil"`, `"sariq"`, `"qizil"`). Har bir holatga mos harakat xabarini chiqaring. Noto'g'ri qiymat uchun `"Noma'lum signal"` deb chiqaring.
7. `amal` o'zgaruvchisi berilgan (`"qo'shish"`, `"ayirish"`, `"ko'paytirish"`, `"bo'lish"`). Ikkita son (`a` va `b`) bilan tegishli amalni bajaring.

**O'rta darajali**

8. `oy` raqami berilgan (1–12). Guruhlangan `case`lar yordamida qaysi faslga tegishli ekanini chiqaring:
   - 12, 1, 2 — Qish
   - 3, 4, 5 — Bahor
   - 6, 7, 8 — Yoz
   - 9, 10, 11 — Kuz
9. `yulduzcha` belgisi berilgan (`"♈"`, `"♉"`, `"♊"` va hokazo). `switch` yordamida burj nomini chiqaring.
10. `tilKodi` o'zgaruvchisi berilgan (`"uz"`, `"ru"`, `"en"`). Salomlashish xabarini mos tilda chiqaring (`"Salom"`, `"Привет"`, `"Hello"`).
11. `kartaTuri` o'zgaruvchisi berilgan (`"visa"`, `"mastercard"`, `"humo"`, `"uzcard"`). Har bir karta turiga mos tavsif va to'lov komissiyasini chiqaring.
12. `statusKodi` berilgan (200, 400, 403, 404, 500). `switch` yordamida HTTP xato yoki muvaffaqiyat mazmunini chiqaring.
13. `transport` o'zgaruvchisi berilgan (`"avtomobil"`, `"avtobus"`, `"piyoda"`, `"velosiped"`). Har biriga mos o'rtacha tezlikni chiqaring.
14. `birlik` o'zgaruvchisi berilgan (`"km"`, `"m"`, `"sm"`). Berilgan masofani millimetrlarga o'tkazing.

**Murakkab darajali**

15. Matnli buyruqlar simulyatsiyasi: `buyruq` o'zgaruvchisi berilgan (`"yangi"`, `"saqlash"`, `"ochish"`, `"yopish"`, `"chiqish"`). Har bir buyruqqa mos jarayon xabarini chiqaring. Noto'g'ri buyruq uchun `"Noma'lum buyruq"` deb chiqaring.
16. O'yin qahramoni boshqaruvi: `tugma` berilgan (`"w"`, `"s"`, `"a"`, `"d"`, `"space"`). Har bir tugmaga mos harakatni yozing (`"oldinga"`, `"orqaga"`, `"chapga"`, `"o'ngga"`, `"sakrash"`).
17. Do'kon chegirma tizimi: `toifaId` berilgan (1 — Elektronika, 2 — Kiyim, 3 — Oziq-ovqat, 4 — Sport). Toifaga mos chegirma foizini aniqlang va berilgan tovar narxidan chegirmani hisoblang.
18. Bank tranzaksiyasi: `operatsiyaTuri` berilgan (`"kirim"`, `"chiqim"`, `"o'tkazma"`, `"konvertatsiya"`). Har bir operatsiyaga mos komissiya foizini aniqlang va hisoblab chiqaring.
19. `switch (true)` uslubidan foydalanib, harorat (`gradus`) bo'yicha tavsiya bering: `< 0` — "Juda sovuq, issiq kiyining", `0 - 15` — "Salqin", `16 - 25` — "Havo ajoyib", `> 25` — "Issiq, ko'proq suv iching".
20. Til o'rganish platformasi: `daraja` berilgan (`"A1"`, `"A2"`, `"B1"`, `"B2"`, `"C1"`, `"C2"`). Har bir darajaga mos lug'at boyligi hajmi va tavsiya etilgan mashg'ulot soatlarini chiqaring.
