# Break operatori (Siklni to'xtatish)

Sikl odatda o'z sharti `false` bo'lguncha to'liq aylanadi. Lekin ba'zida kutilgan natija muddatidan oldin topilsa yoki biror favqulodda holat yuz bersa, siklning to'liq tugashini kutib o'tirish shart emas.

Bunday paytda siklni **shu zahotiyoq, butunlay to'xtatish** uchun **`break`** operatori ishlatiladi.

> **Hayotiy misol:** 1000 ta kitob ichidan kerakli kitobni qidiryapsiz. Agar uni 5-kitobda topib olsangiz, qolgan 995 ta kitobni qarab chiqish shart emas. Qidirishni shu zahoti to'xtatasiz (`break`).

---

## break operatorining tuzilishi

`break` operatori odatda sikl ichidagi `if` sharti bilan birgalikda ishlatiladi.

**Sintaksis:**

```js
for (boshlang'ich; shart; qadam) {
  if (to'xtatish_sharti) {
    break; // Sikl darhol to'xtaydi va undan chiqiladi
  }
  // Boshqa kodlar
}
```

---

## Oddiy misol: 5 ga yetganda to'xtash

1 dan 10 gacha sanaymiz, lekin son 5 ga teng bo'lishi bilan siklni to'xtatamiz:

```js
for (let i = 1; i <= 10; i++) {
  if (i === 5) {
    console.log("5 topildi, sikl to'xtatildi!");
    break;
  }
  console.log("Son: " + i);
}

console.log("Sikldan keyingi kod.");
```

**Natija:**

```
Son: 1
Son: 2
Son: 3
Son: 4
5 topildi, sikl to'xtatildi!
Sikldan keyingi kod.
```

`i` qiymati `5` bo'lgan zahoti `break` ishladi va `6, 7, 8, 9, 10` sonlari tekshirilmadi ham, chiqarilmadi ham.

---

## while siklida break ishlatish

`break` nafaqat `for`, balki `while` va `do...while` sikllarida ham bir xil ishlaydi. 

Ayniqsa, cheksiz `while (true)` siklini ma'lum bir shart bajarilganda to'xtatish uchun juda qulay:

```js
let hisob = 1;

while (true) {
  console.log("Hisob: " + hisob);
  
  if (hisob >= 3) {
    console.log("Cheklovga yetdik, to'xtaymiz.");
    break;
  }
  
  hisob++;
}
```

**Natija:**
```
Hisob: 1
Hisob: 2
Hisob: 3
Cheklovga yetdik, to'xtaymiz.
```

---

## Ichma-ich sikllarda break ning harakati

Agar sikllar bir-birining ichida bo'lsa, `break` **faqat o'zi joylashgan ichki siklni** to'xtatadi. Tashqi sikl o'z ishini davom ettiraveradi.

**Misol:**

```js
for (let i = 1; i <= 3; i++) {
  console.log("Tashqi sikl: " + i);

  for (let j = 1; j <= 5; j++) {
    if (j === 3) {
      break; // Faqat j siklini to'xtatadi!
    }
    console.log("  Ichki sikl: " + j);
  }
}
```

**Natija:**
```
Tashqi sikl: 1
  Ichki sikl: 1
  Ichki sikl: 2
Tashqi sikl: 2
  Ichki sikl: 1
  Ichki sikl: 2
Tashqi sikl: 3
  Ichki sikl: 1
  Ichki sikl: 2
```

---

## Amaliy misol 1: Qidiruv algoritmi

Ro'yxatdan kerakli sonni qidirish. Topilishi bilanoq sikl to'xtatiladi:

```js
let maqsad = 42;
let topildi = false;

for (let i = 1; i <= 100; i++) {
  if (i === maqsad) {
    topildi = true;
    console.log("Maqsadli son topildi: " + i);
    break; // Qolgan 43 dan 100 gacha sonlarni bekorga aylanib o'tirmaymiz!
  }
}

if (!topildi) {
  console.log("Son topilmadi.");
}
```

---

## Amaliy misol 2: Tub sonni tezkor aniqlash

Son tub yoki emasligini tekshirishda, agar bittagina bo'luvchi topilsa ham, son murakkab ekanligi ma'lum bo'ladi va darhol `break` qilish vaqtni tejaydi:

```js
let son = 49;
let tub = true;

for (let i = 2; i < son; i++) {
  if (son % i === 0) {
    tub = false;
    console.log(son + " soni " + i + " ga bo'linadi, shuning uchun tub emas.");
    break; // Boshqa bo'luvchilarni tekshirishga hojat qolmadi!
  }
}

if (tub && son > 1) {
  console.log(son + " — tub son.");
}
```

---

## Xulosa

| Holat | `break` siz | `break` bilan |
|-------|-------------|---------------|
| Sikl davomiyligi | Barcha shartlar to'liq tugaguncha | Kerakli natijaga yetishi bilanoq to'xtaydi |
| Dastur tezligi | Ortiqcha qadamlar hisobiga sekinroq | Tez va tejamkor |
| Qo'llanishi | Barcha elementlar bilan ishlashda | Qidiruv, to'xtatish shartlarida |

---

## Mashqlar

**Oson darajali**

1. 1 dan 20 gacha sonlarni chiqaring, lekin 7 ga yetganda `break` yordamida siklni to'xtating.

2. 100 dan 1 gacha kamaytirib boring, lekin son 95 ga yetganda siklni to'xtating.

3. Cheksiz `while (true)` siklini tuzing va u 5-qadamda `break` orqali to'xtasin.

4. 1 dan 50 gacha sonlar ichidan 13 ga bo'linadigan birinchi sonni toping va ekranga chiqarib, siklni to'xtating.

5. Foydalanuvchining login urinishlari 3 tadan oshganda tizimni bloklab, `break` orqali sikldan chiqing.

6. 1 dan 30 gacha bo'lgan juft sonlarni chiqaring, son 16 bo'lganda to'xtating.

7. 1 dan 10 gacha sonlarni qo'shib boring, yig'indi 20 dan oshishi bilanoq `break` qiling va yig'indini chiqaring.

**O'rta darajali**

8. 1 dan 1000 gacha sonlar orasida 3 ga ham, 7 ga ham, 11 ga ham bir vaqtda bo'linadigan birinchi musbat sonni toping va siklni to'xtating.

9. Berilgan sonning 1 dan katta bo'lgan eng kichik bo'luvchisini `break` yordamida toping.

10. Tasodifiy sonlar (1–10) hosil qiling, qachonki `7` soni tushsa, siklni to'xtating va nechanchi urinishda tushganini chiqaring.

11. O'yin simulyatsiyasi: O'yinchi har qadamda ball to'playdi. Jami ball 100 ga yetishi bilan o'yin g'alaba bilan yakunlansin (`break`).

12. Berilgan matn yoki belgilar ketma-ketligida taqiqlangan belgi (masalan: `"@"`) uchrasa, xatolik berib tekshiruvni to'xtating.

13. 100 dan 1 gacha sonlar orasidan 17 ga bo'linadigan eng katta sonni toping (teskari sanab, birinchi uchraganida `break` qiling).

14. Ichma-ich sikl: 3x3 jadval chop etilayotganda, ichki sikl 2-ustunda `break` qilsin. Natijani tahlil qiling.

**Murakkab darajali**

15. **Eng yaqin kvadrat:** Berilgan sondan (masalan: `70`) katta bo'lgan eng birinchi to'la kvadrat sonni toping (`break` yordamida, natija: `81`).

16. Berilgan `n` soni tub ekanligini aniqlang. Tekshirish chegarasini $\sqrt{n}$ gacha qilib, birinchi bo'luvchida `break` qiling (algoritmik optimallash).

17. ATM (Bankomat) PIN-kod tekshiruvi: 3 ta urinish beriladi. Agar to'g'ri kiritsa `break` qilib "Xush kelibsiz" desin, 3 ta xato urinishdan so'ng "Karta bloklandi" xabari chiqsin.

18. Berilgan ikki massiv yoki sonlar ro'yxatida umumiy (bir xil) element mavjudligini tekshiring. Birinchi umumiy element topilishi bilanoq sikllarni to'xtating.

19. Dastlabki 5 ta tub sonni topib, ekranga chiqargandan so'ng to'xtaydigan sikl yozing.

20. Valyuta konvertori: Foydalanuvchi turli buyruqlar beradi (`"usd"`, `"eur"`, `"rub"`). Agar `"exit"` kiritilsa, dastur `break` orqali to'xtasin va jami bajarilgan operatsiyalar statistikasini ko'rsatsin.
