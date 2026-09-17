# Installer Windows Daily Quest

[English](README.md) · **Bahasa Indonesia**

Daily Quest menggunakan Inno Setup untuk membuat installer Windows per-user dari executable `win-x64` yang self-contained.

## Perilaku installer

- Memasang aplikasi tanpa akses administrator ke `%LOCALAPPDATA%\Programs\Daily Quest`.
- Membuat shortcut Start Menu dan menyediakan pilihan shortcut Desktop yang secara default tidak dicentang.
- Menggunakan ID aplikasi yang tetap agar installer versi berikutnya dapat memperbarui instalasi yang sudah ada.
- Mendeteksi instance Daily Quest yang masih berjalan sebelum mengganti atau menghapus file.
- Menyertakan uninstaller Windows standar.
- Daily Quest mengaktifkan peluncuran saat masuk ke Windows untuk pengguna saat ini secara default setelah aplikasi pertama kali dijalankan; pengguna dapat mematikannya melalui Pengaturan.
- Uninstaller hanya menghapus entri startup jika entri tersebut masih menunjuk ke executable yang terpasang, sehingga pendaftaran portable yang terpisah tidak ikut terhapus.
- Mempertahankan folder `%LOCALAPPDATA%\DailyQuest` dan folder lama `%LOCALAPPDATA%\MorningCheckIn` saat upgrade maupun uninstall, sehingga quest, riwayat, dan preferensi tidak terhapus.
- Mendukung Windows 10/11 x64 serta emulasi x64 pada perangkat Windows 11 Arm64 yang kompatibel.

Installer dan aplikasi saat ini belum ditandatangani secara digital. Karena itu, Windows SmartScreen mungkin menampilkan peringatan penerbit tidak dikenal. Distribusikan installer hanya bersama checksum SHA-256-nya.

## Kebutuhan

- Windows 10 atau Windows 11
- .NET 10 SDK untuk publish dari source
- Inno Setup 6.3 atau yang lebih baru

Pasang compiler untuk pengguna saat ini:

```powershell
winget install --id JRSoftware.InnoSetup --exact --scope user
```

## Build dari source

Ringtone bersifat opsional untuk build aplikasi biasa, tetapi wajib tersedia jika ingin mereproduksi installer resmi dari source. Letakkan file tersebut pada lokasi yang dijelaskan di `Assets\ringtone\README.md`, kemudian jalankan:

```powershell
.\installer\build-installer.ps1
```

Script akan membaca versi dari `DailyQuest.csproj`, menjalankan seluruh logic test dengan validasi ringtone aktif, mem-publish aplikasi self-contained, mengompilasi installer, memverifikasi metadata versinya, lalu membuat `SHA256SUMS-installer.txt` di folder yang sama.

Untuk membungkus executable resmi yang sudah di-publish:

```powershell
$publishedExe = 'C:\lokasi\DailyQuest.exe'
$expectedSha256 = '<SHA-256 dari rilis resmi>'
.\installer\build-installer.ps1 `
  -PublishedExe $publishedExe `
  -ExpectedPublishedExeSha256 $expectedSha256 `
  -OutputRoot .\artifacts\installer\from-release
```

Untuk versi proyek `x.y.z`, output default-nya adalah:

```text
artifacts\installer\vx.y.z\output\DailyQuest-vx.y.z-win-x64-setup.exe
artifacts\installer\vx.y.z\output\SHA256SUMS-installer.txt
```

Gunakan `-OutputRoot` baru untuk setiap build ulang. Script sengaja menolak menimpa output selesai yang sudah ada. Build yang gagal tetap berada di direktori staging dengan nama unik sehingga tidak menghalangi percobaan berikutnya.
