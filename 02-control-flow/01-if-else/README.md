# If - Else (Shartli operator)

Dasturlashda ko'pincha biror shartga qarab turlicha harakat qilishimiz kerak bo'ladi. Masalan: "Agar foydalanuvchi katta yoshli bo'lsa, kirish ruxsat etilsin, aks holda — rad etilsin." Aynan shu ishni `if-else` operatori bajaradi.

---

## if operatori

`if` kalit so'zi "agar" degan ma'noni anglatadi. Agar shart `true` (rost) bo'lsa, ichidagi kod bajariladi.

**Sintaksis:**

```js
if (shart) {
  // shart rost bo'lsa, shu yerga kiradi
}
```

**Misol:**

```js
let yosh = 20;

if (yosh >= 18) {
  console.log("Siz voyaga yetgansiz.");
}
```

Bu misolda `yosh >= 18` sharti tekshiriladi. `20 >= 18` rost bo'lgani uchun `"Siz voyaga yetgansiz."` chiqadi.

---

## else operatori

`else` kalit so'zi "aks holda" degan ma'noni anglatadi. `if` sharti yolg'on bo'lsa, `else` ichidagi kod bajariladi.

**Sintaksis:**

```js
if (shart) {
  // shart rost bo'lsa
} else {
  // shart yolg'on bo'lsa
}
```

**Misol:**

```js
let yosh = 15;

if (yosh >= 18) {
  console.log("Siz voyaga yetgansiz.");
} else {
  console.log("Siz hali voyaga yetmagansiz.");
}
```

`15 >= 18` yolg'on bo'lgani uchun `else` bloki ishlaydi va `"Siz hali voyaga yetmagansiz."` chiqadi.

---

## else if operatori

Bir nechta shartni ketma-ket tekshirish uchun `else if` ishlatiladi.

**Sintaksis:**

```js
if (birinchi shart) {
  // birinchi shart rost bo'lsa
} else if (ikkinchi shart) {
  // ikkinchi shart rost bo'lsa
} else {
  // hech biri rost bo'lmasa
}
```

**Misol:**

```js
let ball = 75;

if (ball >= 90) {
  console.log("A'lo");
} else if (ball >= 70) {
  console.log("Yaxshi");
} else if (ball >= 50) {
  console.log("Qoniqarli");
} else {
  console.log("Qoniqarsiz");
}
```

`ball = 75` bo'lgani uchun ikkinchi shart (`75 >= 70`) rost chiqadi va `"Yaxshi"` deb chiqadi.

> Shartlar yuqoridan pastga ketma-ket tekshiriladi. Birinchi rost shart topilishi bilan qolganlar tekshirilmaydi.

---

## Shartlarda ishlatiladigan operatorlar

Shartlar yozishda quyidagi taqqoslash operatorlaridan foydalanamiz:

| Operator | Ma'nosi              | Misol         |
|----------|----------------------|---------------|
| `==`     | teng                 | `5 == "5"` -> `true`  |
| `===`    | qat'iy teng          | `5 === "5"` -> `false` |
| `!=`     | teng emas            | `5 != 3` -> `true`    |
| `!==`    | qat'iy teng emas     | `5 !== "5"` -> `true` |
| `>`      | katta                | `10 > 5` -> `true`    |
| `<`      | kichik               | `3 < 8` -> `true`     |
| `>=`     | katta yoki teng      | `5 >= 5` -> `true`    |
| `<=`     | kichik yoki teng     | `4 <= 3` -> `false`   |

> `==` va `===` farqi: `==` faqat qiymatni, `===` esa qiymat va turni ham taqqoslaydi. Odatda `===` ishlatish tavsiya etiladi.

---

## Mantiqiy operatorlar

Ba'zan bir vaqtning o'zida bir nechta shartni tekshirish kerak bo'ladi:

| Operator | Ma'nosi | Misol |
|----------|---------|-------|
| `&&`     | va (ikkalasi ham rost bo'lishi kerak) | `yosh > 18 && fuqaro === true` |
| `\|\|`   | yoki (bittasi rost bo'lsa yetarli)   | `kun === "shanba" \|\| kun === "yakshanba"` |
| `!`      | emas (rostni yolg'onga, yolg'onni rostga aylantiradi) | `!false` -> `true` |

**Misol — && operatori:**

```js
let yosh = 22;
let fuqaro = true;

if (yosh >= 18 && fuqaro === true) {
  console.log("Ovoz berishingiz mumkin.");
} else {
  console.log("Ovoz berishga haqingiz yo'q.");
}
```

Ikki shart ham rost bo'lgani uchun `"Ovoz berishingiz mumkin."` chiqadi.

**Misol — || operatori:**

```js
let kun = "shanba";

if (kun === "shanba" || kun === "yakshanba") {
  console.log("Bugun dam olish kuni.");
} else {
  console.log("Bugun ish kuni.");
}
```

---

## Bir qatorli if (qisqa yozuv)

Agar `if` yoki `else` blokida faqat bitta buyruq bo'lsa, figurali qavslarni (`{}`) tushirib qoldirish mumkin. Lekin bu faqat juda oddiy holatlarda qo'llaniladi.

```js
let harorat = 35;

if (harorat > 30) console.log("Havo issiq.");
else console.log("Havo sovuq.");
```

> Ko'p qatorli kodlarda doim figurali qavslardan foydalanish tavsiya etiladi — bu xatoliklarni kamaytiradi.

---

## Uchlik operator (Ternary Operator)

Bu `if-else` ning qisqa yozuv shakli. Oddiy holatlar uchun qulay.

**Sintaksis:**

```js
shart ? "rost bo'lsa" : "yolg'on bo'lsa"
```

**Misol:**

```js
let yosh = 20;
let natija = yosh >= 18 ? "Katta" : "Kichik";
console.log(natija); // "Katta"
```

Bu quyidagiga to'liq teng:

```js
let natija;
if (yosh >= 18) {
  natija = "Katta";
} else {
  natija = "Kichik";
}
```

---

## Amaliy misol: Kalkulyator

```js
let a = 10;
let b = 0;
let amal = "/";

if (amal === "+") {
  console.log(a + b);
} else if (amal === "-") {
  console.log(a - b);
} else if (amal === "*") {
  console.log(a * b);
} else if (amal === "/") {
  if (b === 0) {
    console.log("Nolga bo'lish mumkin emas.");
  } else {
    console.log(a / b);
  }
} else {
  console.log("Noma'lum amal.");
}
```

Bu misolda `if` ichida yana `if` ishlatilgan — buni **ichma-ich if** (nested if) deyiladi.

---

## Xulosa

| Konstruktsiya | Qachon ishlatiladi |
|---------------|--------------------|
| `if`          | Faqat bitta shartni tekshirishda |
| `if-else`     | Rost va yolg'on uchun alohida kod kerak bo'lganda |
| `else if`     | Uch va undan ortiq holat bo'lganda |
| Ternary `? :` | Oddiy ikki holatli ifodalarni qisqa yozishda |

---

## Mashqlar

**Oson darajali**

1. `raqam` o'zgaruvchisi musbat, manfiy yoki nol ekanligini ekranga chiqaring.

2. Foydalanuvchi kiritgan sonning juft yoki toq ekanligini aniqlang.

3. Ikkita son berilgan (`a` va `b`). Kattaroq sonni ekranga chiqaring. Agar teng bo'lsa, `"Sonlar teng"` deb chiqaring.

4. `harorat` o'zgaruvchisi berilgan. Agar `harorat` 0 dan past bo'lsa `"Sovuq"`, 0 dan 20 gacha bo'lsa `"Iliq"`, 20 dan yuqori bo'lsa `"Issiq"` deb chiqaring.

5. `kun` o'zgaruvchisi (`"dushanba"`, `"seshanba"` va hokazo) berilgan. Agar dam olish kuni bo'lsa (`"shanba"` yoki `"yakshanba"`) `"Dam olish kuni"`, aks holda `"Ish kuni"` deb chiqaring.

6. `ball` o'zgaruvchisi berilgan. 90 va undan yuqori bo'lsa `"A'lo"`, 70–89 bo'lsa `"Yaxshi"`, 50–69 bo'lsa `"Qoniqarli"`, 50 dan past bo'lsa `"Qoniqarsiz"` deb chiqaring.

7. Uchta son berilgan (`a`, `b`, `c`). Ularning eng kattasini toping.

**O'rta darajali**

8. `yil` o'zgaruvchisi berilgan. Agar u kabisa yil bo'lsa `"Kabisa yil"`, aks holda `"Oddiy yil"` deb chiqaring.
   - Kabisa yil: 4 ga bo'linadi, lekin 100 ga bo'linmaydi; yoki 400 ga bo'linadi.

9. `oy` raqami berilgan (1–12). Qaysi fasl ekanini ekranga chiqaring:
   - 12, 1, 2 — Qish
   - 3, 4, 5 — Bahor
   - 6, 7, 8 — Yoz
   - 9, 10, 11 — Kuz

10. Foydalanuvchi `login` va `parol` kiritadi. Agar login `"admin"` va parol `"12345"` bo'lsa `"Kirish muvaffaqiyatli"`, aks holda `"Login yoki parol noto'g'ri"` deb chiqaring.

11. `son` o'zgaruvchisi berilgan. Agar son 3 ga ham, 5 ga ham bo'linsa `"FizzBuzz"`, faqat 3 ga bo'linsa `"Fizz"`, faqat 5 ga bo'linsa `"Buzz"`, aks holda sonning o'zini chiqaring.

12. Uchburchak tomonlari berilgan (`a`, `b`, `c`). Tomonlar bir xil bo'lsa `"Tengtomonli"`, ikkitasi bir xil bo'lsa `"Ikki tengtomonli"`, hech biri bir xil bo'lmasa `"Har xil tomonli"` deb chiqaring. Agar tomonlardan biri 0 yoki manfiy bo'lsa `"Noto'g'ri tomonlar"` deb chiqaring.

13. `narx` va `chegirma` (foizda) o'zgaruvchilari berilgan. Agar chegirma 0 dan katta bo'lsa, chegirmali narxni hisoblang va chiqaring. Aks holda `"Chegirma yo'q"` deb chiqaring.

14. Foydalanuvchi `yosh` kiritadi. Agar yosh 0–12 bo'lsa `"Bola"`, 13–17 bo'lsa `"O'smir"`, 18–64 bo'lsa `"Kattalar"`, 65 va undan yuqori bo'lsa `"Keksa"` deb chiqaring. Manfiy yosh uchun `"Noto'g'ri yosh"` deb chiqaring.

**Murakkab darajali**

15. Kalkulyator yarating: `a`, `b` va `amal` (`"+"`, `"-"`, `"*"`, `"/"`) o'zgaruvchilari berilgan. Tegishli amalni bajaring. Bo'lishda `b = 0` bo'lsa, `"Nolga bo'lib bo'lmaydi"` deb chiqaring.

16. `x`, `y` koordinatalari berilgan nuqta qaysi chorakda ekanini aniqlang:
    - x > 0 va y > 0 — 1-chorak
    - x < 0 va y > 0 — 2-chorak
    - x < 0 va y < 0 — 3-chorak
    - x > 0 va y < 0 — 4-chorak
    - Aks holda — "O'q ustida"

17. `a`, `b`, `c` koeffitsientlar berilgan (ax² + bx + c = 0). Diskriminantni hisoblang (`D = b² - 4ac`) va:
    - `D > 0` bo'lsa — ikkita ildiz borligini va ildizlarni chiqaring.
    - `D === 0` bo'lsa — bitta ildiz borligini chiqaring.
    - `D < 0` bo'lsa — haqiqiy ildiz yo'qligini chiqaring.

18. Elektr energiyasi hisob-kitob dasturi: `kVt` qiymati berilgan. Agar 100 kVt gacha bo'lsa — har biri 400 so'm, 100–300 oralig'i uchun — 600 so'm, 300 dan yuqori uchun — 900 so'm. Jami to'lovni hisoblang va chiqaring.

19. Uchta talabaning ismi va bali berilgan. Eng yuqori balga ega talabani aniqlang va `"G'olib: [ism]"` ko'rinishida chiqaring. Agar ikkitasi teng eng yuqori balga ega bo'lsa, `"Ikki g'olib bor"` deb chiqaring.

20. BMI (tana massa indeksi) hisoblovchi dastur yarating: `vazn` (kg) va `boy` (metr) berilgan. BMI = vazn / (boy * boy). Natijaga qarab:
    - BMI < 18.5 — "Ozg'in"
    - 18.5 – 24.9 — "Normal"
    - 25 – 29.9 — "Ortiqcha vazn"
    - 30 va undan yuqori — "Semizlik"

    Agar vazn yoki bo'y 0 yoki manfiy bo'lsa, `"Noto'g'ri ma'lumot"` deb chiqaring.
