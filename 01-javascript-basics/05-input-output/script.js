// =====================================================
// 05 — Input / Output (Kiritish va Chiqarish)
// =====================================================


// -------------------------------------------------------
// 1. MA'LUMOT CHIQARISH — console.log()
// -------------------------------------------------------
console.log("=== 1. console.log() ===");

console.log("Salom, Dunyo!");
console.log(42);
console.log(3.14);
console.log(true);

// Bir nechta qiymat birga
console.log("Ism:", "Ali", "| Yosh:", 20);

// O'zgaruvchilar
let ism = "Umid";
let yosh = 22;
console.log(ism, yosh);

// Template literal (zamonaviy va qulay usul)
console.log(`Mening ismim ${ism}, yoshim ${yosh}`);

// Hisob natijasi
console.log("5 + 3 =", 5 + 3);
console.log(`10 * 4 = ${10 * 4}`);


// -------------------------------------------------------
// 2. BOSHQA KONSOL METODLARI
// -------------------------------------------------------
console.log("\n=== 2. Boshqa konsol metodlari ===");

// Ogohlantirish — sariq
console.warn("Bu ogohlantirish! (warn)");

// Xato — qizil
console.error("Bu xato xabari! (error)");

// Ma'lumot
console.info("Bu ma'lumot xabari (info)");

// Jadval ko'rinishida chiqarish
let talabalar = [
  { ism: "Ali",  baho: 90 },
  { ism: "Vali", baho: 85 },
  { ism: "Guli", baho: 95 },
];
console.table(talabalar);

// Guruhlash
console.group("Foydalanuvchi ma'lumotlari");
console.log("Ism: Sardor");
console.log("Yosh: 25");
console.log("Shahar: Toshkent");
console.groupEnd();


// -------------------------------------------------------
// 3. ALERT — Pop-up oyna
// -------------------------------------------------------
// console.log("\n=== 3. alert() ===");
// alert("Dasturga xush kelibsiz!");
// alert(`Siz ${yosh} yoshdasiz.`);
// ↑ Ishlatganda brauzer to'xtab qoladi, shuning uchun izohda


// -------------------------------------------------------
// 4. PROMPT — Foydalanuvchidan matn olish
// -------------------------------------------------------
// console.log("\n=== 4. prompt() ===");

// prompt() doim STRING qaytaradi!
// let ismKiritma = prompt("Ismingizni kiriting:");
// console.log(typeof ismKiritma); // "string"

// Raqam kerak bo'lsa Number() bilan aylantirish shart
// let yoshKiritma = Number(prompt("Yoshingizni kiriting:"));
// console.log(typeof yoshKiritma); // "number"

// Cancel bosilsa null qaytaradi
// let javob = prompt("Kiriting:");
// if (javob === null) {
//   console.log("Bekor qilindi");
// } else {
//   console.log("Kiritildi:", javob);
// }


// -------------------------------------------------------
// 5. CONFIRM — Tasdiqlash oynasi
// -------------------------------------------------------
// console.log("\n=== 5. confirm() ===");

// true yoki false qaytaradi
// let rozimi = confirm("Davom etasizmi?");
// if (rozimi) {
//   console.log("Foydalanuvchi rozi bo'ldi.");
// } else {
//   console.log("Foydalanuvchi rad etdi.");
// }


// -------------------------------------------------------
// 6. HTML ELEMENTGA YOZISH
// -------------------------------------------------------
console.log("\n=== 6. HTML elementga yozish ===");

// Agar HTML faylda <div id="chiqarish"></div> bo'lsa:
// document.getElementById("chiqarish").innerHTML = "<b>Salom!</b>";
// document.getElementById("chiqarish").textContent = "Faqat matn";

// innerHTML vs textContent farqi
let xabar = "<b>Qalin matn</b>";
console.log("innerHTML bo'lsa HTML ishlardi:", xabar);
console.log("textContent bo'lsa shunday chiqardi:", xabar); // teglari ham ko'rinadi


// -------------------------------------------------------
// 7. AMALIY MISOL — Sodda kiritma/chiqarish
// -------------------------------------------------------
console.log("\n=== 7. Amaliy misol ===");

// Foydalanuvchi kiritmasini simulatsiya qilish (prompt o'rniga)
function simulyatsiyaKiritma(qiymat) {
  return qiymat; // Real kodda: return prompt("...");
}

let birinchiSon = Number(simulyatsiyaKiritma("15"));
let ikkinchiSon = Number(simulyatsiyaKiritma("4"));

if (isNaN(birinchiSon) || isNaN(ikkinchiSon)) {
  console.error("Xato: raqam kiritilmadi!");
} else {
  console.log(`${birinchiSon} + ${ikkinchiSon} = ${birinchiSon + ikkinchiSon}`);
  console.log(`${birinchiSon} - ${ikkinchiSon} = ${birinchiSon - ikkinchiSon}`);
  console.log(`${birinchiSon} * ${ikkinchiSon} = ${birinchiSon * ikkinchiSon}`);

  if (ikkinchiSon !== 0) {
    console.log(`${birinchiSon} / ${ikkinchiSon} = ${birinchiSon / ikkinchiSon}`);
  } else {
    console.warn("Nolga bo'lish mumkin emas!");
  }
}


// -------------------------------------------------------
// 8. MASHQLAR UCHUN SHABLON
// -------------------------------------------------------
console.log("\n=== 8. Mashqlar ===");

// 1-mashq: Ismni o'zgartiring va salomlashtirib chiqaring
let mashq1Ism = "Sardor";
console.log(`Salom, ${mashq1Ism}! Xush kelibsiz.`);

// 2-mashq: Ikkita sonning kattasini chiqarish
let x = 47;
let y = 83;
if (x > y) {
  console.log(`Katta son: ${x}`);
} else if (y > x) {
  console.log(`Katta son: ${y}`);
} else {
  console.log("Sonlar teng!");
}

// 3-mashq: Juft/Toq tekshirish
let tekshirilganSon = 17;
if (tekshirilganSon % 2 === 0) {
  console.log(`${tekshirilganSon} — juft son`);
} else {
  console.log(`${tekshirilganSon} — toq son`);
}
