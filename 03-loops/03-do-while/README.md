# Do...While sikli (Kamida bir marta bajariluvchi takrorlanish)

Oldingi mavzularda ko'rgan `for` va `while` sikllarida eng birinchi navbatda **shart tekshiriladi**. Agar shart boshidanoq yolg'on (`false`) bo'lsa, sikl ichidagi kod **biror marta ham bajarilmaydi**.

Lekin hayotda va dasturlashda shunday holatlar bo'ladiki, biror amal shartdan qat'i nazar **kamida 1 marta** bajarilishi shart bo'ladi. Masalan:
- Foydalanuvchiga kamida bir marta menyuni ko'rsatish kerak;
- Foydalanuvchidan kamida bir marta PIN-kod so'rash kerak;
- O'yinda zar kamida bir marta tashlanishi kerak.

Aynan shunday vazifalar uchun JavaScript-da **`do...while`** sikli mavjud.

---

## do...while siklining tuzilishi

`do...while` avval kodni bajaradi (`do` = bajar), so'ngra shartni tekshiradi (`while` = modomiki).

**Sintaksis:**

```js
do {
  // Kamida bir marta bajariladigan kod
} while (shart);
```

> **Muhim:** `do...while` sintaksisining eng oxirida nuqta-vergul (`;`) qo'yiladi.

---

## Oddiy misol: 1 dan 5 gacha sanash

```js
let i = 1;

do {
  console.log("Qadam: " + i);
  i++;
} while (i <= 5);
```

**Natija:**

```
Qadam: 1
Qadam: 2
Qadam: 3
Qadam: 4
Qadam: 5
```

---

## while va do...while ning asosiy farqi

Quyidagi misolda shart boshidanoq yolg'on (`false`):

```js
// 1. WHILE sikli:
let a = 10;
while (a < 5) {
  console.log("Bu xabar CHIQMAYDI");
  a++;
}

// 2. DO...WHILE sikli:
let b = 10;
do {
  console.log("Bu xabar KAMIDA BIR MARTA chiqadi! Qiymat: " + b);
  b++;
} while (b < 5);
```

**Natija:**
```
Bu xabar KAMIDA BIR MARTA chiqadi! Qiymat: 10
```

`b < 5` ya'ni `10 < 5` yolg'on bo'lsa ham, `do` bloki avval ishlab, keyin shartni tekshirgani uchun xabar bir marta ekranga chiqdi!

---

## Amaliy misol 1: PIN-kod tekshirish simulyatsiyasi

Foydalanuvchi to'g'ri PIN-kod kiritguncha qayta so'raladigan tizim:

```js
let togriPin = "1234";
let kiritilganPin;
let urinishlar = 0;

// Simulyatsiya uchun urinishlar ro'yxati
let urinishlarRuyxati = ["0000", "9999", "1234"];

do {
  kiritilganPin = urinishlarRuyxati[urinishlar];
  urinishlar++;
  console.log(urinishlar + "-urinish: " + kiritilganPin + " kiritildi.");
} while (kiritilganPin !== togriPin);

console.log("PIN kod to'g'ri! Tizimga xush kelibsiz.");
```

**Natija:**
```
1-urinish: 0000 kiritildi.
2-urinish: 9999 kiritildi.
3-urinish: 1234 kiritildi.
PIN kod to'g'ri! Tizimga xush kelibsiz.
```

---

## Amaliy misol 2: O'yin simulyatsiyasi (Zar tashlash)

O'yinda zar 6 tushguncha qayta-qayta tashlanadi. Kamida bir marta zar tashlanishi shart:

```js
let zar;
let tashlashlarSoni = 0;

do {
  // 1 dan 6 gacha bo'lgan tasodifiy son hosil qilish
  zar = Math.floor(Math.random() * 6) + 1;
  tashlashlarSoni++;
  console.log(tashlashlarSoni + "-tashlash: " + zar + " tushdi.");
} while (zar !== 6);

console.log("Tabriklaymiz! " + tashlashlarSoni + " ta urinishda 6 chiqdi!");
```

---

## Uchala siklning taqqoslanishi

| Sikl turi | Shart qachon tekshiriladi? | Minimal bajarilish soni | Qachon ishlatiladi? |
|-----------|----------------------------|--------------------------|---------------------|
| `for` | Boshida (har qadamdan oldin) | 0 marta | Takrorlanishlar soni oldindan aniq bo'lganda |
| `while` | Boshida (har qadamdan oldin) | 0 marta | Qandaydir shart rost bo'lib turganda |
| `do...while` | Oxirida (qadamdan keyin) | **Kamida 1 marta** | Kod eng kamida bir marta ishlashi shart bo'lganda |

---

## Xulosa

- `do...while` siklida avval `do` blokidagi kod bajariladi, so'ngra `while` qavsi ichidagi shart tekshiriladi.
- Agar shart rost bo'lsa, `do` bloki yana qaytadan bajariladi.
- Agar shart hatto birinchi qadamdayoq noto'g'ri bo'lsa ham, kod baribir 1 marta bajariladi.
- Har doim cheksiz siklga tushib qolmaslik uchun `do` bloki ichida shartga ta'sir qiluvchi o'zgarish bo'lishi lozim.

---

## Mashqlar

**Oson darajali**

1. `do...while` sikli yordamida 1 dan 10 gacha bo'lgan sonlarni ekranga chiqaring.

2. `do...while` sikli yordamida 10 dan 1 gacha sonlarni teskari tartibda chiqaring.

3. Shart boshidanoq `false` bo'lgan `do...while` siklini yozing va uning bir marta ishlashini konsolda kuzating.

4. 1 dan 20 gacha bo'lgan barcha juft sonlarni `do...while` yordamida ekranga chiqaring.

5. 1 dan 20 gacha bo'lgan barcha toq sonlarni `do...while` yordamida ekranga chiqaring.

6. Ekranga 3 marta `"Do while o'rganilmoqda"` so'zini chiqaring.

7. 1 dan 10 gacha bo'lgan sonlar yig'indisini `do...while` orqali hisoblang.

**O'rta darajali**

8. 5 sonining karra jadvalini (1 dan 10 gacha) `do...while` sikli yordamida chiqaring.

9. Berilgan sonning faktorialini `do...while` sikli orqali hisoblang.

10. Berilgan sonning raqamlari sonini (necha xonali ekanini) `do...while` bilan aniqlang (eslatma: `0` soni ham 1 xonali, `do...while` bunga ayni mos keladi!).

11. Boshlang'ich qiymati 1 bo'lgan sonni har qadamda 3 ga ko'paytirib, 200 dan oshmaguncha ekranga chiqaring (1, 3, 9, 27, 81).

12. Berilgan sonning raqamlari yig'indisini `do...while` sikli orqali hisoblang.

13. Bank hisobida 500 000 so'm bor. Har kuni 45 000 so'm xarajat qilinadi. Necha kundan so'ng hisobda pul tugashini `do...while` bilan hisoblang.

14. Foydalanuvchiga menyu ko'rsatish: `1 - Bosh sahifa`, `2 - Sozlamalar`, `3 - Chiqish`. Foydalanuvchi `3` ni tanlamaguncha ishlaydigan tsiklni simulyatsiya qiling.

**Murakkab darajali**

15. **Son topish o'yini (Number Guessing):** Kompyuter 1 dan 20 gacha son o'ylagan. Foydalanuvchi taxmin qiladi. `do...while` yordamida to'g'ri topilguncha urinishlar sonini hisoblang.

16. Berilgan sonni teskari tartibda yozuvchi dastur tuzing (`do...while` yordamida, masalan: `9876` → `6789`).

17. Berilgan sonning eng katta raqamini `do...while` yordamida toping.

18. Berilgan son tub yoki tub emasligini `do...while` sikli yordamida aniqlang.

19. Dastlabki `n` ta juft sonlarning ko'paytmasini topuvchi dastur yozing.

20. Matematik qator yig'indisi: $S = 1 + \frac{1}{2} + \frac{1}{3} + \dots + \frac{1}{n}$. Foydalanuvchi bergan aniqlikkacha yoki $n$ qadamgacha yig'indini `do...while` yordamida hisoblang.
