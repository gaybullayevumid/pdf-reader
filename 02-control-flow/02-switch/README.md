# Switch (Tanlash operatori)

Dasturlashda bitta o'zgaruvchining qiymatiga qarab turli xil ishlar bajarish kerak bo'lganda `switch` operatori ishlatiladi. Bu ko'p sonli `else if` zanjiri o'rniga yanada aniq va o'qishga qulay yozuv usulini taqdim etadi.

---

## switch operatori

`switch` kalit so'zi berilgan ifodani bir nechta `case` qiymati bilan solishtiradi va mos kelgan blokni bajaradi.

**Sintaksis:**

```js
switch (ifoda) {
  case qiymat1:
    // ifoda === qiymat1 bo'lsa shu yerga kiradi
    break;
  case qiymat2:
    // ifoda === qiymat2 bo'lsa shu yerga kiradi
    break;
  default:
    // hech bir case mos kelmasa shu yerga kiradi
}
```

**Misol:**

```js
let kun = "dushanba";

switch (kun) {
  case "dushanba":
    console.log("Hafta boshlandi.");
    break;
  case "juma":
    console.log("Hafta oxirlayapti.");
    break;
  default:
    console.log("Oddiy ish kuni.");
}
```

`kun === "dushanba"` bo'lgani uchun birinchi `case` mos keladi va `"Hafta boshlandi."` chiqadi.

---

## break kalit so'zi

`break` joriy `case` bajarilgandan so'ng `switch` bloklarini to'xtatadi. Agar `break` yozilmasa, mos kelgan `case`dan keyin barcha qolgan `case`lar ham bajariladi — bu xato natijalarga olib kelishi mumkin.

**break bo'lmasa nima bo'ladi:**

```js
let rang = "yashil";

switch (rang) {
  case "yashil":
    console.log("Yo'l bering.");
  case "sariq":
    console.log("Ehtiyot bo'ling.");
  case "qizil":
    console.log("To'xtang.");
}
```

Natija:
```
Yo'l bering.
Ehtiyot bo'ling.
To'xtang.
```

`"yashil"` case topildi, lekin `break` yo'qligi sababli keyingi barcha `case`lar ham bajarildi. Shuning uchun har doim `break` yozish tavsiya etiladi.

---

## default bloki

`default` — hech bir `case` mos kelmagan holatda ishlaydigan blok. `if-else` dagi `else` ga o'xshaydi. `default` har doim oxirida yoziladi, lekin majburiy emas.

**Misol:**

```js
let fasl = "yoz";

switch (fasl) {
  case "bahor":
    console.log("Havo iliq.");
    break;
  case "yoz":
    console.log("Havo issiq.");
    break;
  case "kuz":
    console.log("Barglar to'kilmoqda.");
    break;
  case "qish":
    console.log("Havo sovuq.");
    break;
  default:
    console.log("Noma'lum fasl.");
}
```

`fasl === "yoz"` bo'lgani uchun `"Havo issiq."` chiqadi.

---

## Bir nechta case uchun umumiy blok

Ba'zan bir nechta `case` uchun bir xil kod bajarish kerak bo'ladi. Buning uchun `break` siz ketma-ket `case`lar yoziladi.

**Misol:**

```js
let kun = "shanba";

switch (kun) {
  case "shanba":
  case "yakshanba":
    console.log("Dam olish kuni.");
    break;
  default:
    console.log("Ish kuni.");
}
```

`"shanba"` yoki `"yakshanba"` bo'lsa, ikkala `case` ham bir xil blokga kiradi va `"Dam olish kuni."` chiqadi.

---

## switch va if-else farqi

| Xususiyat | `switch` | `if-else` |
|---|---|---|
| Taqqoslash turi | Faqat `===` (qat'iy tenglik) | Har qanday shart (`>`, `<`, `&&` va h.k.) |
| Qo'llanish holati | Bitta o'zgaruvchi ko'p qiymat bilan solishtirilganda | Murakkab va turli shartlarda |
| O'qish qulayligi | Ko'p `case` bo'lsa aniqroq | Oz shart bo'lsa yaxshiroq |

> `switch` faqat `===` operatori bilan taqqoslaydi. Bu degani `"5" === 5` noto'g'ri hisoblanadi.

---

## Amaliy misol: Kalkulyator

```js
let a = 10;
let b = 4;
let amal = "*";

switch (amal) {
  case "+":
    console.log(a + b);
    break;
  case "-":
    console.log(a - b);
    break;
  case "*":
    console.log(a * b);
    break;
  case "/":
    if (b === 0) {
      console.log("Nolga bo'lish mumkin emas.");
    } else {
      console.log(a / b);
    }
    break;
  default:
    console.log("Noma'lum amal.");
}
```

`amal === "*"` bo'lgani uchun `10 * 4 = 40` natija chiqadi.

---

## Xulosa

| Konstruktsiya | Qachon ishlatiladi |
|---|---|
| `case` | O'zgaruvchi qiymatini aniq bir qiymat bilan solishtirish |
| `break` | Joriy `case` bajarilgandan so'ng chiqish uchun |
| `default` | Hech bir `case` mos kelmagan holatda |
| Guruhli `case` | Bir nechta qiymat uchun bir xil kod kerak bo'lganda |

---

## Mashqlar

**Oson darajali**

1. `kun` o'zgaruvchisi berilgan (`"dushanba"`, `"seshanba"` va hokazo). `switch` yordamida hafta kunining tartib raqamini chiqaring (dushanba — 1, seshanba — 2, ..., yakshanba — 7).

2. `fasl` o'zgaruvchisi berilgan (`"bahor"`, `"yoz"`, `"kuz"`, `"qish"`). Har bir faslga mos o'rtacha haroratni chiqaring.

3. `rang` o'zgaruvchisi berilgan. `switch` yordamida rangning inglizcha nomini chiqaring (`"qizil"` → `"red"`, `"yashil"` → `"green"`, `"ko'k"` → `"blue"`).

4. `toifa` o'zgaruvchisi berilgan (`"A"`, `"B"`, `"C"`, `"D"`, `"F"`). Har bir toifaga mos baho tavsifini chiqaring.

5. `oy` raqami berilgan (1–12). `switch` yordamida oyning nomini chiqaring.

6. Svetofor (`"yashil"`, `"sariq"`, `"qizil"`). Har bir holatga mos xabar chiqaring. Noto'g'ri qiymat uchun `"Noma'lum signal"` deb chiqaring.

7. `operatsiya` o'zgaruvchisi berilgan (`"qo'shish"`, `"ayirish"`, `"ko'paytirish"`, `"bo'lish"`). Ikkita son (`a` va `b`) bilan tegishli amalni bajaring.

**O'rta darajali**

8. `oy` raqami berilgan. `switch` yordamida qaysi faslga tegishli ekanini chiqaring:
   - 12, 1, 2 — Qish
   - 3, 4, 5 — Bahor
   - 6, 7, 8 — Yoz
   - 9, 10, 11 — Kuz

9. `yulduzcha` (`"♈"`, `"♉"`, `"♊"` va hokazo) berilgan. `switch` yordamida burj nomini chiqaring.

10. `tilKodi` o'zgaruvchisi berilgan (`"uz"`, `"ru"`, `"en"`). Salomlashish xabarini mos tilda chiqaring (`"Salom"`, `"Привет"`, `"Hello"`).

11. `kartaTuri` o'zgaruvchisi berilgan (`"visa"`, `"mastercard"`, `"humo"`, `"uzcard"`). Har bir karta turiga mos tavsif chiqaring.

12. `xato` kodi berilgan (404, 403, 500, 200). `switch` yordamida HTTP xato mazmunini chiqaring.

13. `transport` o'zgaruvchisi berilgan (`"avtomobil"`, `"avtobus"`, `"piyoda"`, `"velosiped"`). Har biriga mos taxminiy tezlikni chiqaring.

14. `o'lchov` birligi berilgan (`"km"`, `"mil"`, `"metr"`). Berilgan masofani boshqa birliklarga o'tkazing.

**Murakkab darajali**

15. Oddiy matn muharrir simulyatsiyasi: `buyruq` o'zgaruvchisi berilgan (`"yangi"`, `"saqlash"`, `"ochish"`, `"yopish"`, `"chiqish"`). Har bir buyruqqa mos xabar chiqaring. Noto'g'ri buyruq uchun `"Noma'lum buyruq"` deb chiqaring.

16. O'yin boshqaruvi: `tugma` o'zgaruvchisi berilgan (`"yuqori"`, `"pastki"`, `"chap"`, `"o'ng"`, `"ateş"`). Har bir tugmaga mos harakat xabarini chiqaring.

17. Elektron do'kon: `toifaId` berilgan (1 — Elektronika, 2 — Kiyim, 3 — Oziq-ovqat, 4 — Sport). Toifaga mos chegirma foizini aniqlang (1 — 10%, 2 — 15%, 3 — 5%, 4 — 20%) va agar narx berilgan bo'lsa, chegirmali narxni chiqaring.

18. Bank tranzaksiyasi: `operatsiyaTuri` berilgan (`"kirim"`, `"chiqim"`, `"o'tkazma"`, `"qaytarim"`). Har bir operatsiyaga mos komissiya foizini aniqlang va hisoblab chiqaring.

19. Matematika bo'limini tanlash: `daraja` berilgan (`"boshlang'ich"`, `"o'rta"`, `"oliy"`). Har bir darajaga mos mavzular ro'yxatini chiqaring.

20. Til o'rganish dasturi: `daraja` berilgan (`"A1"`, `"A2"`, `"B1"`, `"B2"`, `"C1"`, `"C2"`). Har bir darajaga mos tavsif va tavsiya etilgan mashg'ulot soatlarini chiqaring.
