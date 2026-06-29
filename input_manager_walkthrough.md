# 🎮 Walkthrough Lengkap: Konfigurasi Input Manager untuk Gamepad

Panduan ini akan menuntun Anda secara detail dan langkah demi langkah untuk menyinkronkan pengaturan controller Xbox/PlayStation Anda di dalam Unity (Legacy Input Manager).

Karena semua script C# telah diperbarui untuk membaca tombol controller secara otomatis (`JoystickButton`), **Anda HANYA perlu mengatur input yang berupa tuas analog (Axis)** di panduan ini, yaitu **Analog Kanan (Kamera)**, **Analog Kiri (Pergerakan)**, dan **D-Pad (Senter & Dokumen)**.

> [!IMPORTANT]
> **Perubahan penting:** Axis analog kanan sekarang menggunakan nama **`RightStickX`** dan **`RightStickY`** (bukan "Mouse X" / "Mouse Y") agar tidak bentrok dengan input mouse. Script `PlayerCam.cs` sudah diupdate untuk membaca keduanya secara terpisah.

---

## 🎮 Referensi Pemetaan Tombol PS4 DualShock 4 (Windows, Tanpa DS4Windows)

Berikut adalah pemetaan tombol PS4 DualShock 4 yang **benar** di Windows tanpa software tambahan (DirectInput):

| Tombol PS4 | Unity `JoystickButton` | Fungsi di Game |
|---|---|---|
| **Square (□)** | `joystick button 0` | Interact / Ambil Item |
| **X (✕)** | `joystick button 1` | Jump |
| **Circle (○)** | `joystick button 2` | Crouch / Toggle |
| **Triangle (△)** | `joystick button 3` | *(Tidak dipakai)* |
| **L1** | `joystick button 4` | Aim / Zoom |
| **R1** | `joystick button 5` | Shoot |
| **L2** | `joystick button 6` | *(Gunakan axis 5th untuk analog)* |
| **R2** | `joystick button 7` | *(Gunakan axis 6th untuk analog)* |
| **Share** | `joystick button 8` | *(Tidak dipakai)* |
| **Options** | `joystick button 9` | Pause |
| **L3 (klik kiri)** | `joystick button 10` | Sprint |
| **R3 (klik kanan)** | `joystick button 11` | *(Tidak dipakai)* |
| **PS Button** | `joystick button 12` | *(Tidak dipakai)* |
| **Touchpad Press** | `joystick button 13` | *(Tidak dipakai)* |

> [!NOTE]
> Mapping di atas **khusus PS4 DualShock 4 tanpa DS4Windows** di Windows. Jika menggunakan **DS4Windows** atau **XInput emulation**, pemetaan akan berbeda (cenderung mengikuti pola Xbox).

---

## 🕹️ Referensi Axis PS4 DualShock 4 (Windows DirectInput)

| Axis | Nama Axis Unity | Keterangan |
|---|---|---|
| Left Stick X (Kiri-Kanan) | `X axis` | Pergerakan Horizontal |
| Left Stick Y (Atas-Bawah) | `3rd axis` | Pergerakan Vertikal *(Perlu Invert)* |
| Right Stick X (Kiri-Kanan) | `4th axis` | Kamera Horizontal |
| Right Stick Y (Atas-Bawah) | `7th axis` | Kamera Vertikal *(Perlu Invert)* |
| L2 (Trigger Kiri) | `5th axis` | Range: -1.0 (lepas) s/d 1.0 (tekan penuh) |
| R2 (Trigger Kanan) | `6th axis` | Range: -1.0 (lepas) s/d 1.0 (tekan penuh) |
| D-Pad X (Kiri-Kanan) | `8th axis` | D-Pad Horizontal |
| D-Pad Y (Atas-Bawah) | `9th axis` | D-Pad Vertikal |

---

## 🛠️ Persiapan Awal
1. Buka project Anda di Unity Editor.
2. Pada menu bar di bagian atas, klik **Edit**.
3. Pilih **Project Settings...**
4. Di jendela Project Settings yang muncul, klik tab **Input Manager** di sebelah kiri.
5. Anda akan melihat bagian bernama **Axes** dengan tanda panah (▶). Klik tanda panah tersebut untuk membuka daftarnya.
6. Lihat kolom **Size** (ukuran array). Misalnya tertulis `18` (atau angka berapapun).
7. Kita butuh **4 slot baru** (RightStickX, RightStickY, Vertical override PS4, dan DPadY). Tambahkan angka `Size` tersebut dengan 4. *(Contoh: Jika awalnya 18, ubah menjadi 22)*.
8. Tekan `Enter`. Setelah ditekan, Anda akan melihat ada 4 input baru (duplikat dari input paling bawah) yang muncul di akhir daftar Axes.

> [!WARNING]
> **Jangan buat entry baru bernama "Mouse X" atau "Mouse Y" bertipe Joystick Axis!** Ini akan menyebabkan nilai joystick tercampur dengan mouse dan membuat kamera bergerak sendiri. Gunakan nama `RightStickX` dan `RightStickY` seperti panduan di bawah.

---

## 🔍 Langkah 0 (Opsional): Cari Axis yang Benar untuk Controller Anda

Jika Anda tidak yakin axis mana yang digunakan oleh analog kanan controller Anda:

1. Pasang script **`GamepadAxisDebug.cs`** pada GameObject manapun di scene.
2. Sambungkan controller, lalu klik **Play**.
3. Gerakkan **HANYA analog kanan** ke kiri-kanan, lalu lihat di Console axis mana yang bergerak → itu **RightStickX**.
4. Gerakkan **HANYA analog kanan** ke atas-bawah, lalu lihat di Console axis mana yang bergerak → itu **RightStickY**.
5. Catat nomor axis-nya, lalu gunakan di Langkah 1 dan 2 di bawah.
6. **Hapus script `GamepadAxisDebug.cs`** dari GameObject setelah selesai.

**Referensi umum axis controller:**

| Controller | Left Stick Y | Right Stick X | Right Stick Y | D-Pad Y |
|---|---|---|---|---|
| Xbox (XInput) | `Y axis` | `4th axis` | `5th axis` | `7th axis` |
| PS4 **dengan** DS4Windows | `Y axis` | `4th axis` | `5th axis` | `7th axis` |
| PS4 **tanpa** DS4Windows (DirectInput) | `3rd axis` | `4th axis` | `7th axis` | `9th axis` |

---

## 🕹️ Langkah 1: Setting Analog Kanan (Kamera Kiri-Kanan)
Kita akan membuat Unity mengenali putaran Analog Kanan ke kiri dan ke kanan.

1. Scroll ke bagian paling bawah dari daftar Axes.
2. Buka slot baru yang **ke-3 dari bawah** (atau klik panahnya).
3. Ubah kolom-kolom di dalamnya menjadi sama persis seperti ini:
   - **Name**: `RightStickX`
   - **Descriptive Name**: *(kosongkan)*
   - **Descriptive Negative Name**: *(kosongkan)*
   - **Negative Button**: *(kosongkan)*
   - **Positive Button**: *(kosongkan)*
   - **Alt Negative Button**: *(kosongkan)*
   - **Alt Positive Button**: *(kosongkan)*
   - **Gravity**: `0`
   - **Dead**: `0.19`
   - **Sensitivity**: `1`
   - **Snap**: Jangan dicentang
   - **Invert**: Jangan dicentang
   - **Type**: `Joystick Axis`
   - **Axis**: `4th axis (Joysticks)` *(Sama untuk semua controller)*
   - **Joy Num**: `Get Motion from all Joysticks`

---

## 🕹️ Langkah 2: Setting Analog Kanan (Kamera Atas-Bawah)
Kita akan membuat Unity mengenali putaran Analog Kanan ke atas dan ke bawah.

1. Buka slot baru yang **ke-2 dari bawah**.
2. Ubah pengaturannya menjadi seperti ini:
   - **Name**: `RightStickY`
   - **Descriptive Name**: *(kosongkan)*
   - **Descriptive Negative Name**: *(kosongkan)*
   - **Negative Button**: *(kosongkan)*
   - **Positive Button**: *(kosongkan)*
   - **Alt Negative Button**: *(kosongkan)*
   - **Alt Positive Button**: *(kosongkan)*
   - **Gravity**: `0`
   - **Dead**: `0.19`
   - **Sensitivity**: `1`
   - **Snap**: Jangan dicentang
   - **Invert**: **Centang (✔)** *(Sangat penting agar gerakan kamera atas-bawah tidak terbalik)*
   - **Type**: `Joystick Axis`
   - **Axis**:
     - **PS4 tanpa DS4Windows** → `7th axis (Joysticks)` ← **KOREKSI dari info Reddit**
     - **Xbox / DS4Windows** → `5th axis (Joysticks)`
   - **Joy Num**: `Get Motion from all Joysticks`

> [!TIP]
> Jika kamera masih tidak bergerak vertikal atau bergerak sendiri, gunakan script `GamepadAxisDebug.cs` untuk mendeteksi axis yang benar, lalu sesuaikan nilai **Axis** di atas.

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
   - **Axis**:
     - **PS4 tanpa DS4Windows** → `9th axis (Joysticks)` ← **KOREKSI dari info Reddit**
     - **Xbox / DS4Windows** → `7th axis (Joysticks)`
   - **Joy Num**: `Get Motion from all Joysticks`

---

---

## 🕹️ Langkah 4: Setting Analog Kiri (Pergerakan Karakter)
Jika Anda tidak sengaja menghapus pengaturan bawaan Unity untuk analog kiri, atau jika pergerakan maju-mundur **tidak berfungsi di PS4**, Anda harus membuatnya kembali. Tambahkan slot baru (tambah `Size` +2), lalu isi dengan pengaturan berikut:

**Entry 1 (Analog Kiri Kiri-Kanan):**
- **Name**: `Horizontal`
- **Descriptive Name**: *(kosongkan)*
- **Negative Button**: *(kosongkan)*
- **Positive Button**: *(kosongkan)*
- **Gravity**: `0`
- **Dead**: `0.19`
- **Sensitivity**: `1`
- **Snap**: Jangan dicentang
- **Invert**: Jangan dicentang
- **Type**: `Joystick Axis`
- **Axis**: `X axis` *(Sama untuk semua controller)*
- **Joy Num**: `Get Motion from all Joysticks`

**Entry 2 (Analog Kiri Maju-Mundur):**
- **Name**: `Vertical`
- **Descriptive Name**: *(kosongkan)*
- **Negative Button**: *(kosongkan)*
- **Positive Button**: *(kosongkan)*
- **Gravity**: `0`
- **Dead**: `0.19`
- **Sensitivity**: `1`
- **Snap**: Jangan dicentang
- **Invert**: **Centang (✔)** *(Agar maju/mundur tidak terbalik)*
- **Type**: `Joystick Axis`
- **Axis**:
  - **PS4 tanpa DS4Windows** → `3rd axis (Joysticks)` ← **KOREKSI dari info Reddit**
  - **Xbox / DS4Windows** → `Y axis`
- **Joy Num**: `Get Motion from all Joysticks`

> [!NOTE]
> Pastikan juga entry `Horizontal` dan `Vertical` bertipe **Key or Mouse Button** (untuk keyboard W/A/S/D) tetap ada dan tidak terhapus.

---

## 🗑️ Langkah 5: Hapus Entry "Mouse X" / "Mouse Y" Joystick yang Lama

Jika sebelumnya Anda sudah membuat entry bernama **"Mouse X"** atau **"Mouse Y"** dengan Type **Joystick Axis**, entry tersebut harus **dihapus** karena akan bentrok dengan input mouse bawaan Unity.

1. Cari di daftar Axes entry bernama "Mouse X" yang bertipe **Joystick Axis** (bukan yang bertipe Mouse Movement).
2. Jika ada, hapus dengan mengurangi angka **Size** atau set Name-nya ke nama lain yang tidak digunakan.
3. Lakukan hal yang sama untuk "Mouse Y" bertipe Joystick Axis.

> [!CAUTION]
> **JANGAN hapus** entry "Mouse X" dan "Mouse Y" bawaan Unity yang bertipe **Mouse Movement**! Itu dibutuhkan agar mouse tetap berfungsi. Yang dihapus hanya duplikat bertipe **Joystick Axis**.

---

## ✅ Pengecekan Akhir
*   **Analog Kiri (Pergerakan Karakter)**: `Horizontal` (X axis) dan `Vertical` (**3rd axis** untuk PS4 tanpa DS4Windows / **Y axis** untuk Xbox).
*   **Analog Kanan (Kamera)**: `RightStickX` (4th axis) dan `RightStickY` (**7th axis** untuk PS4 tanpa DS4Windows / **5th axis** untuk Xbox).
*   **D-Pad Y**: `DPadY` (**9th axis** untuk PS4 tanpa DS4Windows / **7th axis** untuk Xbox).
*   **Mouse (Kamera)**: Tetap menggunakan `Mouse X` dan `Mouse Y` bawaan Unity (Type: Mouse Movement).
*   **Tombol Aksi (Square, X, Circle, R1, L1, L3)**: Sudah terhubung otomatis melalui script C# tanpa perlu konfigurasi Input Manager tambahan.
*   Tutup jendela `Project Settings` dan klik Play. Jika kamera berputar terlalu cepat atau lambat, sesuaikan nilai **`gamepadSensMultiplier`** di komponen `PlayerCam` pada Inspector.
