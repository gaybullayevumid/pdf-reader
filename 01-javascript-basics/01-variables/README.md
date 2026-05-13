# O'zgaruvchilar (Variables)

JavaScript-da ma'lumotlarni saqlash uchun o'zgaruvchilardan foydalanamiz. O'zgaruvchilarni xuddi quti kabi tasavvur qilishingiz mumkin - siz bu qutiga biror narsa solasiz va unga nom berasiz. Keyinchalik shu nom orqali ichidagi narsani topib olasiz.

O'zgaruvchilarni e'lon qilish uchun uchta asosiy kalit so'z mavjud:

1. **`let`** - o'zgaruvchan (qiymati o'zgarishi mumkin bo'lgan) ma'lumotlar uchun ishlatiladi. Bu eng ko'p ishlatiladigan usul.
2. **`const`** - o'zgarmas (qiymati faqat bir marta beriladigan va keyin o'zgarmaydigan) ma'lumotlar uchun ishlatiladi (masalan: pi soni, tug'ilgan yil).
3. **`var`** - eski usul. Hozirgi kunda kam ishlatiladi, uning o'rniga asosan `let` va `const` dan foydalanish tavsiya etiladi.

## Misollar:

```javascript
// let orqali o'zgaruvchi yaratish
let ism = "Ali";
console.log(ism); // Natija: Ali

// ism o'zgaruvchisining qiymatini o'zgartirish
ism = "Vali";
console.log(ism); // Natija: Vali

// const orqali o'zgarmas yaratish
const yosh = 20;
console.log(yosh); // Natija: 20

// yosh = 21; // XATO! const bilan yaratilgan o'zgaruvchini o'zgartirib bo'lmaydi.
```

## O'zgaruvchilarni nomlash qoidalari:
- Nom harf, `$` (dollar) belgisi yoki `_` (pastki chiziq) bilan boshlanishi kerak. Raqam bilan boshlash mumkin emas.
- Nom ichida raqamlar ishtirok etishi mumkin (masalan: `ism1`, `user_2`).
- Katta va kichik harflar farqlanadi (`ism` bilan `Ism` ikkita turli o'zgaruvchi hisoblanadi).
- JavaScript-ning o'ziga tegishli maxsus so'zlardan (masalan: `let`, `return`, `function`) nom sifatida foydalanib bo'lmaydi.

_O'zgaruvchilarga har doim ularning ichida qanday ma'lumot borligini tushuntirib beruvchi, ma'noli nomlar bering (masalan `x` o'rniga `foydalanuvchiYoshi`)._
