# Ma'lumot Turlari (Data Types)

JavaScript-da o'zgaruvchilarda turli xil ma'lumotlarni saqlashimiz mumkin. Ma'lumot turlari asosan ikki katta guruhga bo'linadi: **Primitiv** (oddiy) va **Obyekt** (murakkab) turlar.

## 1. Primitiv Ma'lumot Turlari

*   **String (Matn):** Matnli ma'lumotlarni ifodalaydi. Qo'shtirnoq (`""`), bittalik tirnoq (`''`) yoki backtick (`` ` ``) ichiga yoziladi.
    ```javascript
    let ism = "Aziz";
    let shahar = 'Toshkent';
    ```
*   **Number (Raqam):** Butun yoki kasr sonlarni ifodalaydi.
    ```javascript
    let yosh = 25;
    let narx = 99.99;
    ```
*   **Boolean (Mantiqiy):** Faqat ikkita qiymat qabul qiladi: `true` (rost/to'g'ri) yoki `false` (yolg'on/noto'g'ri).
    ```javascript
    let uylanganmi = false;
    let oquvchimi = true;
    ```
*   **Undefined:** O'zgaruvchi e'lon qilingan, lekin unga hali hech qanday qiymat berilmagan holat.
    ```javascript
    let ism;
    console.log(ism); // undefined
    ```
*   **Null:** Atayin bo'sh yoki hech qanday qiymat yo'qligini bildirish uchun ishlatiladi.
    ```javascript
    let natija = null;
    ```

## 2. Murakkab Ma'lumot Turlari

*   **Object (Obyekt):** Bir nechta xususiyatlarni (kalit-qiymat ko'rinishida) o'z ichiga oluvchi to'plam.
    ```javascript
    let odam = {
        ism: "Hasan",
        yosh: 30,
        kasb: "Dasturchi"
    };
    ```
*   **Array (Massiv):** Bir nechta qiymatlarni bitta ro'yxatda saqlash uchun ishlatiladi (Array ham aslida obyektning bir turi).
    ```javascript
    let mevalar = ["olma", "banan", "uzum"];
    ```

> **Eslatma:** `typeof` operatori yordamida o'zgaruvchining qaysi ma'lumot turiga tegishli ekanligini bilib olishingiz mumkin.
> ```javascript
> typeof "Salom" // natija: "string"
> typeof 42      // natija: "number"
> typeof true    // natija: "boolean"
> ```
