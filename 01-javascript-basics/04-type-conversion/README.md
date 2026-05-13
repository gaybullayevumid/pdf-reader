# Ma'lumot Turlarini O'zgartirish (Type Conversion)

JavaScript-da bir ma'lumot turini boshqasiga o'zgartirish jarayoni *Type Conversion* (Tur konvertatsiyasi) deb ataladi. Bu asosan foydalanuvchidan olingan ma'lumot (ko'pincha matn ko'rinishida bo'ladi) bilan hisob-kitob qilish kerak bo'lganda juda asqotadi.

Ikki xil tur o'zgartirish mavjud:
1. **Yashirin (Implicit)** - JavaScript o'zi avtomatik tarzda o'zgartiradi.
2. **Ochiq (Explicit)** - Dasturchi atayin maxsus funksiyalar orqali o'zgartiradi.

## 1. Yashirin (Avtomatik) O'zgartirish
JavaScript matematik amallar vaqtida ma'lumotlarni o'zi tushungan tarzda o'zgartirishga harakat qiladi:
```javascript
// Qo'shish operatorida matn ustunlik qiladi (biriktirib yuboradi)
console.log("5" + 2);  // Natija: "52" (Matn)

// Boshqa matematik operatorlarda matn raqamga aylanadi
console.log("10" - 5); // Natija: 5 (Raqam)
console.log("10" * 2); // Natija: 20 (Raqam)
console.log("15" / 3); // Natija: 5 (Raqam)
```

## 2. Ochiq O'zgartirish (Maxsus funksiyalar yordamida)

Eng ko'p ishlatiladigan konvertatsiya funksiyalari:

### Matnni Raqamga aylantirish - `Number()`
Matn ko'rinishidagi raqamni haqiqiy raqamga aylantiradi.
```javascript
let yoshMatni = "25";
let yoshRaqami = Number(yoshMatni);
console.log(typeof yoshRaqami); // "number"

// Matn raqam bo'lmasa nima bo'ladi?
console.log(Number("Salom")); // NaN (Not a Number - Raqam emas) qaytaradi.
```
*Siz shuningdek butun sonlar uchun `parseInt()`, kasr sonlar uchun `parseFloat()` yoki shunchaki qiymat oldidan `+` belgisidan foydalanishingiz ham mumkin (`+"25"` = 25).*

### Raqamni Matnga aylantirish - `String()` yoki `.toString()`
Raqamni matn ko'rinishiga o'tkazadi.
```javascript
let yil = 2024;
let yilMatni = String(yil); // yoki yil.toString()
console.log(typeof yilMatni); // "string"
```

### Mantiqiy (Boolean) turga o'zgartirish - `Boolean()`
Har qanday qiymatni `true` yoki `false` ga aylantiradi.
```javascript
console.log(Boolean(1));       // true
console.log(Boolean(0));       // false
console.log(Boolean("Salom")); // true
console.log(Boolean(""));      // false (bo'sh matn)
```
> **Faqatgina 6 ta qiymat har doim `false` ga aylanadi (falsy qiymatlar):**
> `0`, `""` (bo'sh matn), `null`, `undefined`, `NaN`, `false`. Boshqa hamma narsa `true` aylanadi.
