# Daily Quest

Aplikasi checklist harian ringan untuk Windows dengan tampilan seperti sticky note.

## Fitur

- Checklist dimulai kosong supaya bisa diisi sesuai rutinitas sendiri.
- Checklist aktivitas pagi yang tersimpan otomatis.
- Centang di-reset otomatis saat tanggal berganti; daftar aktivitas tetap ada.
- Tambah dan hapus aktivitas.
- Progress harian dan pesan penyemangat.
- Pilihan bahasa Indonesia dan English yang tersimpan otomatis.
- Riwayat checklist per tanggal, termasuk aktivitas selesai yang sudah dibersihkan
  dari daftar aktif.
- Widget dapat dipindah, diubah ukurannya, diminimalkan, dan dipasang selalu di atas.
- Posisi, ukuran, daftar, dan preferensi pin disimpan lokal di
  `%LOCALAPPDATA%\DailyQuest\state.json`.
- Data dari versi lama Morning Check-in otomatis disalin sekali ke lokasi baru;
  file lama tetap disimpan sebagai cadangan.
- Ikon aplikasi khusus Daily Quest untuk Explorer, taskbar, dan Alt+Tab.
- Tidak membutuhkan akun atau koneksi internet.

## Menjalankan dari source

```powershell
dotnet run --project .\DailyQuest.csproj
```

## Membuat EXE portable

```powershell
dotnet publish .\DailyQuest.csproj -c Release -r win-x64 --self-contained true -o .\artifacts\DailyQuest-win-x64
```

Hasilnya ada di `artifacts\DailyQuest-win-x64\DailyQuest.exe` dan bisa langsung dijalankan
di Windows 10/11 64-bit tanpa instalasi .NET tambahan.

## Menjalankan tes

```powershell
dotnet run --project .\tests\DailyQuest.LogicTests\DailyQuest.LogicTests.csproj -c Release
```

Test harness ini tidak memakai dependency test eksternal dan mengecek operasi checklist,
reset harian, persistence JSON, serta recovery jika file state rusak.
