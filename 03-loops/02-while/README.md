# While sikli (Shartli takrorlanish operatori)

Dasturlashda ba'zan biror amalni necha marta takrorlash kerakligini oldindan bilmaymiz. Masalan: "Foydalanuvchi to'g'ri parol kiritguncha qayta so'ra" yoki "Son 0 dan katta ekan, uni 2 ga bo'lib boraver". 

Bunday holatlarda **`while`** sikli ishlatiladi. `while` so'zi inglizchadan "modomiki ... ekan", "... davomida" degan ma'nolarni bildiradi.

---

## while siklining tuzilishi

`while` sikli berilgan shart `true` bo'lib turgan vaqtgacha o'z ichidagi kodni qayta-qayta bajaraveradi. Shart `false` bo'lishi bilan sikl to'xtaydi.

**Sintaksis:**

```js
while (shart) {
  // Shart rost bo'lsa, bajariladigan kod
  // Bu yerda shartni o'zgartiruvchi qadam bo'lishi shart!
}
```

---

## Oddiy misol: 1 dan 5 gacha sanash

```js
let i = 1; // 1. Boshlang'ich qiymat

while (i <= 5) { // 2. Shart
  console.log("Qadam: " + i);
  i++; // 3. Qadam (qiymatni oshirish)
}
```

**Natija:**

```
Qadam: 1
Qadam: 2
Qadam: 3
Qadam: 4
Qadam: 5
```

### Kod qadamma-qadam qanday ishlaydi?

1. `let i = 1` o'zgaruvchisi sikldan tashqarida e'lon qilindi.
2. `while (i <= 5)` tekshiriladi: `1 <= 5` rost (`true`).
3. Sikl ichiga kiradi: `Qadam: 1` ekranga chiqadi.
4. `i++` ishlab, `i` qiymati `2` ga aylanadi.
5. Yana shart tekshiriladi: `2 <= 5` rost. Kod yana ishlaydi.
6. Bu jarayon `i = 6` bo'lgunga qadar davom etadi.
7. `6 <= 5` yolg'on (`false`) bo'lgani sababli sikl to'xtaydi.

---

## for va while sikllari farqi

Aslida deyarli har qanday `for` siklini `while` orqali, va aksincha har qanday `while` siklini `for` orqali yozish mumkin. Lekin ularning maqsadi turlicha:

```js
// FOR bilan: Necha marta takrorlanishi aniq bo'lganda (masalan, roppa-rosa 5 marta)
for (let i = 1; i <= 5; i++) {
  console.log(i);
}

// WHILE bilan: Qandaydir shart bajarilguncha davom etganda
let son = 100;
while (son > 1) {
  son = son / 2;
  console.log(son);
}
```

---

## Eng xavfli xato: Cheksiz sikl (Infinite Loop)

`while` siklida eng ko'p uchraydigan xato — hisoblagichni yangilashni (`i++` ni) unutib qoldirishdir.

**Xato misol (buni sinab ko'rmang!):**

```js
let i = 1;

while (i <= 5) {
  console.log(i);
  // i++ YOZILMAGAN!
}
```

Bu holatda `i` doimo `1` bo'lib qoladi, `1 <= 5` sharti hech qachon `false` bo'lmaydi va dastur to'xtamasdan ishlab, kompyuterni qotirib qo'yishi mumkin.

> Har doim `while` sikli ichida shartni `false` holatga keltiruvchi o'zgarish borligini tekshiring!

---

## Amaliy misol 1: Sonning raqamlari yig'indisini topish

Har qanday butun son berilgan. Uning raqamlari yig'indisini `while` sikli yordamida topamiz. Har safar sonning oxirgi raqamini olib, sonni 10 ga bo'lib qisqartirib boramiz:

```js
let son = 456;
let yigindi = 0;

while (son > 0) {
  let oxirgiRaqam = son % 10; // Oxirgi raqamni olish (456 % 10 = 6)
  yigindi += oxirgiRaqam;      // Yig'indiga qo'shish
  son = Math.floor(son / 10);  // Oxirgi raqamni o'chirib tashlash (456 / 10 = 45)
}

console.log("Raqamlar yig'indisi: " + yigindi);
// Natija: 15 (chunki 4 + 5 + 6 = 15)
```

---

## Amaliy misol 2: Sonni teskari o'girish (Reverse number)

Masalan, `1234` sonini `4321` holatiga keltirish:

```js
let son = 1234;
let teskari = 0;

while (son > 0) {
  let qoldiq = son % 10;
  teskari = teskari * 10 + qoldiq;
  son = Math.floor(son / 10);
}

console.log("Teskari son: " + teskari);
// Natija: 4321
```

---

## Amaliy misol 3: 2 ning darajalarini topish

Berilgan sondan kichik bo'lgan barcha 2 ning darajalarini chiqarish:

```js
let chegara = 100;
let daraja = 1;

while (daraja <= chegara) {
  console.log(daraja);
  daraja *= 2; // 1, 2, 4, 8, 16, 32, 64
}
```

---

## Xulosa

| Xususiyat | `for` sikli | `while` sikli |
|-----------|-------------|---------------|
| Takrorlanish soni | Oldindan aniq ma'lum | Oldindan noma'lum bo'lishi mumkin |
| Boshlang'ich qiymat | Sikl sarlavhasida e'lon qilinadi | Sikldan tashqarida e'lon qilinadi |
| Qadam | Sikl sarlavhasida beriladi | Sikl tanasi ichida yoziladi |
| Tavsiya etilgan holat | Sanash, massiv elementlarini aylanib chiqish | Holat yoki shartga asoslangan jarayonlar |

---

## Mashqlar

**Oson darajali**

1. `while` sikli yordamida 1 dan 15 gacha bo'lgan sonlarni ekranga chiqaring.

2. `while` sikli yordamida 20 dan 1 gacha sonlarni teskari tartibda chiqaring.

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

15. **Palindrom son:** Berilgan son teskarisiga o'qilganda ham o'ziga teng ekanligini tekshiring (masalan: `1221` yoki `12321` — palindrom).

16. **EKUB (Eng katta umumiy bo'luvchi) - Evklid algoritmi:** Ikkita son (`a` va `b`) ning EKUBini `while` sikli yordamida toping.

17. **EKUK (Eng kichik umumiy karrali):** Ikkita sonning EKUKini topuvchi dastur tuzing.

18. Berilgan sonni ikkilik (binary) sanoq sistemasiga o'tkazuvchi dastur yozing (masalan: `13` → `"1101"`).

19. **Kollats gipotezasi (3n + 1):** Berilgan `n` soni juft bo'lsa `n / 2`, toq bo'lsa `3n + 1` qilinadi. Bu jarayon `n === 1` bo'lguncha davom etadi. Necha qadamda 1 ga yetishini va barcha oraliq sonlarni chiqaring.

20. Bankomat simulyatsiyasi: Berilgan pul miqdorini (masalan: `380 000` so'm) eng kam kupyuralar soni bilan chiqarish kerak (100 000, 50 000, 10 000 so'mliklar). `while` yordamida har bir kupyuradan nechtadan berilishini hisoblang.
