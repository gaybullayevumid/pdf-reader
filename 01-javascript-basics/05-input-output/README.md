# 05 — Kiritish va Chiqarish (Input / Output)

JavaScript dasturlari foydalanuvchi bilan ikki yo'nalishda ishlaydi:
- **Chiqarish (Output)** — dasturdan foydalanuvchiga ma'lumot ko'rsatish
- **Kiritish (Input)** — foydalanuvchidan ma'lumot olish

---

## Mavzu bo'limlari

1. [Ma'lumot Chiqarish (Output)](#1-malumot-chiqarish-output)
   - [`console.log()`](#11-consolelog--konsol-chiqarish)
   - [`console.warn()`, `console.error()`, `console.table()`](#12-boshqa-konsol-metodlari)
   - [`alert()`](#13-alert--ogohlantirishli-oyna)
   - [`document.write()`](#14-documentwrite)
   - [HTML elementga yozish](#15-html-elementga-yozish)
2. [Ma'lumot Kiritish (Input)](#2-malumot-kiritish-input)
   - [`prompt()`](#21-prompt--matn-kiritish)
   - [`confirm()`](#22-confirm--tasdiqlash)
   - [HTML forma orqali](#23-html-forma-orqali-kiritish)
3. [Amaliy loyiha](#3-amaliy-loyiha)
4. [Xulosa jadvali](#4-xulosa-jadvali)
5. [Mashqlar](#5-mashqlar)

---

## 1. Ma'lumot Chiqarish (Output)

---

### 1.1 `console.log()` — Konsol chiqarish

Eng ko'p ishlatiladigan usul. Natijani brauzerning **DevTools konsoliga** chiqaradi.  
`F12` tugmasi → **Console** tab orqali ko'rishingiz mumkin.

```javascript
// Matn chiqarish
console.log("Salom, Dunyo!");         // Salom, Dunyo!

// Raqam chiqarish
console.log(42);                       // 42

// Bir nechta qiymat birga
console.log("Ism:", "Ali", "Yosh:", 20); // Ism: Ali Yosh: 20

// O'zgaruvchi chiqarish
let ism = "Umid";
let yosh = 22;
console.log(ism, yosh);               // Umid 22

// Hisob natijasi
console.log(5 + 3);                   // 8
console.log("5" + 3);                 // "53" ← matn birlashtirish!
```

**Template literal bilan chiqarish (tavsiya etiladi):**

```javascript
let ism = "Sardor";
let yosh = 25;

// Eski usul (+ bilan birlashtirish)
console.log("Mening ismim " + ism + ", yoshim " + yosh);

// Yangi usul — template literal (oson va qulay)
console.log(`Mening ismim ${ism}, yoshim ${yosh}`);
```

> ✅ **Tavsiya:** Template literal (` `` `) dan foydalaning — kodi ancha o'qilishi oson.

---

### 1.2 Boshqa konsol metodlari

`console` obyektida boshqa foydali metodlar ham bor:

```javascript
// console.warn() — sariq rangli ogohlantirish
console.warn("Bu ogohlantirish xabari!");

// console.error() — qizil rangli xato xabari
console.error("Bu xato xabari!");

// console.info() — ma'lumot xabari (ba'zi brauzerlarda ko'k)
console.info("Bu ma'lumot xabari.");

// console.table() — massiv yoki obyektni jadval ko'rinishida chiqaradi
let talabalar = [
  { ism: "Ali", baho: 90 },
  { ism: "Vali", baho: 85 },
  { ism: "Guli", baho: 95 },
];
console.table(talabalar);

// console.clear() — konsolni tozalash
// console.clear();

// console.group() — guruhlash
console.group("Foydalanuvchi ma'lumotlari");
console.log("Ism: Ali");
console.log("Yosh: 20");
console.groupEnd();
```

---

### 1.3 `alert()` — Ogohlantirishli oyna

Foydalanuvchiga **pop-up oyna** orqali xabar ko'rsatadi.  
Foydalanuvchi **OK** bosmaguncha kod to'xtab turadi.

```javascript
alert("Dasturga xush kelibsiz!");

let ball = 95;
alert(`Siz ${ball} ball to'pladingiz. Tabriklaymiz!`);
```

> ⚠️ **Kamchiliklari:**
> - Brauzer butunlay to'xtab qoladi (bloklanadi)
> - Ko'p ishlatilsa foydalanuvchini bezovta qiladi
> - Ko'rinishi har bir brauzerda har xil
>
> Shuning uchun **haqiqiy loyihalarda `alert()` kamdan-kam ishlatiladi.**

---

### 1.4 `document.write()`

To'g'ridan-to'g'ri HTML sahifaga matn yoki HTML yozadi.

```javascript
document.write("Sahifaga yozilgan matn");
document.write("<h2>Bu sarlavha</h2>");
document.write("<p style='color:red'>Qizil matn</p>");
```

> ⚠️ **Muhim:** `document.write()` sahifa yuklangandan **keyin** chaqirilsa, sahifadagi barcha mavjud kontent **o'chiriladi!**

```javascript
// Sahifa yuklangandan keyin chaqirilsa HAMMA NARSA O'CHADI
window.onload = function() {
  document.write("Barcha kontent o'chirildi!"); // Xavfli!
};
```

> ❌ **Hozirgi kunda `document.write()` ishlatish tavsiya etilmaydi.** O'rniga quyidagi usuldan foydalaning.

---

### 1.5 HTML elementga yozish

Zamonaviy va to'g'ri usul — HTML elementni topib, uning ichiga yozish:

```html
<!-- HTML -->
<p id="natija"></p>
<div id="xabar"></div>
```

```javascript
// innerHTML — HTML kodi bilan yozish
document.getElementById("natija").innerHTML = "<b>Natija: 42</b>";

// textContent — faqat matn (HTML teglari ishlamaydi, xavfsizroq)
document.getElementById("xabar").textContent = "Salom, Dunyo!";
```

**Farqi:**

| Xususiyat | `innerHTML` | `textContent` |
|-----------|-------------|---------------|
| HTML teglari | ✅ Ishlaydi | ❌ Ishlamaydi (matn sifatida chiqaradi) |
| Xavfsizlik | ⚠️ XSS xavfi bor | ✅ Xavfsiz |
| Tezlik | Sekinroq | Tezroq |

> ✅ **Tavsiya:** Foydalanuvchi kiritmasini chiqarayotganda **`textContent`** ishlating (xavfsiz).

---

## 2. Ma'lumot Kiritish (Input)

---

### 2.1 `prompt()` — Matn kiritish

Foydalanuvchiga **kiritish maydoni** bo'lgan oyna chiqaradi.

```javascript
// Asosiy ishlatilishi
let ism = prompt("Ismingizni kiriting:");
console.log("Salom, " + ism);

// Ikkinchi parametr — default qiymat
let shahar = prompt("Shahringizni kiriting:", "Toshkent");
console.log("Shahar:", shahar);
```

**Muhim xususiyatlar:**

```javascript
// prompt() DOIM string qaytaradi!
let son = prompt("Raqam kiriting:");
console.log(typeof son);    // "string" ← raqam emas!
console.log(son + 5);       // "105" ← noto'g'ri!

// Raqam kerak bo'lsa, Number() bilan aylantirish shart
let raqam = Number(prompt("Raqam kiriting:"));
console.log(typeof raqam);  // "number"
console.log(raqam + 5);     // 15 ← to'g'ri!

// Foydalanuvchi "Cancel" bossa — null qaytaradi
let javob = prompt("Kiriting:");
if (javob === null) {
  console.log("Foydalanuvchi bekor qildi.");
} else {
  console.log("Kiritildi:", javob);
}
```

**To'liq misol — yoshni so'rash:**

```javascript
let yoshMatni = prompt("Yoshingizni kiriting:");
let yosh = Number(yoshMatni);

if (yoshMatni === null) {
  console.log("Bekor qilindi.");
} else if (isNaN(yosh) || yosh <= 0) {
  console.log("Iltimos, to'g'ri yosh kiriting!");
} else if (yosh < 18) {
  console.log(`${yosh} yoshda hali voyaga yetmagansiz.`);
} else {
  console.log(`${yosh} yoshda voyaga yetgansiz!`);
}
```

---

### 2.2 `confirm()` — Tasdiqlash

Foydalanuvchiga **Ha/Yo'q** savol beradi.  
- **OK** → `true` qaytaradi  
- **Cancel** → `false` qaytaradi

```javascript
// Asosiy ishlatilishi
let rozimi = confirm("Sayt qoidalariga rozimisiz?");
console.log(rozimi);    // true yoki false

// if bilan birga
if (confirm("O'chirishni xohlaysizmi?")) {
  console.log("Ma'lumot o'chirildi.");
} else {
  console.log("Bekor qilindi.");
}

// Amaliy misol — sahifadan chiqish
let chiqish = confirm("Haqiqatan ham chiqmoqchimisiz?");
if (chiqish) {
  console.log("Xayr!");
  // window.location.href = "https://google.com";
}
```

---

### 2.3 HTML Forma orqali kiritish

Haqiqiy loyihalarda `prompt()` emas, **HTML input** elementlari ishlatiladi:

```html
<!-- HTML -->
<input type="text" id="ismInput" placeholder="Ismingizni kiriting">
<button id="yuborBtn">Yuborish</button>
<p id="natija"></p>
```

```javascript
// JavaScript
let btn = document.getElementById("yuborBtn");

btn.addEventListener("click", function() {
  let ism = document.getElementById("ismInput").value;

  if (ism.trim() === "") {
    document.getElementById("natija").textContent = "Ism bo'sh bo'lmasin!";
  } else {
    document.getElementById("natija").textContent = `Salom, ${ism}!`;
  }
});
```

> ✅ **Haqiqiy loyihalarda har doim HTML forma ishlating.**  
> `prompt()` va `confirm()` — faqat tez-tez sinov yoki o'rganish uchun.

---

## 3. Amaliy Loyiha

**Mini kalkulyator** — foydalanuvchidan ikki son olib, to'rtta amalni bajaradi:

```javascript
// Foydalanuvchidan son olish
let son1 = Number(prompt("Birinchi sonni kiriting:"));
let son2 = Number(prompt("Ikkinchi sonni kiriting:"));

// Kiritma tekshiruvi
if (isNaN(son1) || isNaN(son2)) {
  alert("Iltimos, faqat raqam kiriting!");
} else {
  // Hisob-kitob
  let qoshish  = son1 + son2;
  let ayirish  = son1 - son2;
  let kopaytirish = son1 * son2;
  let bolish   = son2 !== 0 ? son1 / son2 : "nolga bo'lish mumkin emas";

  // Natijani chiqarish
  console.log(`${son1} + ${son2} = ${qoshish}`);
  console.log(`${son1} - ${son2} = ${ayirish}`);
  console.log(`${son1} * ${son2} = ${kopaytirish}`);
  console.log(`${son1} / ${son2} = ${bolish}`);

  alert(
    `Natijalar:\n` +
    `Qo'shish: ${qoshish}\n` +
    `Ayirish: ${ayirish}\n` +
    `Ko'paytirish: ${kopaytirish}\n` +
    `Bo'lish: ${bolish}`
  );
}
```

---

## 4. Xulosa Jadvali

| Metod | Turi | Qaytaradi | Qachon ishlatiladi |
|-------|------|-----------|-------------------|
| `console.log()` | Chiqarish | — | Debug, tekshirish |
| `console.warn()` | Chiqarish | — | Ogohlantirish |
| `console.error()` | Chiqarish | — | Xato xabari |
| `console.table()` | Chiqarish | — | Jadval ko'rinish |
| `alert()` | Chiqarish | — | Muhim xabar (kam) |
| `document.write()` | Chiqarish | — | ❌ Tavsiya etilmaydi |
| `element.innerHTML` | Chiqarish | — | HTML bilan yozish |
| `element.textContent` | Chiqarish | — | Xavfsiz matn yozish |
| `prompt()` | Kiritish | `string \| null` | Tez sinov/o'rganish |
| `confirm()` | Kiritish | `boolean` | Tasdiqlash |
| `<input>` + `.value` | Kiritish | `string` | ✅ Haqiqiy loyiha |

---

## 5. Mashqlar

```javascript
// 1-mashq: Foydalanuvchidan ism so'rab, "Salom, [ism]! Xush kelibsiz." chiqaring.


// 2-mashq: Foydalanuvchidan ikkita raqam so'rab, kattasini console.log bilan chiqaring.


// 3-mashq: confirm() bilan foydalanuvchidan biror amalni tasdiqlating.
// OK bossa "Tasdiqlandi", Cancel bossa "Bekor qilindi" deb console.log chiqaring.


// 4-mashq: prompt() orqali son kiriting, u toq mi yoki juft mi ekanini aniqlang.
// (Maslahat: son % 2 === 0 bo'lsa juft)


// 5-mashq (qiyin): Foydalanuvchi "0" kiritmagunicha takroran raqam so'rang va
// ularning yig'indisini chiqaring. (while loop kerak bo'ladi)
```

<details>
<summary>✅ Javoblarni ko'rish (oldin o'zingiz urinib ko'ring!)</summary>

```javascript
// 1-mashq javobi:
let ism = prompt("Ismingizni kiriting:");
console.log(`Salom, ${ism}! Xush kelibsiz.`);

// 2-mashq javobi:
let a = Number(prompt("Birinchi raqam:"));
let b = Number(prompt("Ikkinchi raqam:"));
if (a > b) {
  console.log("Kattasi:", a);
} else {
  console.log("Kattasi:", b);
}

// 3-mashq javobi:
if (confirm("Davom etasizmi?")) {
  console.log("Tasdiqlandi");
} else {
  console.log("Bekor qilindi");
}

// 4-mashq javobi:
let son = Number(prompt("Son kiriting:"));
if (son % 2 === 0) {
  console.log(`${son} — juft son`);
} else {
  console.log(`${son} — toq son`);
}

// 5-mashq javobi:
let jami = 0;
while (true) {
  let kiritma = Number(prompt("Raqam kiriting (0 — to'xtatish):"));
  if (kiritma === 0) break;
  jami += kiritma;
}
console.log("Yig'indi:", jami);
```

</details>

---

## Keyingi mavzu

➡️ **[06 — Shart Operatorlari (Conditionals)](../06-conditionals/README.md)**
