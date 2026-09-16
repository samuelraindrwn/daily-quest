<p align="center">
  <img src="Assets/DailyQuest-logo.png" width="112" alt="Logo Daily Quest">
</p>

<h1 align="center">Daily Quest</h1>

<p align="center">
  Widget checklist harian ringan dan local-first untuk Windows, dibalut tampilan liquid glass yang minimalis.
</p>

<p align="center">
  <a href="README.md">English</a> · <strong>Bahasa Indonesia</strong>
</p>

<p align="center">
  <a href="https://github.com/samuelraindrwn/daily-quest/releases/latest"><img src="https://img.shields.io/github/v/release/samuelraindrwn/daily-quest?label=rilis" alt="Rilis terbaru"></a>
  <img src="https://img.shields.io/badge/platform-Windows%2010%2F11-2563EB" alt="Windows 10/11">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4" alt=".NET 10">
</p>

Daily Quest membuat prioritas hari ini selalu terlihat tanpa mengubahnya menjadi sistem manajemen proyek yang rumit. Aplikasi ini menggabungkan check-in cepat, penyimpanan lokal otomatis, progres harian, dan riwayat yang dapat dibuka saat dibutuhkan dalam sebuah widget desktop ringkas.

<p align="center">
  <img src="docs/images/daily-quest.png" width="360" alt="Jendela utama Daily Quest dengan checklist kosong">
</p>

## Fitur utama

- Checklist dimulai kosong—rutinitas tetap sepenuhnya milikmu.
- Tambah, centang, dan hapus aktivitas dalam beberapa klik.
- Tambahkan quest langsung ke Hari ini, atau jadwalkan untuk besok hingga maksimal H+8.
- Lihat sapaan, jumlah selesai, persentase, dan progress bar hari ini secara langsung.
- Arsipkan progres harian otomatis dan mulai hari berikutnya dengan daftar yang sama tanpa centang.
- Buka riwayat hari sebelumnya; klik kartu tanggal untuk melihat detail aktivitasnya.
- Pertahankan aktivitas selesai di riwayat setelah dibersihkan dari daftar aktif hari ini.
- Atur urutan quest hari ini dengan menarik handle khusus; urutan tersimpan juga menentukan quest berikutnya di mode ringkas.
- Ringkas widget di pojok kanan atas agar hanya menampilkan satu quest yang belum selesai.
- Pilih Bahasa Indonesia atau English melalui Pengaturan.
- Sematkan widget di atas jendela lain, minimalkan ke taskbar, pindahkan, atau ubah ukurannya.
- Buka di pojok kanan atas area kerja utama dengan jarak yang nyaman dari tepi, sekaligus memulihkan ukuran, bahasa, dan preferensi pin.
- Periksa penggunaan penyimpanan lokal, hapus riwayat, buka Q&A, atau laporkan bug melalui Pengaturan.
- Lihat identitas dan versi aplikasi pada footer tampilan utama.
- Jalankan hanya satu instance sepenuhnya offline tanpa akun, telemetri, atau koneksi internet.

## Unduh dan pasang

1. Buka halaman [rilis terbaru](https://github.com/samuelraindrwn/daily-quest/releases/latest).
2. Unduh aset `win-x64.zip` (disarankan) lalu ekstrak, atau unduh aset `.exe` mandiri.
3. Jalankan `DailyQuest.exe`.

Rilis ini portable dan self-contained untuk Windows 10/11 64-bit, jadi tidak ada installer dan runtime .NET tidak perlu dipasang terpisah. Untuk memperbarui aplikasi, tutup Daily Quest lalu ganti executable lama; data di Local AppData tetap tersimpan.

> [!NOTE]
> File executable saat ini belum ditandatangani secara digital sehingga Windows mungkin menampilkan peringatan SmartScreen. Lanjutkan hanya jika file berasal dari halaman Releases resmi repositori ini. File dapat diverifikasi menggunakan `SHA256SUMS.txt` yang disertakan.

## Cara memakai Daily Quest

| Kontrol | Fungsi |
| --- | --- |
| Kolom aktivitas | Ketik aktivitas baru. Tekan `Enter` atau klik `+` untuk menambahkannya. |
| Pilihan tanggal di composer | Pertahankan **Hari ini** untuk langsung menambahkan quest, atau pilih tanggal besok hingga maksimal H+8. |
| Checkbox | Tandai aktivitas sebagai selesai atau belum selesai. |
| Handle di samping aktivitas | Tarik dan lepas aktivitas untuk mengubah urutannya. |
| `×` di samping aktivitas | Hapus aktivitas tersebut dari checklist aktif. |
| **Hapus selesai** | Hapus aktivitas selesai dari daftar aktif, tetapi pertahankan riwayat selesainya. |
| **Reset** | Hilangkan semua centang untuk hari ini. |
| **Hari ini** | Kembali ke checklist aktif. |
| **Mendatang** | Tinjau atau batalkan quest yang dijadwalkan untuk tanggal mendatang. |
| **Riwayat** | Lihat ringkasan progres per hari. Klik kartunya untuk membuka detail. |
| Tombol Pengaturan | Buka pilihan bahasa, penyimpanan, riwayat, Q&A, dan laporan bug. |
| Tombol mode ringkas | Ringkas widget di pojok kanan atas dan tampilkan hanya quest berikutnya yang belum selesai. Selesaikan quest itu untuk lanjut ke quest berikutnya. |
| Tombol perluas | Kembali dari mode ringkas ke widget penuh. |
| Tombol pin | Aktifkan atau nonaktifkan mode selalu di atas. |
| Tombol `−` | Minimalkan jendela ke taskbar Windows. Fungsi ini terpisah dari mode ringkas. |
| Header dan tepi jendela | Tarik header untuk memindahkan widget, atau tarik tepi untuk mengubah ukurannya. |

Saat tanggal berganti, Daily Quest mengarsipkan hari sebelumnya, mempertahankan daftar aktivitas, lalu mengosongkan centangnya untuk hari baru.

### Menjadwalkan quest mendatang

- Composer menyediakan pilihan **Hari ini** dan delapan tanggal berikutnya berdasarkan tanggal lokal Windows.
- Quest mendatang disimpan dalam antrean terpisah. Sebelum jatuh tempo, quest tersebut tidak memengaruhi checklist Hari ini, progres, mode ringkas, atau Riwayat.
- Saat tanggalnya tiba, quest ditambahkan ke Hari ini tanpa centang setelah hari sebelumnya diarsipkan. Setelah itu, quest berperilaku seperti quest aktif biasa dan mengikuti reset harian sampai kamu menghapusnya.
- Jika Daily Quest tidak dibuka pada tanggal target, quest yang lewat jatuh tempo akan diaktifkan saat aplikasi berikutnya dibuka. Setiap quest hanya diaktifkan satu kali.
- Buka daftar mendatang untuk meninjau atau membatalkan quest terjadwal sebelum aktif.

Penjadwalan tidak membuat notifikasi Windows, menjalankan layanan di latar belakang, atau membuka Daily Quest secara otomatis.

### Pengaturan dan bantuan

- **Bahasa:** pilih Bahasa Indonesia atau English secara langsung. Pilihan disimpan secara lokal.
- **Penyimpanan:** lihat ukuran executable aplikasi, folder data lokal Daily Quest, dan data riwayat yang diserialisasi. Nilai ini merupakan perkiraan lokal dan dapat dibulatkan pada antarmuka.
- **Hapus riwayat:** menghapus permanen arsip riwayat tanpa menghapus quest aktif atau terjadwal. Entri riwayat untuk hari ini dapat dibuat kembali setelah checklist berubah lagi.
- **Q&A:** buka [Pertanyaan yang Sering Diajukan](docs/FAQ.md) dalam dua bahasa.
- **Laporkan bug:** buka formulir issue GitHub yang sudah disiapkan melalui browser bawaan.

Footer pada tampilan Hari ini yang penuh menampilkan nama dan versi aplikasi yang terpasang.

### Navigasi keyboard

| Tombol | Fungsi |
| --- | --- |
| `Enter` | Tambahkan aktivitas saat kolom input aktif. |
| `Esc` | Kosongkan teks yang sedang diketik tanpa menambahkannya. |
| `Tab` / `Shift+Tab` | Pindahkan fokus maju atau mundur. |
| `Space` | Aktifkan checkbox atau tombol yang sedang difokuskan. |

Daily Quest tidak mendaftarkan pintasan keyboard global.

## Data dan privasi

Semua data tetap tersimpan di perangkat pada:

```text
%LOCALAPPDATA%\DailyQuest\state.json
```

File state berisi teks dan urutan quest aktif maupun terjadwal, tanggal target, riwayat harian, bahasa, preferensi selalu di atas, serta ukuran jendela. Daily Quest tidak membutuhkan akun, tidak memiliki telemetri, dan tidak mengunggah data tersebut ke mana pun. Mode ringkas tidak menggantikan fungsi minimize bawaan dan tidak disimpan sebagai state checklist terpisah.

Panel penyimpanan hanya membaca ukuran file dari lokasi aplikasi dan data lokal. **Data tersimpan** mencakup antrean quest mendatang, sedangkan **Riwayat** hanya memperkirakan ukuran catatan riwayat yang diserialisasi. **Hapus riwayat** hanya menghapus catatan riwayat; quest aktif dan terjadwal tetap ada. Riwayat hari ini dapat dibuat kembali setelah checklist diubah berikutnya.

Fungsi checklist utama Daily Quest tetap berjalan tanpa koneksi internet. Memilih **Q&A** atau **Laporkan bug** akan membuka halaman GitHub melalui browser bawaan; tindakan opsional ini membutuhkan internet dan selanjutnya mengikuti praktik privasi GitHub.

Untuk membuat backup manual, tutup aplikasi lalu salin folder `%LOCALAPPDATA%\DailyQuest`. Jika ingin memulai dari awal tanpa langsung menghapus data, tutup aplikasi lalu ubah nama folder tersebut; folder bersih akan dibuat saat aplikasi dibuka kembali.

Jika file state tidak dapat dibaca, Daily Quest menyimpan salinan bertanda waktu dengan nama `state.json.broken-*` sebelum memulai state baru yang bersih.

### Upgrade dari Morning Check-in

Jika `%LOCALAPPDATA%\DailyQuest\state.json` belum ada, Daily Quest memvalidasi dan menyalin file lama `%LOCALAPPDATA%\MorningCheckIn\state.json` ke lokasi baru. File lama tidak diubah dan tetap tersedia sebagai cadangan rollback. Jika kedua file sudah ada, state Daily Quest menjadi prioritas.

## Build dari source

Kebutuhan:

- Windows 10 atau Windows 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git

```powershell
git clone https://github.com/samuelraindrwn/daily-quest.git
cd daily-quest
dotnet restore .\DailyQuest.csproj
dotnet build .\DailyQuest.csproj -c Release
dotnet run --project .\DailyQuest.csproj
```

## Jalankan tes

```powershell
dotnet run --project .\tests\DailyQuest.LogicTests\DailyQuest.LogicTests.csproj -c Release
```

Test harness tanpa dependency eksternal ini mencakup perubahan dan urutan checklist, validasi tanggal dan penjadwalan quest mendatang, aktivasi due maupun overdue, logika pemilihan mode ringkas, pergantian tanggal, penyimpanan dan penghapusan riwayat, pengaturan bahasa, laporan penggunaan penyimpanan, persistence JSON, pemulihan state rusak, dan migrasi state lama.

## Buat build portable

```powershell
dotnet publish .\DailyQuest.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o .\artifacts\DailyQuest-win-x64
```

Aplikasi portable akan dibuat di `artifacts\DailyQuest-win-x64\DailyQuest.exe`.

## Struktur proyek

```text
Assets/          Ikon aplikasi dan logo
Infrastructure/ Helper command
Localization/   Teks antarmuka English dan Bahasa Indonesia
Models/          Data quest aktif, terjadwal, dan riwayat yang disimpan
Services/        Penyimpanan JSON, migrasi state, dan laporan penggunaan penyimpanan
ViewModels/      Logika checklist, penjadwalan, urutan, progres, pengaturan, bahasa, dan riwayat
tests/           Runner tes logika tanpa dependency eksternal
```

## Pemecahan masalah

- **Windows menampilkan “unknown publisher”:** aplikasi belum code-signed. Gunakan hanya rilis resmi dan verifikasi checksum SHA-256-nya.
- **Membuka aplikasi lagi tidak membuat jendela kedua:** Daily Quest hanya mengizinkan satu instance dan akan memulihkan jendela yang sudah ada.
- **Widget ringkas tidak masuk ke taskbar:** mode ringkas mempertahankan jendela quest kecil agar tetap terlihat. Gunakan tombol `−` untuk fungsi minimize bawaan Windows.
- **Riwayat hari ini muncul lagi setelah dihapus:** checklist aktif memang dipertahankan, sehingga ringkasan hari ini dapat ditulis ulang setelah quest berubah. Hapus riwayat setelah selesai melakukan perubahan jika ingin tampilan Riwayat tetap kosong untuk sementara.
- **Proyek source melaporkan SDK tidak ditemukan:** pasang .NET 10 SDK, lalu pastikan versinya muncul melalui `dotnet --list-sdks`.
- **Aplikasi tiba-tiba dimulai dengan state kosong:** periksa folder data untuk backup `state.json.broken-*` yang dibuat dari JSON yang tidak dapat dibaca.
- **Migrasi data lama gagal:** state Morning Check-in lama tetap utuh dan salinan yang gagal disimpan sebagai `state.json.migration-broken-*` di folder data Daily Quest.

## Kontribusi

Laporan bug dan pull request yang terarah sangat diterima. Gunakan tombol **Laporkan bug** di aplikasi atau [buat laporan bug](https://github.com/samuelraindrwn/daily-quest/issues/new?template=bug_report.yml) dengan langkah reproduksi yang jelas, lalu jalankan perintah tes di atas sebelum mengirim perubahan kode. Untuk pertanyaan penggunaan, baca [Q&A bilingual](docs/FAQ.md) terlebih dahulu.
