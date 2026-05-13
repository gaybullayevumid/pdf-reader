# Kiritish va Chiqarish (Input / Output)

Dasturlar foydalanuvchi bilan muloqot qilishi uchun brauzerda maxsus kiritish (foydalanuvchidan ma'lumot olish) va chiqarish (foydalanuvchiga ma'lumot ko'rsatish) usullari mavjud.

## 1. Ma'lumot Chiqarish (Output)

Foydalanuvchiga ma'lumotni yoki dastur natijasini ko'rsatish uchun ishlatiladi.

*   **`console.log()`**
    Eng ko'p ishlatiladigan usul. Ma'lumotlarni brauzerning konsoliga (Console) chiqaradi. Bu asosan dasturchilar uchun xatolarni topish va tekshirish uchun qulay.
    ```javascript
    console.log("Bu matn konsolda ko'rinadi!");
    let x = 10;
    console.log("X ning qiymati:", x);
    ```

*   **`alert()`**
    Brauzerda tepadan sakrab chiquvchi oyna (pop-up) orqali xabar ko'rsatadi. Foydalanuvchi "OK" ni bosmaguncha yo'qolmaydi.
    ```javascript
    alert("Dasturga xush kelibsiz!");
    ```

*   **`document.write()`** (Hozirgi kunda kam ishlatiladi)
    Ma'lumotni to'g'ridan-to'g'ri HTML sahifaning ichiga yozib yuboradi.

## 2. Ma'lumot Kiritish (Input)

Foydalanuvchidan o'zgaruvchilarga saqlash uchun qandaydir ma'lumot so'rash.

*   **`prompt()`**
    Foydalanuvchidan matn kiritishni so'rovchi oyna chiqaradi. Siz kiritilgan ma'lumotni o'zgaruvchiga saqlab olishingiz mumkin.
    > *Muhim: `prompt` har doim kiritilgan ma'lumotni **matn (string)** ko'rinishida qaytaradi.*
    ```javascript
    let ism = prompt("Ismingiz nima?");
    console.log("Foydalanuvchining ismi:", ism);
    ```
    Agar raqam kerak bo'lsa, uni turlarni o'zgartirish orqali raqamga aylantirib olish kerak:
    ```javascript
    let yosh = Number(prompt("Yoshingizni kiriting:"));
    console.log(yosh + 5);
    ```

*   **`confirm()`**
    Foydalanuvchiga biron bir harakatni tasdiqlash uchun "OK" yoki "Cancel" tugmalari bor oyna chiqaradi.
    - Agar "OK" bossa `true` qaytaradi.
    - Agar "Cancel" bossa `false` qaytaradi.
    ```javascript
    let rozimi = confirm("Sayt qoidalariga rozimisiz?");
    console.log("Foydalanuvchi rozimi?", rozimi);
    ```
