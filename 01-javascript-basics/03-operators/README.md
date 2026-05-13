# Operatorlar (Operators)

JavaScript-da ma'lumotlar ustida amallar bajarish uchun operatorlardan foydalanamiz. Matematik hisob-kitoblar, solishtirishlar yoki qiymat berish shular jumlasidandir.

## 1. Matematik Operatorlar (Arithmetic Operators)
Raqamlar ustida matematik amallar bajaradi.
*   `+` (Qo'shish): `5 + 3 // 8`
*   `-` (Ayirish): `10 - 4 // 6`
*   `*` (Ko'paytirish): `4 * 3 // 12`
*   `/` (Bo'lish): `15 / 3 // 5`
*   `%` (Qoldiqni topish): `10 % 3 // 1` (10 ni 3 ga bo'lgandagi qoldiq)
*   `**` (Darajaga ko'tarish): `2 ** 3 // 8` (2 ning 3-darajasi)

*Matematik operatorlar matnlarga (string) ham qo'llanilishi mumkin, bu holda `+` matnlarni bir-biriga ulaydi:*
```javascript
let matn = "Salom " + "Dunyo!"; // "Salom Dunyo!"
```

## 2. Tayinlash Operatorlari (Assignment Operators)
O'zgaruvchiga qiymat berish uchun ishlatiladi.
*   `=` : Oddiy qiymat berish (`let x = 10;`)
*   `+=` : Qo'shib tayinlash (`x += 5;` bu `x = x + 5;` bilan bir xil)
*   `-=` : Ayirib tayinlash (`x -= 2;`)
*   `*=` : Ko'paytirib tayinlash (`x *= 3;`)

## 3. Solishtirish Operatorlari (Comparison Operators)
Ikki qiymatni solishtiradi va natija sifatida doim `true` yoki `false` qaytaradi.
*   `==` : Tengmi? (Faqat qiymatni tekshiradi). Masalan: `5 == "5" // true`
*   `===`: Qat'iy tengmi? (Ham qiymat, ham turini tekshiradi). Masalan: `5 === "5" // false`
*   `!=` : Teng emasmi?
*   `!==`: Qat'iy teng emasmi?
*   `>`  : Katta
*   `<`  : Kichik
*   `>=` : Katta yoki teng
*   `<=` : Kichik yoki teng

## 4. Mantiqiy Operatorlar (Logical Operators)
Bir nechta shartlarni birlashtirish uchun ishlatiladi.
*   `&&` (VA - AND): Ikkala tomon ham `true` bo'lsagina natija `true` bo'ladi.
*   `||` (YOKI - OR): Hech bo'lmaganda bir tomoni `true` bo'lsa, natija `true` bo'ladi.
*   `!` (EMAS - NOT): Natijani teskarisiga o'zgartiradi (`true` ni `false` ga, `false` ni `true` ga).
