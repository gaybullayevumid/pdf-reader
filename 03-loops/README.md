# 03. Loops (Sikllar / Takrorlanish operatorlari)

Dasturlashda eng ko'p bajariladigan vazifalardan biri — bu bir xil yoki o'xshash amallarni qayta-qayta takrorlashdir. Har safar bir xil kodni nusxalab yozmaslik, kodni toza, qisqa va tez ishlaydigan qilish uchun **sikllar (loops)** dan foydalanamiz.

Ushbu bo'limda JavaScript-dagi barcha asosiy sikl turlari va ularni boshqarish vositalari sodda, bosqichma-bosqich tushuntirilgan.

---

## Bo'lim mavzulari

1. [**01. For sikli**](./01-for/README.md)
   - Takrorlanishlar soni oldindan ma'lum bo'lgan holatlar uchun asosiy sikl.
   - Boshlang'ich qiymat, shart, qadam tushunchalari, teskari sanash, ichma-ich sikllar.

2. [**02. While sikli**](./02-while/README.md)
   - Takrorlanishlar soni oldindan noma'lum bo'lib, biror shartga bog'liq bo'lgan holatlar.
   - Shartli takrorlanishlar, raqamlarni ajratib olish va cheksiz sikllardan saqlanish.

3. [**03. Do...While sikli**](./03-do-while/README.md)
   - Kod kamida bir marta bajarilishi kafolatlangan sikl turi.
   - Avval bajarib, keyin shartni tekshirish xususiyati.

4. [**04. Break operatori**](./04-break/README.md)
   - Siklni muddatidan oldin, darhol to'xtatish va undan chiqib ketish.
   - Qidiruv algoritmlari va keraksiz hisob-kitoblarni qisqartirish.

5. [**05. Continue operatori**](./05-continue/README.md)
   - Faqat joriy qadamni (iteratsiyani) tashlab o'tib, keyingi qadamdan davom ettirish.
   - Ma'lumotlarni saralash va filtrlash.

---

## Sikllarni qisqacha taqqoslash

| Operator | Qachon ishlatiladi? | Shart qachon tekshiriladi? | Kamida necha marta ishlaydi? |
|----------|---------------------|----------------------------|-------------------------------|
| `for` | Takrorlanishlar soni aniq bo'lganda (masalan, 1 dan 100 gacha) | Har qadamdan oldin | 0 marta |
| `while` | Shart bajarilib turguncha (takrorlanishlar soni noma'lum) | Har qadamdan oldin | 0 marta |
| `do...while` | Shart qanday bo'lishidan qat'i nazar kamida 1 marta ishlashi kerak bo'lsa | Qadam bajarilgandan keyin | **1 marta** |
| `break` | Siklni darhol va butunlay to'xtatish | — | — |
| `continue` | Faqat bitta qadamni tashlab, keyingisiga o'tish | — | — |

---

Har bir mavzuni tartib bilan o'rganib chiqish va oxiridagi amaliy mashqlarni (har bir mavzuda 20 tadan oson, o'rta va murakkab masalalar) mustaqil bajarib ko'rish tavsiya etiladi!
