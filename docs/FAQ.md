# Daily Quest Q&A / Tanya Jawab

[English](#english) · [Bahasa Indonesia](#bahasa-indonesia)

## English

### Does Daily Quest require an account or internet connection?

No. The checklist, future-quest schedule, history, theme and language settings, and window preferences work locally without an account or internet connection. The optional **Q&A** and **Report a bug** actions open GitHub in your default browser, so those links require internet access.

### Where is my data stored?

Daily Quest stores its state in:

```text
%LOCALAPPDATA%\DailyQuest\state.json
```

The app does not send this file to a server and does not include telemetry. The state includes the saved Light or Dark theme preference.

### What is the difference between compact mode and minimize?

Compact mode keeps a small Daily Quest window in the top-right corner and shows only the first unfinished quest in your saved order. Completing it advances the view to the next unfinished quest. Use the expand control to return to the full widget.

The `−` button keeps the native Windows behavior: it minimizes Daily Quest to the taskbar. Compact mode has its own separate button. Compact mode is temporary for the current session; a new launch starts in the full view.

### How do I change the quest order?

In the full Today view, drag an activity by its dedicated drag handle and drop it in the desired position. The order is saved automatically and determines which unfinished quest appears first in compact mode.

### How do I schedule a quest for another day?

Type the quest in the composer, then choose tomorrow or one of the following seven dates before adding it. Daily Quest supports **D+1 through D+8** (shown as H+1 through H+8 in Indonesian): tomorrow up to eight days from the local date reported by Windows. Keep **Today** selected when the quest should be active immediately.

A scheduled quest stays in the upcoming queue and does not affect Today's checklist, progress, compact mode, or History before it is due. You can open the upcoming list and cancel it while it is still queued.

### What happens if Daily Quest is closed on the scheduled date?

The overdue quest is added to Today, unchecked, the next time Daily Quest opens. It is activated only once. Daily Quest does not create a Windows notification, run in the background, or launch itself automatically.

Once activated, the quest becomes a regular active quest. It follows the normal daily reset and remains in the reusable checklist until you remove it.

### What does Clear history remove?

**Clear history** permanently removes the archived daily history records. It does not remove or uncheck active quests, and it does not cancel scheduled quests in the upcoming queue.

Because the current checklist remains active, Daily Quest can create the current day's history entry again after a quest is added, completed, reordered, reset, or otherwise changed. This is expected and keeps the active day consistent with History.

### What do the storage numbers mean?

- **Application** is the size of the running Daily Quest executable.
- **Saved data** is the space used by files in the local Daily Quest data folder, including active and scheduled quests, state, and any recovery copies.
- **History** is an estimate of the serialized history records inside the state.

Displayed values may be rounded and do not represent Windows filesystem allocation exactly.

### How do I change the language?

Open Settings and choose **Indonesian** or **English**. The selection is saved locally and applies immediately.

### How do I change the appearance?

Open Settings, find **Theme**, and choose **Light** or **Dark**. The change applies immediately across the full and compact views and is saved for the next launch. This version does not automatically follow the Windows theme.

### What is shown in the footer?

The footer identifies Daily Quest and its installed version.

### How do I back up or reset Daily Quest?

Close the app before copying or changing its data folder.

- To back up your data, copy `%LOCALAPPDATA%\DailyQuest` to a safe location.
- To restore it, put the backed-up folder in the same location while the app is closed.
- To start fresh without immediately deleting the old data, rename the folder. Daily Quest creates a new one at the next launch.

If the state file is unreadable, Daily Quest preserves a timestamped `state.json.broken-*` copy before creating clean state.

### Why does Windows SmartScreen show a warning?

Current builds are not code-signed. Download only from the repository's official [Releases page](https://github.com/samuelraindrwn/daily-quest/releases) and compare the file against the provided `SHA256SUMS.txt` checksum before running it.

### How do I report a bug?

Use **Report a bug** in Settings or open the [bug report form](https://github.com/samuelraindrwn/daily-quest/issues/new?template=bug_report.yml). Include the Daily Quest version from the footer, your Windows version, clear reproduction steps, and what you expected to happen. Do not include private quest text or sensitive files.

---

## Bahasa Indonesia

### Apakah Daily Quest membutuhkan akun atau koneksi internet?

Tidak. Checklist, jadwal quest mendatang, riwayat, pilihan tema dan bahasa, serta preferensi jendela bekerja secara lokal tanpa akun atau koneksi internet. Tindakan opsional **Q&A** dan **Laporkan bug** membuka GitHub melalui browser bawaan sehingga kedua tautan tersebut membutuhkan internet.

### Di mana data saya disimpan?

Daily Quest menyimpan state di:

```text
%LOCALAPPDATA%\DailyQuest\state.json
```

Aplikasi tidak mengirim file ini ke server dan tidak memiliki telemetri. State tersebut mencakup preferensi tema Terang atau Gelap yang tersimpan.

### Apa perbedaan mode ringkas dan minimize?

Mode ringkas mempertahankan jendela kecil Daily Quest di pojok kanan atas dan hanya menampilkan quest belum selesai pertama berdasarkan urutan tersimpan. Menyelesaikannya akan menampilkan quest belum selesai berikutnya. Gunakan kontrol perluas untuk kembali ke widget penuh.

Tombol `−` tetap menjalankan fungsi bawaan Windows: meminimalkan Daily Quest ke taskbar. Mode ringkas memiliki tombolnya sendiri. Mode ringkas hanya berlaku sementara selama sesi berjalan; aplikasi dibuka kembali dalam tampilan penuh pada peluncuran berikutnya.

### Bagaimana cara mengubah urutan quest?

Pada tampilan Hari ini yang penuh, tarik aktivitas melalui handle khusus lalu lepas di posisi yang diinginkan. Urutannya tersimpan otomatis dan menentukan quest belum selesai yang pertama kali tampil dalam mode ringkas.

### Bagaimana cara menjadwalkan quest untuk hari lain?

Ketik quest di composer, lalu pilih besok atau salah satu dari tujuh tanggal berikutnya sebelum menambahkannya. Daily Quest mendukung **H+1 sampai H+8**: mulai besok hingga delapan hari dari tanggal lokal yang dilaporkan Windows. Pertahankan pilihan **Hari ini** jika quest harus langsung aktif.

Quest terjadwal tetap berada di antrean mendatang dan tidak memengaruhi checklist Hari ini, progres, mode ringkas, atau Riwayat sebelum jatuh tempo. Kamu dapat membuka daftar mendatang dan membatalkannya selama quest masih berada dalam antrean.

### Apa yang terjadi jika Daily Quest ditutup pada tanggal yang dijadwalkan?

Quest yang lewat jatuh tempo ditambahkan ke Hari ini tanpa centang saat Daily Quest berikutnya dibuka. Setiap quest hanya diaktifkan satu kali. Daily Quest tidak membuat notifikasi Windows, berjalan di latar belakang, atau membuka dirinya secara otomatis.

Setelah aktif, quest menjadi quest aktif biasa. Quest mengikuti reset harian normal dan tetap berada dalam checklist yang digunakan kembali sampai kamu menghapusnya.

### Apa yang dihapus oleh Hapus riwayat?

**Hapus riwayat** menghapus permanen catatan arsip progres harian. Tindakan ini tidak menghapus atau menghilangkan centang quest aktif dan tidak membatalkan quest terjadwal dalam antrean mendatang.

Karena checklist saat ini tetap aktif, Daily Quest dapat membuat kembali entri riwayat hari ini setelah quest ditambah, diselesaikan, diurutkan ulang, di-reset, atau diubah. Hal ini memang dirancang agar hari aktif tetap konsisten dengan Riwayat.

### Apa arti angka penggunaan penyimpanan?

- **Aplikasi** adalah ukuran executable Daily Quest yang sedang dijalankan.
- **Data tersimpan** adalah ruang yang digunakan file dalam folder data lokal Daily Quest, termasuk quest aktif dan terjadwal, state, serta salinan pemulihan.
- **Riwayat** adalah perkiraan ukuran catatan riwayat yang diserialisasi di dalam state.

Nilai yang ditampilkan dapat dibulatkan dan tidak sama persis dengan alokasi filesystem Windows.

### Bagaimana cara mengganti bahasa?

Buka Pengaturan lalu pilih **Bahasa Indonesia** atau **English**. Pilihan disimpan secara lokal dan langsung diterapkan.

### Bagaimana cara mengganti tampilan?

Buka Pengaturan, cari bagian **Tema**, lalu pilih **Terang** atau **Gelap**. Perubahan langsung diterapkan pada tampilan penuh dan mode ringkas serta disimpan untuk peluncuran berikutnya. Versi ini belum mengikuti tema Windows secara otomatis.

### Apa yang ditampilkan pada footer?

Footer menampilkan identitas Daily Quest dan versi yang terpasang.

### Bagaimana cara membuat backup atau me-reset Daily Quest?

Tutup aplikasi sebelum menyalin atau mengubah folder datanya.

- Untuk backup, salin `%LOCALAPPDATA%\DailyQuest` ke lokasi aman.
- Untuk memulihkan, kembalikan folder backup ke lokasi yang sama saat aplikasi tertutup.
- Untuk memulai dari awal tanpa langsung menghapus data lama, ubah nama folder tersebut. Daily Quest akan membuat folder baru saat diluncurkan berikutnya.

Jika file state tidak dapat dibaca, Daily Quest mempertahankan salinan bertanda waktu `state.json.broken-*` sebelum membuat state bersih.

### Mengapa Windows SmartScreen menampilkan peringatan?

Build saat ini belum ditandatangani secara digital. Unduh hanya dari [halaman Releases](https://github.com/samuelraindrwn/daily-quest/releases) resmi repositori dan bandingkan file dengan checksum `SHA256SUMS.txt` yang disediakan sebelum menjalankannya.

### Bagaimana cara melaporkan bug?

Gunakan **Laporkan bug** di Pengaturan atau buka [formulir laporan bug](https://github.com/samuelraindrwn/daily-quest/issues/new?template=bug_report.yml). Sertakan versi Daily Quest dari footer, versi Windows, langkah reproduksi yang jelas, dan hasil yang diharapkan. Jangan sertakan teks quest pribadi atau file sensitif.
