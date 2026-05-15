# 01 — JavaScript Asoslari: Topshiriqlar To'plami

> Har bir mavzu bo'yicha **20 ta topshiriq** — jami **100 ta**.  
> Topshiriqlar oson (**⭐**), o'rta (**⭐⭐**) va qiyin (**⭐⭐⭐**) darajalarga bo'lingan.

---

## 📁 01 — O'zgaruvchilar (Variables)

### ⭐ Oson

**1.** `var`, `let` va `const` dan foydalanib, o'zingizning ismingizni, yoshingizni va shahringizni saqlang. Ularni `console.log` bilan chiqaring.

**2.** `let` bilan `sana` nomli o'zgaruvchi yarating va unga bugungi sanani (matn ko'rinishida) belgilang.

**3.** `const` bilan `PI` nomli o'zgaruvchi yarating va unga `3.14159` qiymatini bering. Uni o'zgartirishga urinib ko'ring — nima xato chiqadi?

**4.** Quyidagi kodni o'qib, har bir `console.log` nima chiqarishini taxmin qiling, keyin tekshiring:
```javascript
let a = 10;
let b = a;
a = 20;
console.log(a);
console.log(b);
```

**5.** `let` bilan bo'sh o'zgaruvchi (qiymatsiz) e'lon qiling. Uning qiymati qanday bo'ladi?

**6.** Bitta o'zgaruvchiga avval raqam, keyin matn, keyin `true` belgilang. Har safar turini `typeof` bilan chiqaring.

**7.** Ikkita o'zgaruvchi yarating: `x = 5` va `y = 10`. Uchinchi o'zgaruvchi `z` ga ularning yig'indisini belgilang.

### ⭐⭐ O'rta

**8.** `const` bilan massiv (array) yarating: `const mevalar = ["olma", "nok", "banan"]`. Massivga yangi meva qo'shing. `const` bo'lishiga qaramay nima uchun qo'shish mumkin?

**9.** Bir xil nomli ikkita `let` o'zgaruvchi e'lon qilishga urinib ko'ring. Xato beradi yoki yo'qmi? `var` bilan ham sinab ko'ring — farqi nimada?

**10.** Quyidagi kodni xatosiz ishlating:
```javascript
console.log(ism);
let ism = "Ali";
```
Keyin `let` o'rniga `var` bilan almashtiring. Natija farq qiladimi? Nima uchun?

**11.** `let` bilan `hisoblagich = 0` yarating. Uni 5 marta `hisoblagich++` bilan oshiring va har safar qiymatini chiqaring.

**12.** `const` bilan obyekt yarating:
```javascript
const foydalanuvchi = { ism: "Kamol", yosh: 30 };
```
`yosh` ni o'zgartiring. Mumkinmi? Nima uchun?

### ⭐⭐⭐ Qiyin

**13.** `var` ning **hoisting** xususiyatini tushuntirib bering va misol yozing. `let` da hoisting bor, lekin nima uchun xato beradi?

**14.** Temporal Dead Zone (TDZ) nima? `let` va `const` uchun misol keltiring.

**15.** Quyidagi kodni tahlil qiling — natija nima va nima uchun?
```javascript
let i = 1;
{
  let i = 2;
  console.log(i);
}
console.log(i);
```

**16.** Global scope va local scope farqini misol bilan ko'rsating. Funksiya ichidagi o'zgaruvchi tashqaridan ko'rinadimi?

**17.** `const` bilan e'lon qilingan obyekt ichidagi massivga elementlar qo'shing va o'chiring. Bu nima uchun mumkin, lekin obyektni butunlay qayta belgilash mumkin emas?

**18.** Quyidagi kod nima chiqaradi? Oldin taxmin qiling:
```javascript
var x = 1;
function test() {
  console.log(x);
  var x = 2;
  console.log(x);
}
test();
console.log(x);
```

**19.** Quyidagi `for` siklida `var` va `let` ning farqini ko'rsating:
```javascript
for (var i = 0; i < 3; i++) {
  setTimeout(() => console.log(i), 100);
}
```
`var` ni `let` ga almashtiring — natija qanday o'zgaradi?

**20.** Yaxshi nomlash qoidalarini amalda qo'llang: `camelCase`, mazmunli nomlar, `UPPER_CASE` konstantalar. 5 ta o'zgaruvchi e'lon qiling — ulardan 2 tasi noto'g'ri nomlangan bo'lsin va xatoni toping.

---

## 📁 02 — Ma'lumot Turlari (Data Types)

### ⭐ Oson

**1.** Quyidagi har bir qiymatning turini `typeof` bilan aniqlang va natijani yozing:
```javascript
42, "salom", true, null, undefined, [], {}, function(){}
```

**2.** Matn (string) o'zgaruvchisi yarating va `.length` xususiyati orqali uzunligini chiqaring.

**3.** Quyidagi stringlar bilan ishlang:
- Bosh harfga aylantirish: `.toUpperCase()`
- Kichik harfga: `.toLowerCase()`
- Bo'sh joylarni kesish: `.trim()`

**4.** `null` va `undefined` ning farqini misol bilan ko'rsating. Ikkalasini `==` va `===` bilan taqqoslang.

**5.** `NaN` nima? `typeof NaN` nima qaytaradi? `isNaN()` funksiyasi bilan tekshiring.

**6.** Butun va kasr sonlar yarating. `Number.isInteger()` bilan tekshiring.

**7.** Quyidagilarni hisoblang:
```javascript
console.log(0.1 + 0.2);  // Nima chiqadi? Nima uchun?
```

### ⭐⭐ O'rta

**8.** Template literal yordamida ko'p qatorli matn yarating (backtick ichida Enter bosish).

**9.** String metodlaridan foydalaning:
- `.indexOf("so'z")` — joylashuvini toping
- `.includes("so'z")` — bor-yo'qligini tekshiring
- `.slice(0, 5)` — qismini oling
- `.replace("eski", "yangi")` — almashtiring

**10.** Quyidagi sonlar bilan ishlang:
```javascript
console.log(Infinity);        // ?
console.log(-Infinity);       // ?
console.log(1 / 0);          // ?
console.log(-1 / 0);         // ?
console.log(0 / 0);          // ?
```

**11.** `Number.MAX_SAFE_INTEGER` va `Number.MIN_SAFE_INTEGER` qiymatlari nima? Ulardan kattaroq sonlarda nima muammo paydo bo'ladi?

**12.** Quyidagi massiv (array) va obyekt (object) bilan ishlang:
```javascript
let mevalar = ["olma", "nok", "uzum"];
let odam = { ism: "Zulfiya", yosh: 28 };
```
Massivning uzunligi va obyektning kalitlarini chiqaring.

### ⭐⭐⭐ Qiyin

**13.** **Primitive** va **Reference** turlar farqini tushuntiring. Quyidagi kod nima chiqaradi?
```javascript
let a = [1, 2, 3];
let b = a;
b.push(4);
console.log(a);  // ?
console.log(b);  // ?
```

**14.** Massivni nusxalash (copy) uchun 3 xil usul yozing:
- Spread operator `[...arr]`
- `Array.from(arr)`
- `.slice()`

**15.** Obyektni nusxalash uchun 3 xil usul yozing:
- `Object.assign({}, obj)`
- Spread `{...obj}`
- `JSON.parse(JSON.stringify(obj))`

**16.** `Symbol` turi nima? Qachon ishlatiladi? Misol yozing.

**17.** `BigInt` turi nima? `Number.MAX_SAFE_INTEGER + 1` ni `BigInt` bilan hisoblang.

**18.** Quyidagi ifoda nima qaytaradi va nima uchun?
```javascript
typeof null === "object"    // ?
typeof NaN === "number"     // ?
typeof [] === "object"      // ?
typeof function(){} === "function" // ?
```

**19.** `Array.isArray()` funksiyasi nima uchun kerak? `typeof []` yetarli emasmi?

**20.** Chuqur nusxalash (deep clone) va sayoz nusxalash (shallow clone) farqini tushuntiring. Ichma-ich obyekt uchun muammo toping:
```javascript
let asl = { ism: "Ali", manzil: { shahar: "Toshkent" } };
let nusxa = { ...asl };
nusxa.manzil.shahar = "Samarqand";
console.log(asl.manzil.shahar); // ?
```

---

## 📁 03 — Operatorlar (Operators)

### ⭐ Oson

**1.** Ikki sonni qabul qilib, to'rtta matematik amalning natijasini chiqaring: `+`, `-`, `*`, `/`.

**2.** `%` (qoldiq) operatoridan foydalanib, son juft yoki toq ekanini aniqlang.

**3.** Quyidagilarni hisoblang va natijani taxmin qiling:
```javascript
console.log(10 / 3);    // ?
console.log(10 % 3);    // ?
console.log(2 ** 8);    // ?
```

**4.** `++` va `--` operatorlarining prefiksli (`++x`) va postfiksli (`x++`) farqini misolda ko'rsating.

**5.** `+=`, `-=`, `*=`, `/=` operatorlarini har birini misol bilan ishlatib ko'ring.

**6.** Quyidagi taqqoslash ifodalarining natijasini yozing:
```javascript
console.log(5 > 3);     // ?
console.log(5 >= 5);    // ?
console.log(3 != 4);    // ?
console.log(3 !== "3"); // ?
```

**7.** `&&` (VA) va `||` (YOKI) operatorlarini ishlating. 3 ta mantiqiy ifoda yozing.

### ⭐⭐ O'rta

**8.** `==` va `===` ning farqini 5 ta misol bilan ko'rsating — har birida natija farq qiladimi?

**9.** Uchlik operator (ternary `? :`) bilan 2 sondan kattasini toping.

**10.** `&&` va `||` operatorlari **qisqa tutashuv (short-circuit)** tamoyili bilan ishlaydi. Quyidagi ifodalar nima qaytaradi?
```javascript
console.log(0 && "Salom");      // ?
console.log(1 && "Salom");      // ?
console.log(0 || "Salom");      // ?
console.log("" || "Default");   // ?
```

**11.** **Nullish coalescing** operatori `??` nima? `||` dan farqi nimada?
```javascript
let ism = null;
console.log(ism || "Noma'lum");   // ?
console.log(ism ?? "Noma'lum");   // ?

let son = 0;
console.log(son || 10);    // ?
console.log(son ?? 10);    // ?
```

**12.** Bit operatorlari (`&`, `|`, `^`, `~`, `<<`, `>>`) ichidan faqat `&` va `|` ni 4 va 6 sonlari uchun qo'llang va ikkilik tizimda tushuntiring.

### ⭐⭐⭐ Qiyin

**13.** **Operator ustunligi (precedence)** ni tushuntiring. Quyidagi ifodalar nima qaytaradi?
```javascript
console.log(2 + 3 * 4);       // ?
console.log((2 + 3) * 4);     // ?
console.log(true + true);     // ?
console.log(true + false);    // ?
console.log("5" - - 3);      // ?
```

**14.** Optional chaining `?.` operatorini ishlating:
```javascript
let foydalanuvchi = { profil: { shahar: "Toshkent" } };
console.log(foydalanuvchi?.profil?.shahar);   // ?
console.log(foydalanuvchi?.manzil?.shahar);   // ?  (xatosiz)
```

**15.** Quyidagi ifodalar nima qaytaradi? Nima uchun?
```javascript
console.log([] + []);     // ?
console.log([] + {});     // ?
console.log({} + []);     // ?
console.log(+[]);         // ?
console.log(+{});         // ?
```

**16.** `**` (daraja) operatori bilan hisoblang: `2**10`, `(-2)**3`, `4**0.5`. Oxirgisi nima?

**17.** Quyidagi AND va OR zanjirlari nima qaytaradi?
```javascript
let a = null, b = undefined, c = "Topildi!";
console.log(a || b || c);         // ?
console.log(a && b && c);         // ?
console.log(a ?? b ?? c);         // ?
```

**18.** `!` (NOT) operatorini ikki marta ishlating (`!!`) — bu nima uchun qimmatli?
```javascript
console.log(!!"Salom");    // ?
console.log(!!0);          // ?
console.log(!!null);       // ?
console.log(!![]);         // ?
```

**19.** Quyidagi uchlik operatorlar zanjiriga baxo bering — sodda `if-else` ko'rinishiga o'tkazing:
```javascript
let ball = 75;
let baho = ball >= 90 ? "A" : ball >= 75 ? "B" : ball >= 60 ? "C" : "D";
console.log(baho); // ?
```

**20.** O'z `typeof` simulyatsiyangizni yozing — funksiya qabul qilgan qiymatning turini o'zi aniqlaydi (primitive turlar uchun).

---

## 📁 04 — Tur O'zgartirish (Type Conversion)

### ⭐ Oson

**1.** Quyidagi qiymatlarni `String()` bilan matnga aylantiring va `typeof` bilan tekshiring:
```
42,  true,  null,  undefined,  0
```

**2.** Quyidagi matnlarni `Number()` bilan raqamga aylantiring — natijani yozing:
```
"42",  "3.14",  "",  "  ",  "abc",  "12px"
```

**3.** Quyidagi qiymatlarni `Boolean()` bilan mantiqiyga aylantiring:
```
0,  1,  "",  "0",  null,  undefined,  [],  {}
```

**4.** `"10" + 5` va `"10" - 5` ning natijasi nima va nima uchun farq qiladi?

**5.** `parseInt("42.9abc")` va `parseFloat("42.9abc")` qanday natija beradi?

**6.** `+` (unary) operatoridan foydalanib, quyidagilarni raqamga aylantiring:
```javascript
console.log(+"");       // ?
console.log(+"42");     // ?
console.log(+true);     // ?
console.log(+null);     // ?
console.log(+"abc");    // ?
```

**7.** `Number(null)`, `Number(undefined)`, `Number(true)`, `Number(false)` qiymatlari nima? Jadval ko'rinishida yozing.

### ⭐⭐ O'rta

**8.** Foydalanuvchi `prompt` orqali `"  15  "` kiritdi (bosh-oxirida bo'sh joy). Uni raqamga aylantiring — `Number()` bu bilan qanday ishlaydi?

**9.** `parseInt()` ikkinchi parametr — **radix** (asos) oladi. Quyidagilar nima beradi?
```javascript
console.log(parseInt("ff", 16));   // ?  (16lik)
console.log(parseInt("10", 2));    // ?  (ikkilik)
console.log(parseInt("10", 8));    // ?  (sakkizlik)
```

**10.** Quyidagi yashirin konversiyalarni tahlil qiling:
```javascript
console.log(1 + "2" + 3);      // ?
console.log(1 + 2 + "3");      // ?
console.log("3" - 1 - 1);      // ?
console.log(true + true + "1"); // ?
```

**11.** `"5" == 5` nima uchun `true`, lekin `"5" === 5` nima uchun `false`? Qaysi birini ishlatish kerak va nima uchun?

**12.** `NaN === NaN` qanday natija beradi? `isNaN()` va `Number.isNaN()` ning farqi nimada?
```javascript
console.log(NaN === NaN);            // ?
console.log(isNaN("abc"));           // ?
console.log(Number.isNaN("abc"));    // ?
console.log(isNaN(NaN));             // ?
console.log(Number.isNaN(NaN));      // ?
```

### ⭐⭐⭐ Qiyin

**13.** Quyidagi qiymatlarni `String()` bilan aylantiring — kutilmagan natija bormi?
```javascript
console.log(String([]));          // ?
console.log(String([1,2,3]));    // ?
console.log(String({}));          // ?
console.log(String([1,[2,3]]));  // ?
```

**14.** Quyidagi taqqoslashlarni tushuntiring — barchasining natijasini va sababini yozing:
```javascript
console.log(null == undefined);    // ?
console.log(null === undefined);   // ?
console.log(null == 0);           // ?
console.log(null > 0);            // ?
console.log(null >= 0);           // ?
```

**15.** **Falsy** qiymatlar nima? Ularni yod oling va ularni `if` shart ichida test qiling:
```javascript
let qiymatlar = [0, "", null, undefined, NaN, false, -1, "0", [], {}];
qiymatlar.forEach(q => {
  console.log(q, "→", Boolean(q));
});
```

**16.** Foydalanuvchidan son oluvchi va noto'g'ri kiritma (matn, bo'sh, null) uchun qayta so'rovchi funksiya yozing. `Number()` va `isNaN()` dan foydalaning.

**17.** Quyidagi kod nima chiqaradi?
```javascript
console.log([] == false);    // ?
console.log([] == 0);        // ?
console.log("" == false);    // ?
console.log(0 == false);     // ?
console.log("0" == false);   // ?
```
Har birini **abstrakt tenglik algoritmi** bilan tushuntiring.

**18.** `.toString()` metodiga **asos (radix)** bering va sonni turli sanoq sistemasida chiqaring:
```javascript
let son = 255;
console.log(son.toString(2));    // ikkilik  → ?
console.log(son.toString(8));    // sakkizlik → ?
console.log(son.toString(16));   // o'n oltili → ?
```

**19.** `Number()` va `parseInt()` ning farqini 5 ta misol bilan ko'rsating — qaysi holatda natijalar farq qiladi?

**20.** `JSON.stringify()` va `JSON.parse()` da tur o'zgartirish qanday bo'ladi?
```javascript
let asl = { son: 42, matn: "salom", mantiq: true, nol: null, noaniq: undefined };
let json = JSON.stringify(asl);
console.log(json);                // ?  (undefined yo'qoladi!)
let qayta = JSON.parse(json);
console.log(qayta);              // ?
```

---

## 📁 05 — Kiritish va Chiqarish (Input / Output)

### ⭐ Oson

**1.** `console.log()` bilan o'zingiz haqingizda 5 ta ma'lumotni chiqaring: ism, yosh, shahar, sevimli rang, sevimli taom.

**2.** `console.warn()` va `console.error()` farqini ko'rsating — har biri uchun bitta misol yozing.

**3.** Quyidagi massivni `console.table()` bilan jadval ko'rinishida chiqaring:
```javascript
let mahsulotlar = [
  { nom: "Non", narx: 3000 },
  { nom: "Sut", narx: 8000 },
  { nom: "Tuxum", narx: 15000 }
];
```

**4.** `alert()` bilan "Sahifaga xush kelibsiz!" xabarini chiqaring.

**5.** `prompt()` bilan foydalanuvchidan ism so'rang va konsolta `"Salom, [ism]!"` deb chiqaring.

**6.** `confirm()` bilan "Davom etasizmi?" deb so'rang. Javobiga qarab turli xabar chiqaring.

**7.** `console.group()` va `console.groupEnd()` yordamida guruhlangan ma'lumotlar chiqaring:
```
▼ Foydalanuvchi
    Ism: Ali
    Yosh: 25
▼ Manzil
    Shahar: Toshkent
    Tuman: Yunusobod
```

### ⭐⭐ O'rta

**8.** `prompt()` bilan ikki son so'rang, ularning **yig'indisi**, **ko'paytmasi** va **farqini** `alert()` bilan chiqaring.

**9.** Foydalanuvchidan son so'rang. Agar u juft bo'lsa "Juft son", toq bo'lsa "Toq son" deb `alert` chiqaring. Raqam kiritilmasa xato xabar bering.

**10.** `prompt()` qaytaradigan qiymat doim `string`. Foydalanuvchi `"Cancel"` ni bossa `null` qaytaradi. Ikkalasini ham to'g'ri boshqaradigan kod yozing.

**11.** Foydalanuvchidan **3 ta son** so'rang (alohida `prompt` bilan) va ularning o'rtacha qiymatini `console.log` bilan chiqaring.

**12.** `console.log()` ichida hisobni bajarish mumkin. Quyidagi barcha usullar bilan matn + o'zgaruvchini chiqaring:
```javascript
let ism = "Nodira";
let yosh = 24;
// 1. + operatori bilan
// 2. Template literal bilan
// 3. Vergul bilan (console.log ning o'zi bo'sh joy qo'shadi)
```

### ⭐⭐⭐ Qiyin

**13.** `while` sikli va `prompt()` yordamida foydalanuvchi **to'g'ri raqam kiritmagunicha** qayta so'raydigan dastur yozing. Faqat `1` dan `100` gacha qabul qiling.

**14.** `confirm()` bilan tasdiqlash orqali ishlaydigan **"o'chirish" stsenariysini** simulyatsiya qiling:
```
→ "Haqiqatan ham o'chirmoqchimisiz?" → OK → "Rostdan ham?" → OK → "O'chirildi!"
→ Istalgan bosqichda Cancel → "Bekor qilindi."
```

**15.** HTML sahifada `<input>` dan qiymat olib, `<p>` ga chiqaradigan dastur yozing. `innerHTML` va `textContent` ning xavfsizlik farqini ko'rsating — foydalanuvchi `<b>Qalin</b>` kiritsа nima bo'ladi?

**16.** `console.time()` va `console.timeEnd()` yordamida kodni bajarilish vaqtini o'lchang:
```javascript
console.time("sikl");
let yigindi = 0;
for (let i = 0; i < 1_000_000; i++) yigindi += i;
console.timeEnd("sikl");
console.log("Yig'indi:", yigindi);
```

**17.** `prompt()` orqali foydalanuvchidan **yirik shaharlar soni** va har birining **nomini** so'rang, ularni massivga saqlang va `console.table()` bilan chiqaring.

**18.** Foydalanuvchidan **to'g'ri elektron pochta manzili** kiritmagunicha qayta so'raydigan dastur yozing. Elektron pochta `@` va `.` belgilarini o'z ichiga olishi kerak.

**19.** `console` obyektidagi metodlardan foydalanib, mini **debug panel** yarating. Har xil turdagi ma'lumotlarni (xatolar, ogohlantirishlar, jadvallar) guruhlangan tarzda chiqaring.

**20.** `prompt()`, `confirm()` va `alert()` dan foydalanib, oddiy **viktorina** (quiz) o'yini yarating:
- 3 ta savol bering
- Har to'g'ri javob uchun 1 ball bering
- Oxirida natijani `alert` bilan ko'rsating: `"Siz 3 dan N ta to'g'ri javob berdingiz!"`

---

## 📊 Umumiy statistika

| Mavzu | Oson (⭐) | O'rta (⭐⭐) | Qiyin (⭐⭐⭐) | Jami |
|-------|-----------|------------|-------------|------|
| 01 — Variables | 7 | 5 | 8 | 20 |
| 02 — Data Types | 7 | 5 | 8 | 20 |
| 03 — Operators | 7 | 5 | 8 | 20 |
| 04 — Type Conversion | 7 | 5 | 8 | 20 |
| 05 — Input / Output | 7 | 5 | 8 | 20 |
| **Jami** | **35** | **25** | **40** | **100** |

---

> 💡 **Maslahat:** Topshiriqlarni ketma-ket bajaring. Qiyin topshiriqlarga o'tishdan oldin oson va o'rtalarilarni to'liq tugatib oling.
