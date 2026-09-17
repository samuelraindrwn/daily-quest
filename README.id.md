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

Daily Quest membuat prioritas hari ini selalu terlihat tanpa mengubahnya menjadi sistem manajemen proyek yang rumit. Aplikasi ini menggabungkan check-in cepat, label yang dapat diatur, timer quest opsional, pengurutan fleksibel, penyimpanan lokal otomatis, progres harian, dan riwayat yang dapat dibuka saat dibutuhkan dalam sebuah widget desktop ringkas.

Panduan ini membahas Daily Quest v1.5.0.

<p align="center">
  <img src="docs/images/daily-quest.png" width="360" alt="Jendela utama Daily Quest berbahasa Indonesia dengan timer quest berjalan">
</p>

## Fitur utama

- Checklist dimulai kosong—rutinitas tetap sepenuhnya milikmu.
- Tambah, centang, dan hapus aktivitas dalam beberapa klik.
- Tambahkan quest langsung ke Hari ini, atau jadwalkan untuk tanggal Besok hingga H+8.
- Tetapkan label berwarna yang dapat diatur saat menambahkan quest, atau ubah labelnya nanti melalui kartu quest.
- Tambahkan quest baru tanpa timer, dengan preset 5/10/15/25/30/45/60 menit, atau durasi khusus dari 1 sampai 480 menit.
- Mulai, jeda, lanjutkan, atau reset hitung mundur quest, dengan maksimal satu timer berjalan dalam satu waktu.
- Pada rilis resmi Windows, dengarkan ringtone facility-alarm bawaan aplikasi selama maksimal satu menit dan terima satu notifikasi bawaan saat waktu habis selama Daily Quest terbuka, dalam mode ringkas, atau diminimalkan; lanjutkan secara opsional ke overtime merah setelah mematikannya.
- Lihat sapaan, jumlah selesai, persentase, dan progress bar hari ini secara langsung.
- Jadikan setiap quest aktif sebagai Daily Quest: pertahankan untuk hari berikutnya dan reset status selesainya secara otomatis.
- Pindahkan quest yang selesai ke urutan paling bawah secara otomatis agar prioritas yang belum selesai tetap di atas.
- Buka riwayat hari sebelumnya; klik kartu tanggal untuk melihat detail aktivitasnya.
- Pertahankan aktivitas selesai di riwayat setelah dibersihkan dari daftar aktif hari ini.
- Pertahankan urutan manual yang tersimpan, atau urutkan quest belum selesai berdasarkan label, durasi tersingkat, atau durasi terlama; quest selesai selalu berada di bawah.
- Ringkas widget menjadi lebih kecil di pojok kanan atas untuk memprioritaskan quest bertimer aktif, lalu quest berikutnya yang belum selesai, dengan tombol pin dan perluas selalu tersedia.
- Instalasi baru dimulai dalam English; pilih English atau Bahasa Indonesia melalui Pengaturan tanpa mengubah preferensi pengguna lama.
- Pilih tema Terang atau Gelap, kelola label, serta aktifkan atau nonaktifkan overtime melalui Pengaturan; semua preferensi disimpan secara lokal.
- Sematkan widget di atas jendela lain, minimalkan ke taskbar, pindahkan, atau ubah ukuran jendela penuhnya dengan bebas.
- Buka di pojok kanan atas area kerja utama dengan jarak yang nyaman dari tepi, sekaligus memulihkan ukuran, bahasa, tema, dan preferensi pin. Kembalikan ukuran jendela penuh ke default 520 × 680 melalui Pengaturan saat dibutuhkan.
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
| Pilihan tanggal di composer | Pertahankan **Hari ini** untuk langsung menambahkan quest, atau pilih **Besok** hingga **H+8**. |
| Pilihan timer di composer | Pertahankan **Tanpa timer**, pilih 5, 10, 15, 25, 30, 45, atau 60 menit, atau masukkan durasi khusus dari 1 sampai 480 menit. |
| Pilihan label di composer | Pertahankan **Tanpa label** atau tetapkan salah satu label yang sudah kamu atur ke quest baru. |
| Checkbox | Tandai aktivitas sebagai selesai atau belum selesai. Quest yang selesai otomatis pindah ke urutan paling bawah. |
| Kontrol timer pada quest | Mulai atau jeda hitung mundur, lanjutkan timer yang dijeda, atau reset ke durasi penuh quest. |
| **Overtime** pada quest yang waktunya habis | Saat mode overtime aktif, langsung matikan alarm dan lanjutkan hitungan naik berwarna merah sampai dijeda, di-reset, atau diselesaikan. |
| Label pada quest | Ubah atau hapus label yang ditetapkan ke quest yang sudah ada. |
| Kontrol urutan | Gunakan urutan manual tersimpan, urutan label, durasi tersingkat, atau durasi terlama. |
| Handle di samping aktivitas | Dalam mode urutan **Manual**, tarik dan lepas aktivitas untuk mengubah urutan tersimpannya. |
| `×` di samping aktivitas | Hapus aktivitas tersebut dari checklist aktif. |
| **Hapus selesai** | Hapus aktivitas selesai dari daftar aktif, tetapi pertahankan riwayat selesainya. |
| **Reset** | Hilangkan semua centang untuk hari ini. |
| **Hari ini** | Kembali ke checklist aktif. |
| **Mendatang** | Tinjau atau batalkan quest yang dijadwalkan untuk tanggal mendatang. |
| **Riwayat** | Lihat ringkasan progres per hari. Klik kartunya untuk membuka detail. |
| Tombol Pengaturan | Buka pilihan bahasa, tema, label, overtime, penyimpanan, riwayat, Q&A, dan laporan bug. |
| Tombol mode ringkas | Ringkas widget di pojok kanan atas dan tampilkan quest bertimer aktif, atau quest berikutnya yang belum selesai saat tidak ada timer berjalan. Selesaikan quest itu untuk lanjut ke quest berikutnya dalam kondisi belum dicentang. |
| Tombol pin di mode ringkas | Pertahankan widget ringkas di atas jendela lain atau kembalikan ke urutan jendela normal. |
| Tombol perluas | Kembali dari mode ringkas ke widget penuh. |
| Tombol pin | Aktifkan atau nonaktifkan mode selalu di atas. |
| Tombol `−` | Minimalkan jendela ke taskbar Windows. Fungsi ini terpisah dari mode ringkas. |
| Header dan tepi jendela | Tarik header untuk memindahkan widget, atau tarik tepi untuk mengubah ukurannya. Ukuran jendela penuh tetap tersimpan setelah aplikasi ditutup. |

Setiap quest aktif merupakan **Daily Quest**. Saat tanggal berganti, Daily Quest mengarsipkan hari sebelumnya, mempertahankan daftar quest aktif, lalu mengosongkan semua centangnya untuk hari baru. Quest akan terus muncul setiap hari sampai kamu menghapusnya.

### Menggunakan timer quest

- Pilih **Tanpa timer** atau suatu durasi saat menambahkan quest. Preset tersedia untuk 5, 10, 15, 25, 30, 45, dan 60 menit; timer khusus menerima bilangan bulat dari 1 sampai 480 menit.
- Gunakan kontrol timer pada quest untuk memulai, menjeda, melanjutkan, atau me-reset hitung mundur. Hanya satu timer quest yang dapat berjalan dalam satu waktu.
- Hitung mundur yang berjalan disimpan bersama timestamp. Jika Daily Quest ditutup lalu dibuka kembali, waktu yang telah berlalu dihitung dari timestamp tersebut sehingga timer tidak dimulai ulang.
- Timer yang mencapai nol tidak otomatis menandai quest sebagai selesai. Selesaikan quest secara terpisah melalui checkbox.
- Mode overtime nonaktif secara default. Saat nonaktif, timer yang habis tetap memutar alarm yang sama selama maksimal satu menit dan tidak menampilkan tindakan **Overtime**.
- Pada rilis resmi Windows, setiap timer yang habis memutar ringtone bawaan aplikasi secara berulang selama maksimal 60 detik dan menampilkan satu notifikasi bawaan. Build dari source tanpa aset ringtone opsional akan memakai bunyi sistem Windows bergantian. Bunyi berhenti lebih awal saat quest di-reset, diselesaikan, atau dihapus; saat overtime dimatikan melalui Pengaturan; ketika hari berganti; atau ketika Daily Quest ditutup.
- Saat mode overtime aktif, timer yang habis menawarkan **Overtime**. Memilihnya akan langsung mematikan alarm dan memulai hitungan naik berwarna merah sampai timer dijeda atau di-reset, atau quest diselesaikan.
- Alarm timer dan notifikasi bawaan Windows bekerja pada tampilan penuh, mode ringkas, dan ketika jendela diminimalkan selama proses Daily Quest masih berjalan.
- Daily Quest tidak menjalankan layanan latar belakang, sehingga alarm dan notifikasi tidak dapat muncul saat proses aplikasi benar-benar ditutup.

### Label dan pengurutan

- Instalasi baru dan state yang dimigrasikan dari versi sebelum dukungan label dimulai dengan **Important**, **Personal**, dan **Routine**. Ketiganya merupakan label awal, bukan label sistem permanen: kamu dapat mengganti nama, warna, menariknya ke urutan prioritas baru, atau menghapusnya, dan daftar label yang sengaja dikosongkan akan tetap kosong.
- Kamu dapat menyimpan maksimal 12 label. Setiap nama harus unik dan maksimal 24 karakter; warna menggunakan format heksadesimal `#RRGGBB`.
- Urutan label menentukan mode urutan **Label**. Quest belum selesai tanpa label ditempatkan setelah quest belum selesai yang memiliki label.
- Di Pengaturan, tarik label melalui handle enam titik untuk mengubah urutan prioritas tersebut. Draft nama dan warna tetap dipertahankan saat label dipindahkan.
- **Tersingkat** dan **Terlama** menggunakan durasi timer yang diatur pada quest. Quest belum selesai tanpa timer ditempatkan setelah quest bertimer pada kedua mode durasi.
- Quest selesai selalu berada di bawah quest belum selesai pada setiap mode urutan.
- Drag and drop hanya tersedia dalam mode **Manual**. Urutan otomatis tidak menimpa urutan manual tersimpan sehingga kembali ke **Manual** akan memulihkannya.
- Menghapus label hanya melepaskan label tersebut dari quest aktif, terjadwal, dan riwayat; tindakan ini tidak pernah menghapus quest.

### Menjadwalkan quest mendatang

- Composer menyediakan pilihan **Hari ini**, **Besok**, dan **H+2** sampai **H+8** berdasarkan tanggal lokal Windows.
- Ringkasan progres disembunyikan selama pemilih tanggal terbuka agar jadwal tetap jelas.
- Quest mendatang disimpan dalam antrean terpisah. Sebelum jatuh tempo, quest tersebut tidak memengaruhi checklist Hari ini, progres, mode ringkas, atau Riwayat.
- Saat tanggalnya tiba, quest ditambahkan ke Hari ini tanpa centang setelah hari sebelumnya diarsipkan. Setelah itu, quest berperilaku seperti quest aktif biasa dan mengikuti reset harian sampai kamu menghapusnya.
- Jika Daily Quest tidak dibuka pada tanggal target, quest yang lewat jatuh tempo akan diaktifkan saat aplikasi berikutnya dibuka. Setiap quest hanya diaktifkan satu kali.
- Buka daftar mendatang untuk meninjau atau membatalkan quest terjadwal sebelum aktif.

Penjadwalan saja tidak membuat notifikasi Windows, menjalankan layanan di latar belakang, atau membuka Daily Quest secara otomatis. Notifikasi timer yang habis mengikuti perilaku yang dijelaskan di atas.

### Pengaturan dan bantuan

- **Tema:** pilih Terang atau Gelap. Perubahan langsung diterapkan dan disimpan secara lokal untuk peluncuran berikutnya.
- **Ukuran jendela:** ubah ukuran jendela penuh mulai dari 390 × 500 hingga 1200 × 1200. Pilih **Reset ukuran** pada bagian Tampilan untuk mengembalikannya ke default 520 × 680.
- **Bahasa:** pilih Bahasa Indonesia atau English secara langsung. Pilihan disimpan secara lokal.
- **Label:** tambah, ganti nama atau warna, tarik untuk mengubah urutan, maupun hapus hingga 12 label quest. Nama, warna, dan urutan prioritas disimpan secara lokal.
- **Overtime:** aktifkan tindakan **Overtime** untuk timer yang habis, atau biarkan nonaktif secara default. Alarm kedaluwarsa maksimal satu menit tetap bekerja pada kedua mode.
- **Penyimpanan:** lihat ukuran executable aplikasi, folder data lokal Daily Quest, dan data riwayat yang diserialisasi. Nilai ini merupakan perkiraan lokal dan dapat dibulatkan pada antarmuka.
- **Hapus riwayat:** menghapus permanen arsip riwayat tanpa menghapus quest aktif atau terjadwal. Entri riwayat untuk hari ini dapat dibuat kembali setelah checklist berubah lagi.
- **Q&A:** buka [Pertanyaan yang Sering Diajukan](docs/FAQ.md) dalam dua bahasa.
- **Laporkan bug:** buka formulir issue GitHub yang sudah disiapkan melalui browser bawaan.

Footer pada tampilan Hari ini yang penuh menampilkan nama dan versi aplikasi yang terpasang (v1.5.0 untuk rilis ini).

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

File state berisi teks dan urutan quest aktif maupun terjadwal, definisi dan penetapan label, mode urutan yang dipilih beserta urutan manual tersimpan, durasi serta state hitung mundur atau overtime, timestamp timer yang berjalan, tanggal target, riwayat harian, tema, bahasa, preferensi overtime dan selalu di atas, serta ukuran jendela. Daily Quest tidak membutuhkan akun, tidak memiliki telemetri, dan tidak mengunggah data tersebut ke mana pun. Mode ringkas tidak menggantikan fungsi minimize bawaan dan tidak disimpan sebagai state checklist terpisah.

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

Rilis resmi menyematkan **Facility alarm sound** dari Mixkit berdasarkan Mixkit Sound Effects Free License. File WAV mentahnya sengaja tidak disertakan dalam repository source ini. Build source biasa tetap berfungsi penuh dan menggunakan bunyi sistem Windows sebagai fallback timer. Untuk menghasilkan audio yang sama dengan rilis resmi, unduh bunyinya langsung dari Mixkit lalu simpan sebagai `Assets\ringtone\mixkit-facility-alarm-sound-999.wav` sebelum build. Lihat [Pemberitahuan Pihak Ketiga](THIRD_PARTY_NOTICES.md).

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

Test harness tanpa dependency eksternal ini mencakup perubahan checklist, label beserta migrasinya, mode urutan manual dan otomatis, penempatan quest selesai, validasi serta persistence durasi timer, perilaku mulai/jeda/lanjutkan/reset dan overtime, aturan satu timer berjalan, pemulihan hitung mundur berbasis timestamp dan alarm kedaluwarsa, validasi tanggal dan penjadwalan quest mendatang, aktivasi due maupun overdue, logika pemilihan mode ringkas, reset Daily Quest saat pergantian tanggal, penyimpanan dan penghapusan riwayat, pengaturan yang tersimpan, laporan penggunaan penyimpanan, persistence JSON, pemulihan state rusak, dan migrasi state lama.

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
Assets/          Ikon, logo, dan petunjuk ringtone timer lokal opsional
Infrastructure/ Helper command
Localization/   Teks antarmuka English dan Bahasa Indonesia
Models/          Data quest aktif, terjadwal, dan riwayat yang disimpan
Services/        Penyimpanan JSON, migrasi state, laporan penggunaan, dan alarm ringtone Windows
ViewModels/      Logika checklist, label, pengurutan, timer/overtime, penjadwalan, progres, pengaturan, bahasa, dan riwayat
tests/           Runner tes logika tanpa dependency eksternal
```

## Pemecahan masalah

- **Windows menampilkan “unknown publisher”:** aplikasi belum code-signed. Gunakan hanya rilis resmi dan verifikasi checksum SHA-256-nya.
- **Membuka aplikasi lagi tidak membuat jendela kedua:** Daily Quest hanya mengizinkan satu instance dan akan memulihkan jendela yang sudah ada.
- **Widget ringkas tidak masuk ke taskbar:** mode ringkas mempertahankan jendela quest kecil agar tetap terlihat. Gunakan tombol `−` untuk fungsi minimize bawaan Windows.
- **Timer habis tanpa alarm saat aplikasi ditutup:** hitung mundur dipulihkan dari timestamp tersimpan pada peluncuran berikutnya, tetapi Daily Quest tidak dapat memutar bunyi atau mengirim notifikasi ketika prosesnya tidak berjalan.
- **Tombol Overtime tidak ada:** aktifkan mode overtime di Pengaturan sebelum timer habis. Saat overtime nonaktif, alarm tetap berjalan maksimal satu menit, tetapi timer tidak menawarkan overtime.
- **Riwayat hari ini muncul lagi setelah dihapus:** checklist aktif memang dipertahankan, sehingga ringkasan hari ini dapat ditulis ulang setelah quest berubah. Hapus riwayat setelah selesai melakukan perubahan jika ingin tampilan Riwayat tetap kosong untuk sementara.
- **Proyek source melaporkan SDK tidak ditemukan:** pasang .NET 10 SDK, lalu pastikan versinya muncul melalui `dotnet --list-sdks`.
- **Aplikasi tiba-tiba dimulai dengan state kosong:** periksa folder data untuk backup `state.json.broken-*` yang dibuat dari JSON yang tidak dapat dibaca.
- **Migrasi data lama gagal:** state Morning Check-in lama tetap utuh dan salinan yang gagal disimpan sebagai `state.json.migration-broken-*` di folder data Daily Quest.

## Kontribusi

Laporan bug dan pull request yang terarah sangat diterima. Gunakan tombol **Laporkan bug** di aplikasi atau [buat laporan bug](https://github.com/samuelraindrwn/daily-quest/issues/new?template=bug_report.yml) dengan langkah reproduksi yang jelas, lalu jalankan perintah tes di atas sebelum mengirim perubahan kode. Untuk pertanyaan penggunaan, baca [Q&A bilingual](docs/FAQ.md) terlebih dahulu.
