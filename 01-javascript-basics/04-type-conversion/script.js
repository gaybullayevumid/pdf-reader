// =====================================================
// 04 — Type Conversion (Ma'lumot turlarini o'zgartirish)
// =====================================================

// -------------------------------------------------------
// 1. YASHIRIN O'ZGARTIRISH (Implicit Conversion)
// -------------------------------------------------------
console.log("=== Yashirin O'zgartirish ===");

// + operatori: matn ustunlik qiladi
console.log("5" + 2);        // "52"
console.log(1 + 2 + "3");    // "33"
console.log("3" + 1 + 2);    // "312"

// -, *, /, % operatorlari: raqam ustunlik qiladi
console.log("10" - 5);       // 5
console.log("10" * "2");     // 20
console.log("15" / 3);       // 5
console.log("abc" - 1);      // NaN

// == vs ===
console.log("5" == 5);       // true  (tur o'zgartiradi)
console.log("5" === 5);      // false (qat'iy tekshiradi)


// -------------------------------------------------------
// 2. MATNGA AYLANTIRISH — String()
// -------------------------------------------------------
console.log("\n=== Matnga aylantirish — String() ===");

console.log(String(2024));      // "2024"
console.log(String(3.14));      // "3.14"
console.log(String(true));      // "true"
console.log(String(false));     // "false"
console.log(String(null));      // "null"
console.log(String(undefined)); // "undefined"

// Alternativ usullar
let son = 42;
console.log(son.toString());    // "42"
console.log(`${son}`);          // "42"
console.log(son + "");          // "42" (tavsiya etilmaydi)


// -------------------------------------------------------
// 3. RAQAMGA AYLANTIRISH — Number()
// -------------------------------------------------------
console.log("\n=== Raqamga aylantirish — Number() ===");

console.log(Number("42"));        // 42
console.log(Number("3.14"));      // 3.14
console.log(Number("  10  "));    // 10
console.log(Number(""));          // 0
console.log(Number("abc"));       // NaN
console.log(Number(true));        // 1
console.log(Number(false));       // 0
console.log(Number(null));        // 0
console.log(Number(undefined));   // NaN

// parseInt va parseFloat
console.log(parseInt("42.9"));    // 42
console.log(parseInt("10px"));    // 10
console.log(parseFloat("3.14")); // 3.14

// Unary + operatori
console.log(+"42");               // 42
console.log(+"");                 // 0


// -------------------------------------------------------
// 4. MANTIQIYGA AYLANTIRISH — Boolean()
// -------------------------------------------------------
console.log("\n=== Mantiqiyga aylantirish — Boolean() ===");

// Falsy qiymatlar (false ga aylanadi)
console.log(Boolean(0));          // false
console.log(Boolean(""));        // false
console.log(Boolean(null));       // false
console.log(Boolean(undefined));  // false
console.log(Boolean(NaN));        // false
console.log(Boolean(false));      // false

// Truthy qiymatlar (true ga aylanadi)
console.log(Boolean(1));          // true
console.log(Boolean(-1));         // true  ← diqqat!
console.log(Boolean("Salom"));   // true
console.log(Boolean("0"));        // true  ← diqqat! "0" matn, bo'sh emas
console.log(Boolean([]));         // true  ← diqqat! bo'sh massiv
console.log(Boolean({}));         // true  ← diqqat! bo'sh obyekt


// -------------------------------------------------------
// 5. AMALIY MISOL — Forma kiritmasini qayta ishlash
// -------------------------------------------------------
console.log("\n=== Amaliy misol ===");

// Foydalanuvchi kiritgan qiymat har doim matn bo'ladi
let kiritma = "25";                    // prompt() dan kelgan qiymat
console.log(typeof kiritma);           // "string"

let yosh = Number(kiritma);           // Raqamga aylantiramiz
console.log(typeof yosh);              // "number"

if (isNaN(yosh)) {
  console.log("Noto'g'ri kiritma!");
} else if (yosh < 0 || yosh > 120) {
  console.log("Yosh 0 dan 120 gacha bo'lishi kerak!");
} else {
  console.log(`Siz ${yosh} yoshdasiz.`);
}


// -------------------------------------------------------
// 6. TYPEOF OPERATORI
// -------------------------------------------------------
console.log("\n=== typeof operatori ===");

console.log(typeof 42);            // "number"
console.log(typeof "Salom");       // "string"
console.log(typeof true);          // "boolean"
console.log(typeof undefined);     // "undefined"
console.log(typeof null);          // "object"  ← JavaScript xatosi!
console.log(typeof {});            // "object"
console.log(typeof []);            // "object"  ← massiv ham object!
console.log(typeof function(){}); // "function"
