# Continue operatori (Qadamni tashlab ketish)

Oldingi mavzuda ko'rganimizdek, `break` operatori siklni butunlay to'xtatib yuboradi. 

Lekin ba'zida siklni to'xtatish emas, balki ma'lum bir qadamdagi ishni **tashlab o'tib (sakrab o'tib)**, keyingi qadamdan davom ettirish kerak bo'ladi. Aynan shunday holatlarda **`continue`** operatori ishlatiladi.

> **Hayotiy misol:** Siz 1 dan 10 gacha bo'lgan xonalarni tozalayapsiz. 5-xonaning eshigida "Bezovta qilmang" degan yozuv bor. Siz 5-xonani tashlab o'tasiz (`continue`) va darhol 6-xonaga o'tib ishingizni davom ettirasiz. Butun ishni to'xtatmaysiz!

---

## continue operatorining tuzilishi

`continue` kalit so'zi uchragan zahoti, sikl tanasidagi undan keyin kelgan barcha kodlar bajarilmay qoladi va sikl to'g'ridan-to'g'ri keyingi qadamga (iteratsiyaga) o'tadi.

**Sintaksis:**

```js
for (boshlang'ich; shart; qadam) {
  if (tashlab_ketish_sharti) {
    continue; // Joriy qadam shu yerda to'xtaydi va keyingi qadamga o'tiladi
  }
  // continue ishlaganda bu yerdagi kodlar bajarilmaydi
}
```

---

## Oddiy misol: 3 raqamini tashlab ketish

1 dan 5 gacha sanaymiz, lekin 3 raqamini ekranga chiqarmay tashlab o'tamiz:

```js
for (let i = 1; i <= 5; i++) {
  if (i === 3) {
    console.log("3 soni tashlab ketildi!");
    continue; // Keyingi qatorlarni o'qimay, i = 4 ga o'tadi
  }
  console.log("Son: " + i);
}
```

**Natija:**

```
Son: 1
Son: 2
3 soni tashlab ketildi!
Son: 4
Son: 5
```

E'tibor bering, `i === 3` bo'lganda `console.log("Son: " + i)` ishlamadi, lekin sikl to'xtab qolmadi, `4` va `5` uchun odatdagidek davom etdi.

---

## Amaliy misol: Faqat toq sonlarni chiqarish

Agar son juft bo'lsa, uni tashlab ketamiz:

```js
for (let i = 1; i <= 10; i++) {
  if (i % 2 === 0) {
    continue; // Juft sonlarni tashlab ketamiz
  }
  console.log("Toq son: " + i);
}
```

**Natija:**
```
Toq son: 1
Toq son: 3
Toq son: 5
Toq son: 7
Toq son: 9
```

---

## break va continue farqi

| Xususiyat | `break` | `continue` |
|-----------|---------|------------|
| Asosiy vazifasi | Siklni butunlay yakunlaydi | Faqat joriy qadamni yakunlaydi |
| Sikl davom etadimi? | Yo'q, sikldan butunlay chiqib ketadi | Ha, keyingi qadamdan davom etadi |
| Qachon ishlatiladi? | Qidiruv yakunlanganda, limit to'lganda | Keraksiz qiymatlarni filtrlab tashlashda |

### Yonma-yon taqqoslash:

```js
// BREAK bilan:
for (let i = 1; i <= 5; i++) {
  if (i === 3) break;
  console.log(i); // Natija: 1, 2 (sikl tugadi!)
}

// CONTINUE bilan:
for (let i = 1; i <= 5; i++) {
  if (i === 3) continue;
  console.log(i); // Natija: 1, 2, 4, 5 (faqat 3 tushib qoldi!)
}
```

---

## DIQQAT: while siklida continue ishlatishdagi tuzoq!

`while` siklida `continue` ishlatayotganda juda ehtiyot bo'lish kerak. Agar hisoblagichni oshirish (`i++`) `continue` dan pastda qolib ketsa, sikl **cheksiz** bo'lib qoladi!

**Xato misol (cheksiz qotib qoladi!):**

```js
let i = 1;
while (i <= 5) {
  if (i === 3) {
    continue; // XATO! i++ gacha yetib bormaydi, i doim 3 bo'lib qoladi!
  }
  console.log(i);
  i++;
}
```

**To'g'ri usul:**

```js
let i = 1;
while (i <= 5) {
  if (i === 3) {
    i++; // Continue dan oldin qiymatni oshirib qo'yish shart!
    continue;
  }
  console.log(i);
  i++;
}
```

---

## Amaliy misol: Faqat musbat sonlar yig'indisi

Berilgan sonlar orasida manfiylari ham bor. Biz manfiylarni va 0 ni hisobga olmasdan, faqat musbat sonlar yig'indisini topmoqchimiz:

```js
let sonlar = [10, -5, 20, -15, 30, 0, 40];
let yigindi = 0;

for (let i = 0; i < sonlar.length; i++) {
  let joriySon = sonlar[i];

  if (joriySon <= 0) {
    continue; // Manfiy yoki 0 bo'lsa, yig'indiga qo'shmaymiz
  }

  yigindi += joriySon;
}

console.log("Musbat sonlar yig'indisi: " + yigindi);
// Natija: 100 (10 + 20 + 30 + 40 = 100)
```

---

## Xulosa

- `continue` operatori butun siklni to'xtatmaydi, faqat o'sha turgan qadamdagi qolgan kodlarni tashlab o'tadi.
- Filtrlash va keraksiz holatlarni chetlab o'tish uchun ayni muddao.
- `while` siklida foydalanganda hisoblagichni (`i++`) `continue` dan oldin oshirib qo'yish esdan chiqmasligi kerak.

---

## Mashqlar

**Oson darajali**

1. 1 dan 20 gacha sonlarni chiqaring, lekin 13 raqamini `continue` yordamida tashlab keting.

2. 1 dan 15 gacha sonlar ichidan faqat juft sonlarni chiqaring (toq sonlar uchraganda `continue` qiling).

3. 1 dan 30 gacha bo'lgan sonlar orasidan 3 ga karrali bo'lgan barcha sonlarni tashlab o'tib, qolganlarini ekranga chiqaring.

4. 1 dan 10 gacha sonlarni chiqaring, lekin 5 va 7 sonlarini tashlab o'ting.

5. 1 dan 50 gacha bo'lgan sonlar ichida oxiri 0 bilan tugaydigan sonlarni tashlab keting.

6. 20 dan 1 gacha teskari sanang, lekin 10 sonini tashlab o'ting.

7. `while` siklida `continue` dan to'g'ri foydalanib, 1 dan 10 gacha sonlar orasidagi faqat toq sonlarni chiqaring.

**O'rta darajali**

8. 1 dan 50 gacha bo'lgan sonlar yig'indisini hisoblang, lekin 5 ga karrali bo'lgan sonlarni yig'indiga qo'shmasdan tashlab keting.

9. Berilgan sonlar massividan faqat musbat sonlarning o'rtacha arifmetigini toping (manfiy va 0 larni `continue` qiling).

10. 1 dan 100 gacha sonlar ichida raqamlarida 7 qatnashgan sonlarni tashlab o'tib, qolganlarini ekranga chiqaring.

11. Talabalar ballari berilgan: `[85, 42, 90, 33, 76, 50, 95]`. O'tish bali — 60. Imtihondan o'tmagan ballarni `continue` bilan tashlab o'tib, o'tganlarning o'rtacha balini hisoblang.

12. Berilgan matndagi (string) barcha unli harflarni tashlab o'tib, faqat undosh harflarni ekranga chiqaring.

13. 1 dan 30 gacha sonlarni chiqaring, lekin agar son bir vaqtning o'zida ham 2 ga, ham 3 ga bo'linsa (ya'ni 6 ga bo'linsa), uni tashlab keting.

14. Ichma-ich sikl: 1 dan 5 gacha bo'lgan sonlar uchun karra jadvalini tuzing, lekin o'ziga teng ko'paytuvchilarni (masalan $2 \times 2$, $3 \times 3$...) `continue` bilan tashlab o'ting.

**Murakkab darajali**

15. 1 dan 100 gacha sonlar orasidan faqat murakkab (tub bo'lmagan) sonlarni ekranga chiqaring (tub sonlarni `continue` qiling).

16. Berilgan sonlar ro'yxatida nollar va manfiy sonlar bor. Manfiy sonlarni tashlab o'ting, agar ketma-ketlikda `999` xavfsizlik kodi uchrasa siklni butunlay to'xtating (`break` va `continue` birgalikda).

17. Matndan takrorlanuvchi bo'shliqlarni (probellarni) qisqartirib bitta bo'shliq qoldiruvchi dastur yozing (`continue` yordamida).

18. Bank tranzaksiyalari ro'yxati berilgan: agar tranzaksiya summasi 0 bo'lsa yoki shubhali deb belgilangan bo'lsa, uni tashlab o'ting, qolgan barcha amallar bo'yicha yakuniy balansni chiqaring.

19. Raqamlari yig'indisi toq bo'lgan 2 xonali sonlarni tashlab o'tib, faqat raqamlari yig'indisi juft bo'lgan 2 xonali sonlarni ekranga chiqaring.

20. 1 dan 100 gacha sonlar ustida quyidagi qoidani qo'llang:
    - Agar son 3 ga bo'linsa — `"Fizz"` deb chiqsin va keyingi qadamga o'tsin (`continue`);
    - Agar son 5 ga bo'linsa — `"Buzz"` deb chiqsin va keyingi qadamga o'tsin (`continue`);
    - Agar 15 ga bo'linsa — `"FizzBuzz"` deb chiqsin;
    - Qolgan hollarda sonning o'zi chiqsin.
