# While sikli (Shartli takrorlanish operatori)

Dasturlashda ko'pincha amallar soni oldindan aniq bo'lmaydi. Masalan:
- Telefoningiz batareyasi quvvati 0% bo'lmaguncha ishlayveradi;
- Avtomobil benzin tugaguncha harakatlanadi;
- O'yinchi mag'lub bo'lmaguncha o'yin davom etaveradi;
- Foydalanuvchi to'g'ri parol kiritmaguncha tizim qayta-qayta parolni so'rayveradi.

Bunday holatlarda takrorlanishlar soni oldindan ma'lum bo'lmagani uchun **`while`** sikli ishlatiladi. `while` so'zi inglizchadan tarjima qilinganda **"modomiki ... ekan"**, **"... davomida"** degan ma'nolarni anglatadi.

---

## while siklining tuzilishi

`while` sikli berilgan shart `true` (rost) bo'lib turgan vaqtgacha o'z blokidagi kodni to'xtovsiz bajaraveradi. Shart `false` (yolg'on) bo'lishi bilan sikl o'z ishini yakunlaydi.

**Sintaksis:**

```js
while (shart) {
  // Shart rost bo'lsa, bajariladigan kod
  // Shartga ta'sir qiluvchi o'zgarish (qadam) bo'lishi SHART!
}
```

---

## Qadamma-qadam ishlash sxemasi

Siklning ishlash mantiqini quyidagi sxema orqali tasavvur qilish mumkin:

```text
[Boshlanish] ───► [Shart tekshiriladi]
                         │
              ┌──────────┴──────────┐
              ▼                     ▼
         ROST (true)          YOLG'ON (false)
              │                     │
              ▼                     ▼
        [Sikl tanasi]        [Sikl tugaydi!]
              │
              ▼
    [O'zgaruvchi yangilanadi]
              │
              └──────────► (Yana shartga qaytadi)
```

1. Avval qavs ichidagi `shart` tekshiriladi.
2. Agar shart `true` bo'lsa — jingalak qavslar `{}` ichidagi kod bajariladi.
3. Kod bajarilib bo'lgach, jarayon yana boshiga qaytib, shartni qayta tekshiradi.
4. Agar shart `false` bo'lsa — sikl to'xtaydi va dastur undan keyingi qatorlarga o'tadi.

---

## while siklining 3 ta oltin ustuni

Har qanday sog'lom `while` sikli 3 ta muhim qismdan iborat bo'lishi kerak:

1. **Boshlang'ich holat:** Sikldan tashqarida hisoblagich yoki holat o'zgaruvchisi e'lon qilinadi (masalan: `let i = 1;`).
2. **Shart:** Sikl qachongacha ishlashini tekshiruvchi ifoda (masalan: `i <= 5`).
3. **O'zgarish (qadam):** Sikl ichida har safar shartdagi o'zgaruvchini yangilab boruvchi amal (masalan: `i++;`).

Agar shu uchtadan bittasi tushib qolsa, sikl yo umuman ishlamaydi, yoki to'xtovsiz ishlab qotib qoladi.

---

## Oddiy misol: 1 dan 5 gacha sanash

```js
let i = 1; // 1. Boshlang'ich qiymat

while (i <= 5) { // 2. Shart tekshiruvi
  console.log("Qadam: " + i);
  i++; // 3. Qadam (qiymatni 1 taga oshirish)
}

console.log("Sikl yakunlandi!");
```

**Natija:**

```
Qadam: 1
Qadam: 2
Qadam: 3
Qadam: 4
Qadam: 5
Sikl yakunlandi!
```

### Kod qadamlari tahlili:

| Qadam | `i` ning qiymati | `i <= 5` sharti | Ekranga chiqadi | Yangi `i` (`i++`) |
|-------|------------------|-----------------|-----------------|-------------------|
| 1-aylana | 1 | `1 <= 5` (true) | `Qadam: 1` | 2 |
| 2-aylana | 2 | `2 <= 5` (true) | `Qadam: 2` | 3 |
| 3-aylana | 3 | `3 <= 5` (true) | `Qadam: 3` | 4 |
| 4-aylana | 4 | `4 <= 5` (true) | `Qadam: 4` | 5 |
| 5-aylana | 5 | `5 <= 5` (true) | `Qadam: 5` | 6 |
| 6-aylana | 6 | `6 <= 5` (false) | *(Sikldan chiqadi)* | — |

---

## Eng xavfli xato: Cheksiz sikl (Infinite Loop)

`while` bilan ishlashda yangi boshlovchilar yo'l qo'yadigan eng katta xato — bu hisoblagichni oshirishni (`i++` ni) unutib qoldirishdir.

**Xato kod (buni sinab ko'rmang!):**

```js
let i = 1;

while (i <= 5) {
  console.log(i);
  // i++ YOZILMAGAN!
}
```

Bu holatda `i` doim `1` ga teng bo'lib qoladi, `1 <= 5` sharti hech qachon `false` bo'lmaydi va kompyuter cheksiz ravishda `1` chiqaraverib, brauzer yoki dasturni qotirib qo'yadi.

> **Agar cheksiz siklga tushib qolsangiz:** Brauzer sahifasini darhol yoping yoki terminalda `Ctrl + C` tugmasini bosing.

---

## Shart boshidanoq yolg'on (false) bo'lsa-chi?

Agar shart eng birinchi tekshiruvdayoq `false` bo'lsa, `while` siklining ichidagi kod **biror marta ham bajarilmaydi**:

```js
let yosh = 15;

while (yosh >= 18) {
  console.log("Siz voyaga yetgansiz."); // Bu qatorga dastur umuman kirmaydi!
  yosh++;
}

console.log("Dastur davom etmoqda...");
```

---

## Bayroqcha (Boolean Flag) yordamida siklni boshqarish

Dasturlashda ko'pincha holatni ifodalovchi `true` yoki `false` qiymatli o'zgaruvchilar (ularni *bayroqcha* yoki *flag* deyiladi) orqali `while` sikli boshqariladi:

```js
let oynaOchiq = true;
let xonaHarorati = 25;

while (oynaOchiq) {
  console.log("Harorat: " + xonaHarorati + "°C. Oyna ochiq.");
  xonaHarorati--; // Xona sovib bormoqda

  // Harorat 20 darajaga tushganda oynani yopamiz
  if (xonaHarorati <= 20) {
    oynaOchiq = false; // Sikl shartini to'xtatuvchi bayroq
    console.log("Xona yetarlicha sovidi. Oyna yopildi!");
  }
}
```

**Natija:**
```
Harorat: 25°C. Oyna ochiq.
Harorat: 24°C. Oyna ochiq.
Harorat: 23°C. Oyna ochiq.
Harorat: 22°C. Oyna ochiq.
Harorat: 21°C. Oyna ochiq.
Xona yetarlicha sovidi. Oyna yopildi!
```

---

## for va while sikllarining farqi

Har qanday `for` siklini `while` ga, har qanday `while` siklini esa `for` ga aylantirish mumkin. Lekin qaysi birini qachon tanlash qulayroq?

```js
// 1. FOR: Qadamlar soni oldindan ma'lum bo'lganda (10 marta, 100 marta)
for (let i = 1; i <= 5; i++) {
  console.log(i);
}

// 2. WHILE: Takrorlanishlar soni shartga bog'liq bo'lganda
let batareya = 100;
while (batareya > 0) {
  batareya -= 20;
  console.log("Qolgan zaryad: " + batareya + "%");
}
```

---

## Amaliy misol 1: Sonning raqamlari yig'indisini hisoblash

Har qanday butun sonning (masalan: `385`) raqamlari yig'indisini topish. Buning uchun `while` siklida sonni oxirgi raqamini `% 10` orqali ajratib olamiz va `Math.floor(son / 10)` orqali oxirgi raqamni qirqib tashlaymiz:

```js
let son = 385;
let yigindi = 0;

while (son > 0) {
  let oxirgiRaqam = son % 10; // 385 % 10 = 5
  yigindi += oxirgiRaqam;      // yigindi = 0 + 5 = 5
  son = Math.floor(son / 10);  // 385 / 10 = 38 (butun qismi)
}

console.log("Raqamlar yig'indisi: " + yigindi);
// Natija: 16 (chunki 5 + 8 + 3 = 16)
```

---

## Amaliy misol 2: Sonni teskari o'girish (Reverse number)

Sonni orqadan oldinga qarab yozish (masalan: `1234` → `4321`):

```js
let son = 1234;
let teskari = 0;

while (son > 0) {
  let qoldiq = son % 10;
  teskari = teskari * 10 + qoldiq; // Raqamni yangi o'ringa qo'shish
  son = Math.floor(son / 10);
}

console.log("Teskari son: " + teskari); // 4321
```

---

## Amaliy misol 3: Bank depoziti o'sishini hisoblash

Foydalanuvchi hisobida 1 000 000 so'm bor. Har oyda pulga 5% foyda qo'shiladi. Pul qachon 2 000 000 so'mdan oshadi?

```js
let balans = 1000000;
let maqsad = 2000000;
let oy = 0;

while (balans < maqsad) {
  balans += balans * 0.05; // 5% ustama qo'shildi
  oy++;
}

console.log(oy + " oydan so'ng balans: " + Math.round(balans) + " so'm bo'ladi.");
// Natija: 15 oydan so'ng balans: 2078928 so'm bo'ladi.
```

---

## Amaliy misol 4: Bo'lish amalisiz butun qism va qoldiqni topish

Matematikada bo'lish amali aslida ketma-ket ayirish demakdir ($17 / 5$ — 17 dan necha marta 5 ni ayira olamiz?):

```js
let bolinuvchi = 17;
let boluvchi = 5;
let butunQism = 0;

while (bolinuvchi >= boluvchi) {
  bolinuvchi -= boluvchi;
  butunQism++;
}

console.log("Butun qismi: " + butunQism); // 3
console.log("Qoldiq: " + bolinuvchi);     // 2
```

---

## Amaliy misol 5: 2 ning darajalarini chiqarish

100 dan kichik bo'lgan barcha ikkining darajalarini chiqarish:

```js
let daraja = 1;

while (daraja < 100) {
  console.log(daraja);
  daraja *= 2; // 1, 2, 4, 8, 16, 32, 64
}
```

---

## Xulosa

| Xususiyat | `for` sikli | `while` sikli |
|-----------|-------------|---------------|
| Takrorlanish soni | Oldindan aniq ma'lum bo'lganda | Noma'lum bo'lib, shartga bog'liq bo'lganda |
| Boshlang'ich qiymat | Sikl ichida e'lon qilinadi (`let i = 0`) | Sikldan oldin tashqarida e'lon qilinadi |
| Qadam (yangilanish) | Sikl boshida belgilanadi (`i++`) | Sikl tanasi ichida yoziladi |
| Cheksiz sikl xavfi | Kamroq (sintaksis o'zi talab qiladi) | Yuqoriroq (`i++` ni unutib qo'yish mumkin) |

---

## Mashqlar

**Oson darajali**

1. `while` sikli yordamida 1 dan 15 gacha bo'lgan sonlarni ekranga chiqaring.

2. `while` yordamida 20 dan 1 gacha sonlarni teskari tartibda chiqaring.

3. 1 dan 30 gacha bo'lgan barcha juft sonlarni `while` yordamida chiqaring.

4. 1 dan 25 gacha bo'lgan barcha toq sonlarni `while` yordamida chiqaring.

5. Berilgan `n` sonigacha bo'lgan sonlar yig'indisini `while` sikli bilan hisoblang.

6. Ekranga 7 marta `"Men JavaScript o'rganyapman"` so'zini chiqaring.

7. 100 dan boshlab har safar 10 ga kamaytirib, 0 gacha bo'lgan sonlarni chiqaring (100, 90, 80, ..., 0).

**O'rta darajali**

8. Berilgan sonning necha xonali ekanligini `while` sikli yordamida aniqlang (masalan: `34567` → 5 xonali).

9. Berilgan son 2 ning qandaydir darajasimi yoki yo'qmi ekanligini aniqlang (masalan: 16 — ha, 18 — yo'q).

10. Berilgan butun musbat sonning raqamlari ko'paytmasini hisoblang (masalan: `234` → `2 * 3 * 4 = 24`).

11. Berilgan sonning eng katta raqamini `while` yordamida toping (masalan: `48291` → eng kattasi `9`).

12. Berilgan sonning eng kichik raqamini `while` yordamida toping (masalan: `7392` → eng kichigi `2`).

13. Foydalanuvchi hisobida 1000 dollar bor. Har oyda balans 5% ga oshadi. Necha oydan so'ng balans 2000 dollardan oshishini hisoblang.

14. Berilgan sonning faktorialini `while` sikli bilan hisoblang.

**Murakkab darajali**

15. **Palindrom son:** Berilgan son teskarisiga o'qilganda ham o'ziga teng ekanligini `while` orqali tekshiring (masalan: `1221` yoki `12321` — palindrom).

16. **EKUB (Eng katta umumiy bo'luvchi) - Evklid algoritmi:** Ikkita son (`a` va `b`) ning EKUBini `while` sikli yordamida toping.

17. **EKUK (Eng kichik umumiy karrali):** Ikkita sonning EKUKini topuvchi dastur tuzing.

18. Berilgan butun sonni ikkilik (binary) sanoq sistemasiga o'tkazuvchi dastur yozing (masalan: `13` → `"1101"`).

19. **Kollats gipotezasi (3n + 1):** Berilgan `n` soni juft bo'lsa `n / 2`, toq bo'lsa `3n + 1` qilinadi. Bu jarayon `n === 1` bo'lguncha davom etadi. Necha qadamda 1 ga yetishini va barcha oraliq sonlarni chiqaring.

20. **Bankomat simulyatsiyasi:** Berilgan pul miqdorini (masalan: `380 000` so'm) eng kam kupyuralar soni bilan chiqarish kerak (100 000, 50 000, 10 000 so'mliklar). `while` yordamida har bir kupyuradan nechtadan berilishini hisoblang.
