# 🎮 Walkthrough Lengkap: Konfigurasi Input Manager untuk Gamepad

Panduan ini akan menuntun Anda secara detail dan langkah demi langkah untuk menyinkronkan pengaturan controller Xbox/PlayStation Anda di dalam Unity (Legacy Input Manager).

Karena semua script C# telah saya perbarui untuk membaca tombol controller secara otomatis (`JoystickButton`), **Anda HANYA perlu mengatur input yang berupa tuas analog (Axis)** di panduan ini, yaitu **Analog Kanan (Kamera)** dan **D-Pad (Senter & Dokumen)**.

---

## 🛠️ Persiapan Awal
1. Buka project Anda di Unity Editor.
2. Pada menu bar di bagian atas, klik **Edit**.
3. Pilih **Project Settings...**
4. Di jendela Project Settings yang muncul, klik tab **Input Manager** di sebelah kiri.
5. Anda akan melihat bagian bernama **Axes** dengan tanda panah (▶). Klik tanda panah tersebut untuk membuka daftarnya.
6. Lihat kolom **Size** (ukuran array). Misalnya tertulis `18` (atau angka berapapun).
7. Kita butuh **3 slot baru** (Mouse X, Mouse Y, dan DPadY). Jadi, tambahkan angka `Size` tersebut dengan 3. *(Contoh: Jika awalnya 18, ubah menjadi 21)*.
8. Tekan `Enter`. Setelah ditekan, Anda akan melihat ada 3 input baru (duplikat dari input paling bawah) yang muncul di akhir daftar Axes.

---

## 🕹️ Langkah 1: Setting Analog Kanan (Kamera Kiri-Kanan)
Kita akan membuat Unity mengenali putaran Analog Kanan ke kiri dan ke kanan sebagai pergerakan Mouse X.

1. Scroll ke bagian paling bawah dari daftar Axes.
2. Buka slot baru yang **ke-3 dari bawah** (atau klik panahnya).
3. Ubah kolom-kolom di dalamnya menjadi sama persis seperti ini:
   - **Name**: `Mouse X` *(Perhatikan spasi dan huruf besarnya)*
   - **Descriptive Name**: *(kosongkan)*
   - **Descriptive Negative Name**: *(kosongkan)*
   - **Negative Button**: *(kosongkan)*
   - **Positive Button**: *(kosongkan)*
   - **Alt Negative Button**: *(kosongkan)*
   - **Alt Positive Button**: *(kosongkan)*
   - **Gravity**: `0`
   - **Dead**: `0.19`
   - **Sensitivity**: `1` *(Jika dirasa putaran kamera terlalu pelan di dalam game, naikkan angka ini menjadi 2 atau 3)*
   - **Snap**: Jangan dicentang
   - **Invert**: Jangan dicentang
   - **Type**: `Joystick Axis`
   - **Axis**: `4th axis (Joysticks)`
   - **Joy Num**: `Get Motion from all Joysticks`

---

## 🕹️ Langkah 2: Setting Analog Kanan (Kamera Atas-Bawah)
Kita akan membuat Unity mengenali putaran Analog Kanan ke atas dan ke bawah sebagai pergerakan Mouse Y.

1. Buka slot baru yang **ke-2 dari bawah**.
2. Ubah pengaturannya menjadi seperti ini:
   - **Name**: `Mouse Y`
   - **Descriptive Name**: *(kosongkan)*
   - **Descriptive Negative Name**: *(kosongkan)*
   - **Negative Button**: *(kosongkan)*
   - **Positive Button**: *(kosongkan)*
   - **Alt Negative Button**: *(kosongkan)*
   - **Alt Positive Button**: *(kosongkan)*
   - **Gravity**: `0`
   - **Dead**: `0.19`
   - **Sensitivity**: `1` *(Bisa dinaikkan jika kurang responsif)*
   - **Snap**: Jangan dicentang
   - **Invert**: **Centang (✔)** *(Sangat penting agar gerakan kamera atas-bawah tidak terbalik)*
   - **Type**: `Joystick Axis`
   - **Axis**: `5th axis (Joysticks)`
   - **Joy Num**: `Get Motion from all Joysticks`

---

## ➕ Langkah 3: Setting D-Pad (Senter & Dokumen)
Karena D-Pad terbaca sebagai sebuah sumbu (axis) di PC, kita harus membuatnya di Input Manager agar script kita tahu kapan tombol panah Atas dan Bawah ditekan.

1. Buka slot baru yang **paling bawah** (slot terakhir).
2. Ubah pengaturannya menjadi seperti ini:
   - **Name**: `DPadY` *(Harus persis seperti ini tanpa spasi)*
   - **Descriptive Name**: *(kosongkan)*
   - **Descriptive Negative Name**: *(kosongkan)*
   - **Negative Button**: *(kosongkan)*
   - **Positive Button**: *(kosongkan)*
   - **Alt Negative Button**: *(kosongkan)*
   - **Alt Positive Button**: *(kosongkan)*
   - **Gravity**: `1000`
   - **Dead**: `0.001`
   - **Sensitivity**: `1000`
   - **Snap**: **Centang (✔)**
   - **Invert**: Jangan dicentang
   - **Type**: `Joystick Axis`
   - **Axis**: `7th axis (Joysticks)` *(Di Windows/Xbox, D-Pad vertikal adalah axis ke-7)*
   - **Joy Num**: `Get Motion from all Joysticks`

---

## ✅ Pengecekan Akhir
*   **Analog Kiri (WASD)**: Otomatis menggunakan setting bawaan Unity bernama `Horizontal` dan `Vertical`.
*   **Tombol Aksi (A, B, X, Y, R1, dll)**: Sudah tertulis dan terhubung secara otomatis melalui modifikasi C# script yang saya lakukan sebelumnya.
*   Anda bisa menutup jendela `Project Settings`. Unity otomatis menyimpan perubahan ini.
*   Silakan coba sambungkan Gamepad Anda, klik Play, dan rasakan pergerakan kameranya! Jika dirasa kamera berputar terlalu cepat atau lambat, Anda bisa kembali ke **Mouse X** dan **Mouse Y** (tipe Joystick Axis) lalu sesuaikan angka **Sensitivity**-nya.
