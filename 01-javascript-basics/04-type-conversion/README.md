# 04 — Ma'lumot Turlarini O'zgartirish (Type Conversion)

JavaScript-da ma'lumotlar turli ko'rinishda bo'ladi: **raqam**, **matn**, **mantiqiy** va boshqalar.  
Ba'zida bir turni boshqa turga o'tkazishimiz kerak bo'ladi — buni **Type Conversion** deyiladi.

---

## Mavzu bo'limlari

1. [Nima uchun kerak?](#1-nima-uchun-kerak)
2. [JavaScript'dagi asosiy turlar](#2-javascriptdagi-asosiy-turlar)
3. [Yashirin o'zgartirish (Implicit)](#3-yashirin-ozgartirish-implicit)
4. [Ochiq o'zgartirish (Explicit)](#4-ochiq-ozgartirish-explicit)
   - [Matnga aylantirish — `String()`](#41-matnga-aylantirish--string)
   - [Raqamga aylantirish — `Number()`](#42-raqamga-aylantirish--number)
   - [Mantiqiyga aylantirish — `Boolean()`](#43-mantiqqiya-aylantirish--boolean)
5. [`typeof` operatori](#5-typeof-operatori)
6. [Xulosa jadvali](#6-xulosa-jadvali)
7. [Mashqlar](#7-mashqlar)

---

## 1. Nima uchun kerak?

Tasavvur qiling: foydalanuvchi forma orqali **yosh** kirityapti.  
Brauzer bu qiymatni har doim **matn (string)** sifatida qaytaradi, hatto `"25"` bo'lsa ham.

```javascript
let yosh = "25";          // Bu matn, raqam emas!
console.log(yosh + 5);    // "255"  ← noto'g'ri natija
```

Agar biz uni raqamga aylantirmasak, hisob-kitob noto'g'ri chiqadi.  
Shuning uchun **Type Conversion** juda muhim.

---

## 2. JavaScript'dagi asosiy turlar

| Tur        | Misol                         |
|------------|-------------------------------|
| `string`   | `"Salom"`, `'Dunyo'`, `` `Hi` `` |
| `number`   | `42`, `3.14`, `-7`            |
| `boolean`  | `true`, `false`               |
| `null`     | `null`                        |
| `undefined`| `undefined`                   |
| `object`   | `{}`, `[]`                    |

Qaysi turda ekanligini bilish uchun `typeof` operatoridan foydalanamiz:

```javascript
console.log(typeof "Salom");    // "string"
console.log(typeof 42);         // "number"
console.log(typeof true);       // "boolean"
console.log(typeof undefined);  // "undefined"
console.log(typeof null);       // "object"  ← bu JavaScript'dagi tarixiy xato!
```

---

## 3. Yashirin O'zgartirish (Implicit Conversion)

JavaScript ba'zi hollarda turni **o'zi avtomatik** o'zgartiradi.  
Bu yashirin o'zgartirish deyiladi. Ko'pincha kutilmagan natijalar chiqarishi mumkin!

### `+` operatori — matn ustunlik qiladi

```javascript
console.log("5" + 2);      // "52"   ← 2 ni matnga aylantirdi
console.log("Yosh: " + 25); // "Yosh: 25"
console.log(1 + 2 + "3");  // "33"   ← 1+2=3, keyin "3" bilan birlashdi
console.log("3" + 1 + 2);  // "312"  ← "3"+1="31", keyin "31"+2="312"
```

> ⚠️ `+` operatori matn bilan ishlashda **birlashtiradi (concatenation)**, qo'shmaydi!

### `-`, `*`, `/`, `%` operatorlari — raqam ustunlik qiladi

```javascript
console.log("10" - 5);     // 5    ← "10" raqamga aylandi
console.log("10" * "2");   // 20   ← ikkalasi ham raqamga aylandi
console.log("15" / 3);     // 5
console.log("10" % 3);     // 1
console.log("abc" - 1);    // NaN  ← "abc" raqamga aylanmadi
```

### Taqqoslash operatorlarida

```javascript
console.log("5" == 5);     // true  ← == turni o'zgartiradi
console.log("5" === 5);    // false ← === tur va qiymatni bir vaqtda tekshiradi
```

> ✅ **Tavsiya:** Har doim `===` (qat'iy tenglik) ishlatavering, `==` dan saqlaning!

---

## 4. Ochiq O'zgartirish (Explicit Conversion)

Biz o'zimiz maxsus funksiyalar yordamida turni ataylab o'zgartiramiz.

---

### 4.1 Matnga aylantirish — `String()`

Istalgan qiymatni matnga aylantiradi.

```javascript
// Raqamdan matnga
let son = 2024;
let matn = String(son);
console.log(matn);          // "2024"
console.log(typeof matn);   // "string"

// Boolean'dan matnga
console.log(String(true));  // "true"
console.log(String(false)); // "false"

// null va undefined
console.log(String(null));      // "null"
console.log(String(undefined)); // "undefined"
```

**Alternativ usullar:**

```javascript
let son = 42;

// .toString() metodi
console.log(son.toString());     // "42"

// Template literal (backtick)
console.log(`${son}`);           // "42"

// Bo'sh matnga qo'shish (yashirin, tavsiya etilmaydi)
console.log(son + "");           // "42"
```

> ✅ **Eng yaxshi amaliyot:** `String()` yoki template literal `` `${value}` `` ishlatish.

---

### 4.2 Raqamga aylantirish — `Number()`

Matn yoki boshqa turlarni raqamga aylantiradi.

```javascript
// Matni raqamga
console.log(Number("42"));       // 42
console.log(Number("3.14"));     // 3.14
console.log(Number("  10  "));   // 10  ← bo'sh joylar kesib tashlanadi
console.log(Number(""));         // 0   ← bo'sh matn 0 ga teng
console.log(Number("abc"));      // NaN ← raqamga aylanmadi

// Boolean'dan raqamga
console.log(Number(true));       // 1
console.log(Number(false));      // 0

// null va undefined
console.log(Number(null));       // 0
console.log(Number(undefined));  // NaN
```

**Alternativ usullar:**

```javascript
// parseInt() — faqat butun son qaytaradi
console.log(parseInt("42.9"));   // 42   ← kasr qismini o'chiradi
console.log(parseInt("10px"));   // 10   ← raqamdan keyingi harflarni o'chiradi
console.log(parseInt("px10"));   // NaN  ← bosh harf bo'lsa NaN

// parseFloat() — kasr sonlarni saqlaydi
console.log(parseFloat("3.14")); // 3.14
console.log(parseFloat("3.14abc")); // 3.14

// Unary + operatori (qisqa yozuv)
console.log(+"42");              // 42
console.log(+"3.14");            // 3.14
console.log(+"");                // 0
```

> ✅ **Foydalanuvchi kiritmasini o'zgartirish uchun:** `Number()` yoki `parseFloat()`

---

### 4.3 Mantiqiyga aylantirish — `Boolean()`

Har qanday qiymatni `true` yoki `false` ga aylantiradi.

```javascript
// FALSE qaytaradigan qiymatlar (Falsy qiymatlar)
console.log(Boolean(0));          // false
console.log(Boolean(-0));         // false
console.log(Boolean(""));        // false — bo'sh matn
console.log(Boolean(null));       // false
console.log(Boolean(undefined));  // false
console.log(Boolean(NaN));        // false
console.log(Boolean(false));      // false

// TRUE qaytaradigan qiymatlar (Truthy qiymatlar)
console.log(Boolean(1));          // true
console.log(Boolean(-1));         // true  ← manfiy raqam ham true!
console.log(Boolean("Salom"));    // true
console.log(Boolean("0"));        // true  ← "0" MATN, bo'sh emas, shuning uchun true!
console.log(Boolean([]));         // true  ← bo'sh massiv ham true!
console.log(Boolean({}));         // true  ← bo'sh obyekt ham true!
```

> ⚠️ **Yangi boshlovchilar uchun keng tarqalgan xatolar:**
> - `Boolean("0")` → `true`  (matn bo'sh emas!)
> - `Boolean([])` → `true`   (bo'sh massiv bo'lsa ham!)
> - `Boolean(-1)` → `true`   (manfiy raqam bo'lsa ham!)

**Falsy qiymatlarni eslab qolish uchun jadval:**

| Qiymat      | Boolean natija |
|-------------|---------------|
| `0`         | `false`       |
| `""`        | `false`       |
| `null`      | `false`       |
| `undefined` | `false`       |
| `NaN`       | `false`       |
| `false`     | `false`       |
| **Boshqa hamma narsa** | **`true`** |

---

## 5. `typeof` Operatori

Qiymatning turini aniqlash uchun ishlatiladi.

```javascript
console.log(typeof 42);           // "number"
console.log(typeof "Salom");      // "string"
console.log(typeof true);         // "boolean"
console.log(typeof undefined);    // "undefined"
console.log(typeof null);         // "object"  ← JavaScript xatosi
console.log(typeof {});           // "object"
console.log(typeof []);           // "object"  ← massiv ham object!
console.log(typeof function(){}); // "function"
```

**Amaliy misol — foydalanuvchi kiritmasini tekshirish:**

```javascript
let kiritma = prompt("Raqam kiriting:");   // "42" — matn sifatida keladi
console.log(typeof kiritma);               // "string"

let raqam = Number(kiritma);              // Raqamga aylantiramiz
console.log(typeof raqam);                // "number"

if (isNaN(raqam)) {
  console.log("Iltimos, to'g'ri raqam kiriting!");
} else {
  console.log("Siz kiritgan raqam:", raqam);
}
```

---

## 6. Xulosa Jadvali

| Asl qiymat    | `String(x)`     | `Number(x)` | `Boolean(x)` |
|---------------|-----------------|-------------|--------------|
| `0`           | `"0"`           | `0`         | `false`      |
| `1`           | `"1"`           | `1`         | `true`       |
| `""`          | `""`            | `0`         | `false`      |
| `"0"`         | `"0"`           | `0`         | **`true`**   |
| `"42"`        | `"42"`          | `42`        | `true`       |
| `"abc"`       | `"abc"`         | `NaN`       | `true`       |
| `true`        | `"true"`        | `1`         | `true`       |
| `false`       | `"false"`       | `0`         | `false`      |
| `null`        | `"null"`        | `0`         | `false`      |
| `undefined`   | `"undefined"`   | `NaN`       | `false`      |
| `[]`          | `""`            | `0`         | **`true`**   |
| `{}`          | `"[object Object]"` | `NaN`   | **`true`**   |

---

## 7. Mashqlar

Quyidagi savollarga javob bering (konsolda tekshiring):

```javascript
// 1-mashq: Quyidagi ifodalar qanday natija beradi?
console.log("3" + 4 + 5);      // ?
console.log(3 + 4 + "5");      // ?

// 2-mashq: Natijani oldin taxmin qiling, keyin tekshiring
console.log(Number(""));        // ?
console.log(Number("  "));      // ?
console.log(Number(null));      // ?
console.log(Number(undefined)); // ?

// 3-mashq: Boolean qiymatlari
console.log(Boolean("false"));  // ? (diqqat!)
console.log(Boolean("0"));      // ? (diqqat!)
console.log(Boolean([]));       // ? (diqqat!)

// 4-mashq: Bu funksiya nima qiladi?
function sonMi(qiymat) {
  return !isNaN(Number(qiymat));
}
console.log(sonMi("42"));   // ?
console.log(sonMi("abc"));  // ?
console.log(sonMi(""));     // ?
```

<details>
<summary>✅ Javoblarni ko'rish (oldin o'zingiz urinib ko'ring!)</summary>

```javascript
// 1-mashq javobi:
"3" + 4 + 5   // → "345"  (chapdan o'ngga: "3"+4="34", "34"+5="345")
3 + 4 + "5"   // → "75"   (chapdan o'ngga: 3+4=7, 7+"5"="75")

// 2-mashq javobi:
Number("")        // → 0
Number("  ")      // → 0   (faqat bo'sh joylar ham 0)
Number(null)      // → 0
Number(undefined) // → NaN

// 3-mashq javobi:
Boolean("false")  // → true  (bo'sh bo'lmagan matn)
Boolean("0")      // → true  (bo'sh bo'lmagan matn)
Boolean([])       // → true  (bo'sh massiv bo'lsa ham)

// 4-mashq javobi:
sonMi("42")   // → true
sonMi("abc")  // → false
sonMi("")     // → false  ... yo'q, aslida TRUE! Chunki Number("")=0, isNaN(0)=false
```

</details>

---

## Keyingi mavzu

➡️ **[05 — Operatorlar (Operators)](../05-operators/README.md)**
