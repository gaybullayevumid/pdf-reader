# For sikli (Takrorlanish operatori)

Dasturlashda ko'pincha bir xil yoki o'xshash harakatlarni qayta-qayta bajarishga to'g'ri keladi. Masalan, ekranga 100 marta `"Salom Dunyo"` deb chiqarish kerak bo'lsin. Buning uchun 100 marta `console.log("Salom Dunyo")` yozib chiqish noqulay va samarasiz. Aynan shunday takrorlanuvchi jarayonlarni avtomatlashtirish uchun **sikllar (loops)** ishlatiladi.

Agar takrorlanishlar soni oldindan aniq ma'lum bo'lsa, odatda eng qulay va keng qo'llaniladigan sikl turi bu — `for` siklidir.

---

## for siklining tuzilishi

`for` sikli bitta qatorda uchta asosiy qismni birlashtiradi:

**Sintaksis:**

```js
for (boshlang'ich_qiymat; shart; qadam) {
  // Har bir takrorlanishda bajariladigan kod
}
```

Bu uch qism quyidagilarni bildiradi:

1. **Boshlang'ich qiymat (`initialization`):** Sikl hisoblagichi (counter) yaratiladi va unga ilk qiymat beriladi. Faqat bir marta — sikl boshlanishida ishlaydi (masalan: `let i = 1`).
2. **Shart (`condition`):** Har bir takrorlanishdan oldin tekshiriladi. Agar shart `true` bo'lsa, sikl tanasi ishlaydi. `false` bo'lsa, sikl to'xtaydi (masalan: `i <= 5`).
3. **Qadam (`increment / decrement`):** Sikl tanasi bajarib bo'lingach, hisoblagich qiymatini o'zgartiradi (masalan: `i++` yoki `i--`).

---

## Oddiy misol: 1 dan 5 gacha sanash

```js
for (let i = 1; i <= 5; i++) {
  console.log("Qadam: " + i);
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

1. `let i = 1` yaratildi.
2. `1 <= 5` rostmi? Ha (`true`). Kod ishlaydi: `Qadam: 1`.
3. `i++` hisobiga `i` endi `2` bo'ldi.
4. `2 <= 5` rostmi? Ha. Kod ishlaydi: `Qadam: 2`.
5. Jarayon shu tariqa davom etadi...
6. `i` qiymati `6` bo'lganda: `6 <= 5` yolg'on (`false`). Sikl to'xtaydi va keyingi qatorlarga o'tadi.

---

## Teskari sanash (Kamayib boruvchi sikl)

Siklni nafaqat oshirish, balki kamaytirib borish ham mumkin. Buning uchun qadamda `i--` ishlatiladi.

**Misol: 5 dan 1 gacha sanash:**

```js
for (let i = 5; i >= 1; i--) {
  console.log(i);
}

console.log("Start!");
```

**Natija:**

```
5
4
3
2
1
Start!
```

---

## Qadamni ixtiyoriy songa o'zgartirish

Sikl hisoblagichini faqat 1 taga emas, istalgan songa oshirish yoki kamaytirish mumkin.

**Misol: 0 dan 10 gacha faqat juft sonlarni chiqarish:**

```js
for (let i = 0; i <= 10; i += 2) {
  console.log(i);
}
```

**Natija:**

```
0
2
4
6
8
10
```

---

## Cheksiz sikl (Infinite Loop) xavfi

Agar sikl sharti hech qachon `false` bo'lmasa, dastur to'xtovsiz ishlayveradi. Buni **cheksiz sikl** deyiladi va bu kompyuter yoki brauzerning qotib qolishiga sabab bo'ladi.

**Xato misol (buni ishlatmang!):**

```js
// XATO: i doim 1 ga oshadi, lekin u 0 dan kichik bo'lishi tekshirilmayapti
for (let i = 1; i > 0; i++) {
  console.log(i); // Cheksiz ishlaydi!
}
```

> Har doim sikl sharti qachondir `false` bo'lishiga va qadam to'g'ri yozilganiga ishonch hosil qiling.

---

## Ichma-ich for sikllari (Nested Loops)

Bitta `for` siklining ichida boshqa bir `for` siklini yozish mumkin. Bunda tashqi siklning har bir qadami uchun ichki sikl to'liq boshidan oxirigacha aylanadi.

**Misol: Oddiy jadval hosil qilish:**

```js
for (let i = 1; i <= 3; i++) {
  console.log("Guruh: " + i);
  for (let j = 1; j <= 2; j++) {
    console.log("  O'quvchi: " + j);
  }
}
```

**Natija:**

```
Guruh: 1
  O'quvchi: 1
  O'quvchi: 2
Guruh: 2
  O'quvchi: 1
  O'quvchi: 2
Guruh: 3
  O'quvchi: 1
  O'quvchi: 2
```

---

## Amaliy misol 1: 1 dan N gacha sonlar yig'indisi

```js
let n = 10;
let yigindi = 0;

for (let i = 1; i <= n; i++) {
  yigindi += i;
}

console.log("1 dan " + n + " gacha sonlar yig'indisi: " + yigindi);
// Natija: 55
```

---

## Amaliy misol 2: Karra jadvali

```js
let son = 7;

console.log(son + " sonining karra jadvali:");

for (let i = 1; i <= 10; i++) {
  let natija = son * i;
  console.log(son + " x " + i + " = " + natija);
}
```

---

## Amaliy misol 3: Faktorialni hisoblash

Faktorial ($N!$) — 1 dan $N$ gacha bo'lgan barcha sonlarning ko'paytmasi ($5! = 1 \times 2 \times 3 \times 4 \times 5 = 120$).

```js
let son = 5;
let faktorial = 1;

for (let i = 1; i <= son; i++) {
  faktorial *= i;
}

console.log(son + "! = " + faktorial); // 5! = 120
```

---

## Xulosa

| Qism | Nomi | Vazifasi | Misol |
|------|------|----------|-------|
| 1-qism | Boshlang'ich qiymat (`initialization`) | Hisoblagich o'zgaruvchini e'lon qilish | `let i = 0` |
| 2-qism | Shart (`condition`) | Sikl qachongacha ishlashini belgilaydi | `i < 10` |
| 3-qism | Qadam (`iteration`) | Har qadamda hisoblagichni yangilaydi | `i++` yoki `i += 2` |

> `for` sikli takrorlanishlar soni oldindan aniq bo'lgan holatlarda eng to'g'ri tanlovdir.

---

## Mashqlar

**Oson darajali**

1. 1 dan 20 gacha bo'lgan sonlarni ekranga chiqaring.

2. 10 dan 1 gacha bo'lgan sonlarni teskari tartibda ekranga chiqaring.

3. 1 dan 50 gacha bo'lgan barcha juft sonlarni ekranga chiqaring.

4. 1 dan 30 gacha bo'lgan barcha toq sonlarni ekranga chiqaring.

5. Ekranga 5 marta `"JavaScript juda qiziq!"` matnini chiqaring.

6. 1 dan 10 gacha bo'lgan sonlarning kvadratlarini chiqaring (1, 4, 9, 16, ..., 100).

7. 1 dan 100 gacha bo'lgan sonlar orasidan 5 ga karrali (bo'linadigan) sonlarni chiqaring.

**O'rta darajali**

8. 1 dan 50 gacha bo'lgan barcha sonlarning yig'indisini hisoblang va chiqaring.

9. Foydalanuvchi kiritgan `n` sonigacha bo'lgan juft sonlar yig'indisini toping.

10. Berilgan sonning faktorialini hisoblang (masalan, `n = 6` bo'lsa `6! = 720`).

11. 1 dan 100 gacha sonlar ichida 3 ga ham, 7 ga ham bo'linadigan sonlarni va ularning soni nechta ekanini chiqaring.

12. Ichma-ich `for` yordamida quyidagi yulduzchalar shaklini chiqaring:
    ```
    *
    **
    ***
    ****
    *****
    ```

13. 1 dan 10 gacha bo'lgan barcha sonlar uchun to'liq karra jadvalini ekranga chiqaring (1x1 dan 10x10 gacha).

14. Berilgan `n` sonining barcha bo'luvchilarini ekranga chiqaring (masalan: 12 uchun: 1, 2, 3, 4, 6, 12).

**Murakkab darajali**

15. Berilgan `n` soni tub (faqat 1 ga va o'ziga bo'linadigan) yoki murakkab son ekanligini `for` sikli orqali aniqlang.

16. Dastlabki `n` ta Fibonachchi sonlarini ekranga chiqaring (0, 1, 1, 2, 3, 5, 8, 13, ...).

17. Berilgan sonning raqamlari yig'indisini hisoblang (masalan: `son = 4567` bo'lsa, natija: `4 + 5 + 6 + 7 = 22`).

18. Ichma-ich `for` yordamida teskari uchburchak shaklini yasang:
    ```
    *****
    ****
    ***
    **
    *
    ```

19. 1 dan 500 gacha bo'lgan sonlar orasidan barcha Armstrong sonlarini toping (raqamlari kublarining yig'indisi o'ziga teng bo'lgan sonlar, masalan: $153 = 1^3 + 5^3 + 3^3 = 1 + 125 + 27 = 153$).

20. Mukammal sonni aniqlang: o'zidan tashqari barcha bo'luvchilari yig'indisi o'ziga teng bo'lgan son (masalan: 6 ning bo'luvchilari 1, 2, 3 va ularning yig'indisi $1 + 2 + 3 = 6$). Berilgan son mukammal yoki yo'qligini tekshiring.
