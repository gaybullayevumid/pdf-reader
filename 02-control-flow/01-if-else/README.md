# If - Else (Shartli operator)

Dasturlashda ko'pincha biror shartga qarab turlicha harakat qilishimiz kerak bo'ladi. Masalan: "Agar foydalanuvchi katta yoshli bo'lsa, kirish ruxsat etilsin, aks holda — rad etilsin." Aynan shu ishni `if-else` operatori bajaradi.

---

## if operatori

`if` kalit so'zi "agar" degan ma'noni anglatadi. Agar shart `true` (rost) bo'lsa, ichidagi kod bajariladi.

**Sintaksis:**

```js
if (shart) {
  // shart rost bo'lsa, shu yerga kiradi
}
```

**Misol:**

```js
let yosh = 20;

if (yosh >= 18) {
  console.log("Siz voyaga yetgansiz.");
}
```

Bu misolda `yosh >= 18` sharti tekshiriladi. `20 >= 18` rost bo'lgani uchun `"Siz voyaga yetgansiz."` chiqadi.

---

## else operatori

`else` kalit so'zi "aks holda" degan ma'noni anglatadi. `if` sharti yolg'on bo'lsa, `else` ichidagi kod bajariladi.

**Sintaksis:**

```js
if (shart) {
  // shart rost bo'lsa
} else {
  // shart yolg'on bo'lsa
}
```

**Misol:**

```js
let yosh = 15;

if (yosh >= 18) {
  console.log("Siz voyaga yetgansiz.");
} else {
  console.log("Siz hali voyaga yetmagansiz.");
}
```

`15 >= 18` yolg'on bo'lgani uchun `else` bloki ishlaydi va `"Siz hali voyaga yetmagansiz."` chiqadi.

---

## else if operatori

Bir nechta shartni ketma-ket tekshirish uchun `else if` ishlatiladi.

**Sintaksis:**

```js
if (birinchi shart) {
  // birinchi shart rost bo'lsa
} else if (ikkinchi shart) {
  // ikkinchi shart rost bo'lsa
} else {
  // hech biri rost bo'lmasa
}
```

**Misol:**

```js
let ball = 75;

if (ball >= 90) {
  console.log("A'lo");
} else if (ball >= 70) {
  console.log("Yaxshi");
} else if (ball >= 50) {
  console.log("Qoniqarli");
} else {
  console.log("Qoniqarsiz");
}
```

`ball = 75` bo'lgani uchun ikkinchi shart (`75 >= 70`) rost chiqadi va `"Yaxshi"` deb chiqadi.

> Shartlar yuqoridan pastga ketma-ket tekshiriladi. Birinchi rost shart topilishi bilan qolganlar tekshirilmaydi.

---

## Shartlarda ishlatiladigan operatorlar

Shartlar yozishda quyidagi taqqoslash operatorlaridan foydalanamiz:

| Operator | Ma'nosi              | Misol         |
|----------|----------------------|---------------|
| `==`     | teng                 | `5 == "5"` -> `true`  |
| `===`    | qat'iy teng          | `5 === "5"` -> `false` |
| `!=`     | teng emas            | `5 != 3` -> `true`    |
| `!==`    | qat'iy teng emas     | `5 !== "5"` -> `true` |
| `>`      | katta                | `10 > 5` -> `true`    |
| `<`      | kichik               | `3 < 8` -> `true`     |
| `>=`     | katta yoki teng      | `5 >= 5` -> `true`    |
| `<=`     | kichik yoki teng     | `4 <= 3` -> `false`   |

> `==` va `===` farqi: `==` faqat qiymatni, `===` esa qiymat va turni ham taqqoslaydi. Odatda `===` ishlatish tavsiya etiladi.

---

## Mantiqiy operatorlar

Ba'zan bir vaqtning o'zida bir nechta shartni tekshirish kerak bo'ladi:

| Operator | Ma'nosi | Misol |
|----------|---------|-------|
| `&&`     | va (ikkalasi ham rost bo'lishi kerak) | `yosh > 18 && fuqaro === true` |
| `\|\|`   | yoki (bittasi rost bo'lsa yetarli)   | `kun === "shanba" \|\| kun === "yakshanba"` |
| `!`      | emas (rostni yolg'onga, yolg'onni rostga aylantiradi) | `!false` -> `true` |

**Misol — && operatori:**

```js
let yosh = 22;
let fuqaro = true;

if (yosh >= 18 && fuqaro === true) {
  console.log("Ovoz berishingiz mumkin.");
} else {
  console.log("Ovoz berishga haqingiz yo'q.");
}
```

Ikki shart ham rost bo'lgani uchun `"Ovoz berishingiz mumkin."` chiqadi.

**Misol — || operatori:**

```js
let kun = "shanba";

if (kun === "shanba" || kun === "yakshanba") {
  console.log("Bugun dam olish kuni.");
} else {
  console.log("Bugun ish kuni.");
}
```

---

## Bir qatorli if (qisqa yozuv)

Agar `if` yoki `else` blokida faqat bitta buyruq bo'lsa, figurali qavslarni (`{}`) tushirib qoldirish mumkin. Lekin bu faqat juda oddiy holatlarda qo'llaniladi.

```js
let harorat = 35;

if (harorat > 30) console.log("Havo issiq.");
else console.log("Havo sovuq.");
```

> Ko'p qatorli kodlarda doim figurali qavslardan foydalanish tavsiya etiladi — bu xatoliklarni kamaytiradi.

---

## Uchlik operator (Ternary Operator)

Bu `if-else` ning qisqa yozuv shakli. Oddiy holatlar uchun qulay.

**Sintaksis:**

```js
shart ? "rost bo'lsa" : "yolg'on bo'lsa"
```

**Misol:**

```js
let yosh = 20;
let natija = yosh >= 18 ? "Katta" : "Kichik";
console.log(natija); // "Katta"
```

Bu quyidagiga to'liq teng:

```js
let natija;
if (yosh >= 18) {
  natija = "Katta";
} else {
  natija = "Kichik";
}
```

---

## Amaliy misol: Kalkulyator

```js
let a = 10;
let b = 0;
let amal = "/";

if (amal === "+") {
  console.log(a + b);
} else if (amal === "-") {
  console.log(a - b);
} else if (amal === "*") {
  console.log(a * b);
} else if (amal === "/") {
  if (b === 0) {
    console.log("Nolga bo'lish mumkin emas.");
  } else {
    console.log(a / b);
  }
} else {
  console.log("Noma'lum amal.");
}
```

Bu misolda `if` ichida yana `if` ishlatilgan — buni **ichma-ich if** (nested if) deyiladi.

---

## Xulosa

| Konstruktsiya | Qachon ishlatiladi |
|---------------|--------------------|
| `if`          | Faqat bitta shartni tekshirishda |
| `if-else`     | Rost va yolg'on uchun alohida kod kerak bo'lganda |
| `else if`     | Uch va undan ortiq holat bo'lganda |
| Ternary `? :` | Oddiy ikki holatli ifodalarni qisqa yozishda |

---

## Mashqlar

1. `raqam` o'zgaruvchisi musbat, manfiy yoki nol ekanligini ekranga chiqaring.
2. Uchburchak tomonlari berilgan. Tomonlar bir xil bo'lsa `"Tengtomonli"`, ikkitasi bir xil bo'lsa `"Ikki tengtomonli"`, hech biri bir xil bo'lmasa `"Har xil tomonli"` deb chiqaring.
3. Foydalanuvchi kiritgan sonning juft yoki toq ekanligini aniqlang.
