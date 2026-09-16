# Daily Quest Q&A / Tanya Jawab

[English](#english) · [Bahasa Indonesia](#bahasa-indonesia)

## English

This Q&A covers Daily Quest v1.5.0.

### Does Daily Quest require an account or internet connection?

No. The checklist, labels, sorting, quest timers and overtime, future-quest schedule, history, theme and language settings, and window preferences work locally without an account or internet connection. The optional **Q&A** and **Report a bug** actions open GitHub in your default browser, so those links require internet access.

### Where is my data stored?

Daily Quest stores its state in:

```text
%LOCALAPPDATA%\DailyQuest\state.json
```

The app does not send this file to a server and does not include telemetry. The state includes label definitions and assignments, sort and manual-order data, quest timer durations and countdown or overtime state, running-timer timestamps, and saved settings such as theme, language, overtime mode, pin preference, and window size.

### What is the difference between compact mode and minimize?

Compact mode instantly switches to a smaller Daily Quest window in the top-right corner. It prioritizes the quest whose timer is currently running; when no timer is running, it shows the first unfinished quest in the currently selected sort order. Completing it advances to an unchecked next quest. Pin and expand controls remain available in the compact card.

The `−` button keeps the native Windows behavior: it minimizes Daily Quest to the taskbar. Compact mode has its own separate button. Compact mode is temporary for the current session; a new launch starts in the full view.

### How do I change the quest order?

Choose **Manual**, **Label**, **Shortest**, or **Longest** from the sort control. **Label** follows the order configured in Settings and puts unlabeled unfinished quests last. The duration modes put untimed unfinished quests last. Completed quests always remain below unfinished quests in every mode.

Drag and drop is available only in **Manual** mode. The manual order is saved separately and determines which unfinished quest appears first in compact mode while **Manual** is selected. Automatic sorting does not overwrite it, so switching back to **Manual** restores the saved order. A running timer still takes priority in compact mode.

### How do I create and manage quest labels?

Open Settings to add, rename, recolor, reorder, or delete labels. You can keep at most 12 labels; each name must be unique and no longer than 24 characters, and each color must use the `#RRGGBB` format. Assign a label in the composer or select the label chip on an existing quest to change it.

Fresh installations and state migrated from a version before labels begin with **Important**, **Personal**, and **Routine**. These are editable starter labels. If you delete every label, Daily Quest keeps the list empty instead of recreating them. Deleting a label only detaches it from active, scheduled, and historical quests; it does not delete any quest.

### Does a quest repeat every day?

Yes. Every active quest is a Daily Quest by default. At the date change, Daily Quest archives the previous day's result, keeps the quest in the active list, and resets its checkbox. It repeats this way until you remove the quest. A future quest follows the same daily behavior after its scheduled date arrives and it becomes active.

### How do I add and control a quest timer?

Choose a timer in the composer before adding the quest. You can select **No timer**, a 5, 10, 15, 25, 30, 45, or 60-minute preset, or a custom whole-number duration from 1 through 480 minutes.

After adding the quest, use its controls to start or pause the countdown, resume it, or reset it to the full duration. Only one quest timer can run at a time. Reaching zero does not automatically complete the quest; use its checkbox when the work is actually done.

### Does a running timer survive an app restart?

Yes. Daily Quest saves the countdown state and a running timer's timestamp locally. When you reopen the app, it calculates the elapsed time from that timestamp rather than restarting the countdown.

### When will the timer alarm and notification work?

When a timer reaches zero while the Daily Quest process is running, Windows plays an alarm sound and shows a native notification. This works when the app is in its full view, compact mode, or minimized to the taskbar. With overtime disabled, which is the default, the alarm is finite and there is no **Overtime** action.

Daily Quest does not run a background service. If you fully close its process, it cannot play the alarm or deliver the notification while closed; the countdown is reconciled from its saved timestamp the next time the app starts. The expired timer still does not auto-complete its quest.

### How does overtime mode work?

Enable overtime in Settings before a timer expires. At zero, the alarm repeats and the quest offers **Overtime**. Selecting **Overtime** silences the alarm and continues the timer upward in red until you pause or reset it, or complete the quest. Reaching zero or entering overtime never completes the quest automatically.

If overtime is disabled, Daily Quest uses the finite expiry alarm and does not show the **Overtime** action. The overtime preference and timer state are saved locally.

### How do I schedule a quest for another day?

Type the quest in the composer, then choose **Tomorrow** or one of the following seven dates before adding it. The picker supports **Today** through **D+8** (shown as Hari ini through H+8 in Indonesian), based on the local date reported by Windows. Keep **Today** selected when the quest should be active immediately.

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

New installations start in **English**. Open Settings to choose **Indonesian** or **English**; the selection is saved locally and applies immediately. Existing saved preferences remain unchanged after an update.

### How do I change the appearance?

Open Settings, find **Theme**, and choose **Light** or **Dark**. The change applies immediately across the full and compact views and is saved for the next launch. This version does not automatically follow the Windows theme.

Language, theme, label definitions and order, sort mode, overtime mode, pin preference, and expanded window size are also saved locally and restored on the next launch.

### Can I resize the window, and is its size saved?

Yes. Drag an edge of the expanded window to resize it from 390 × 500 up to 1200 × 1200. Daily Quest restores that size after you close and reopen the app. To return to the 520 × 680 default, open Settings > Appearance and select **Reset size**.

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

Tanya jawab ini membahas Daily Quest v1.5.0.

### Apakah Daily Quest membutuhkan akun atau koneksi internet?

Tidak. Checklist, label, pengurutan, timer quest dan overtime, jadwal quest mendatang, riwayat, pilihan tema dan bahasa, serta preferensi jendela bekerja secara lokal tanpa akun atau koneksi internet. Tindakan opsional **Q&A** dan **Laporkan bug** membuka GitHub melalui browser bawaan sehingga kedua tautan tersebut membutuhkan internet.

### Di mana data saya disimpan?

Daily Quest menyimpan state di:

```text
%LOCALAPPDATA%\DailyQuest\state.json
```

Aplikasi tidak mengirim file ini ke server dan tidak memiliki telemetri. State tersebut mencakup definisi dan penetapan label, data mode urutan dan urutan manual, durasi serta state hitung mundur atau overtime, timestamp timer yang berjalan, dan pengaturan tersimpan seperti tema, bahasa, mode overtime, preferensi pin, serta ukuran jendela.

### Apa perbedaan mode ringkas dan minimize?

Mode ringkas langsung mengubah Daily Quest menjadi jendela yang lebih kecil di pojok kanan atas. Mode ini memprioritaskan quest dengan timer yang sedang berjalan; jika tidak ada timer berjalan, mode ringkas menampilkan quest belum selesai pertama berdasarkan mode urutan yang sedang dipilih. Menyelesaikannya akan menampilkan quest berikutnya dalam kondisi belum dicentang. Tombol pin dan perluas tetap tersedia pada kartu ringkas.

Tombol `−` tetap menjalankan fungsi bawaan Windows: meminimalkan Daily Quest ke taskbar. Mode ringkas memiliki tombolnya sendiri. Mode ringkas hanya berlaku sementara selama sesi berjalan; aplikasi dibuka kembali dalam tampilan penuh pada peluncuran berikutnya.

### Bagaimana cara mengubah urutan quest?

Pilih **Manual**, **Label**, **Tersingkat**, atau **Terlama** melalui kontrol urutan. **Label** mengikuti urutan label di Pengaturan dan menempatkan quest belum selesai tanpa label paling akhir. Kedua mode durasi menempatkan quest belum selesai tanpa timer paling akhir. Quest selesai selalu berada di bawah quest belum selesai pada setiap mode.

Drag and drop hanya tersedia dalam mode **Manual**. Urutan manual disimpan secara terpisah dan menentukan quest belum selesai yang pertama kali tampil dalam mode ringkas selama **Manual** dipilih. Urutan otomatis tidak menimpanya sehingga kembali ke **Manual** akan memulihkan urutan tersimpan. Timer yang sedang berjalan tetap mendapat prioritas dalam mode ringkas.

### Bagaimana cara membuat dan mengelola label quest?

Buka Pengaturan untuk menambah, mengganti nama atau warna, mengubah urutan, maupun menghapus label. Kamu dapat menyimpan maksimal 12 label; setiap nama harus unik dan maksimal 24 karakter, sedangkan setiap warna harus menggunakan format `#RRGGBB`. Tetapkan label melalui composer atau pilih chip label pada quest yang sudah ada untuk mengubahnya.

Instalasi baru dan state yang dimigrasikan dari versi sebelum dukungan label dimulai dengan **Important**, **Personal**, dan **Routine**. Ketiganya merupakan label awal yang dapat diubah. Jika semua label dihapus, Daily Quest mempertahankan daftar kosong dan tidak membuatnya kembali. Menghapus label hanya melepaskannya dari quest aktif, terjadwal, dan riwayat; tindakan ini tidak menghapus quest apa pun.

### Apakah quest berulang setiap hari?

Ya. Setiap quest aktif menjadi Daily Quest secara default. Saat tanggal berganti, Daily Quest mengarsipkan hasil hari sebelumnya, mempertahankan quest dalam daftar aktif, lalu mengosongkan centangnya. Quest berulang dengan cara ini sampai kamu menghapusnya. Quest mendatang mengikuti perilaku harian yang sama setelah tanggal jadwalnya tiba dan quest tersebut menjadi aktif.

### Bagaimana cara menambahkan dan mengontrol timer quest?

Pilih timer pada composer sebelum menambahkan quest. Kamu dapat memilih **Tanpa timer**, preset 5, 10, 15, 25, 30, 45, atau 60 menit, maupun durasi khusus berupa bilangan bulat dari 1 sampai 480 menit.

Setelah quest ditambahkan, gunakan kontrolnya untuk memulai atau menjeda hitung mundur, melanjutkannya, atau me-reset ke durasi penuh. Hanya satu timer quest yang dapat berjalan dalam satu waktu. Timer yang mencapai nol tidak otomatis menyelesaikan quest; gunakan checkbox setelah pekerjaannya benar-benar selesai.

### Apakah timer yang berjalan tetap berlanjut setelah aplikasi dibuka kembali?

Ya. Daily Quest menyimpan state hitung mundur dan timestamp timer yang berjalan secara lokal. Saat aplikasi dibuka kembali, waktu yang telah berlalu dihitung dari timestamp tersebut sehingga hitung mundur tidak dimulai ulang.

### Kapan alarm dan notifikasi timer dapat bekerja?

Saat timer mencapai nol selama proses Daily Quest masih berjalan, Windows memutar bunyi alarm dan menampilkan notifikasi bawaan. Fitur ini bekerja ketika aplikasi menggunakan tampilan penuh, mode ringkas, atau diminimalkan ke taskbar. Saat overtime nonaktif—yang merupakan pengaturan default—alarm berbunyi terbatas dan tindakan **Overtime** tidak tersedia.

Daily Quest tidak menjalankan layanan latar belakang. Jika prosesnya benar-benar ditutup, aplikasi tidak dapat memutar alarm atau mengirim notifikasi selama tertutup; hitung mundur disesuaikan dari timestamp tersimpan saat aplikasi berikutnya dibuka. Timer yang sudah habis tetap tidak otomatis menyelesaikan quest.

### Bagaimana cara kerja mode overtime?

Aktifkan overtime di Pengaturan sebelum timer habis. Saat mencapai nol, alarm berbunyi berulang dan quest menawarkan **Overtime**. Memilih **Overtime** akan mematikan alarm dan melanjutkan timer sebagai hitungan naik berwarna merah sampai kamu menjeda atau me-reset timer, atau menyelesaikan quest. Mencapai nol maupun memasuki overtime tidak pernah otomatis menyelesaikan quest.

Jika overtime nonaktif, Daily Quest menggunakan alarm kedaluwarsa terbatas dan tidak menampilkan tindakan **Overtime**. Preferensi overtime dan state timer disimpan secara lokal.

### Bagaimana cara menjadwalkan quest untuk hari lain?

Ketik quest di composer, lalu pilih **Besok** atau salah satu dari tujuh tanggal berikutnya sebelum menambahkannya. Pemilih tanggal mendukung **Hari ini** sampai **H+8**, berdasarkan tanggal lokal yang dilaporkan Windows. Pertahankan pilihan **Hari ini** jika quest harus langsung aktif.

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

Instalasi baru dimulai dalam **English**. Buka Pengaturan untuk memilih **Bahasa Indonesia** atau **English**; pilihan disimpan secara lokal dan langsung diterapkan. Preferensi yang sudah tersimpan tidak berubah setelah pembaruan.

### Bagaimana cara mengganti tampilan?

Buka Pengaturan, cari bagian **Tema**, lalu pilih **Terang** atau **Gelap**. Perubahan langsung diterapkan pada tampilan penuh dan mode ringkas serta disimpan untuk peluncuran berikutnya. Versi ini belum mengikuti tema Windows secara otomatis.

Bahasa, tema, definisi dan urutan label, mode urutan quest, mode overtime, preferensi pin, serta ukuran jendela penuh juga disimpan secara lokal dan dipulihkan pada peluncuran berikutnya.

### Apakah ukuran jendela dapat diubah dan tetap tersimpan?

Ya. Tarik tepi jendela penuh untuk mengubah ukurannya mulai dari 390 × 500 hingga 1200 × 1200. Daily Quest akan memulihkan ukuran tersebut setelah aplikasi ditutup dan dibuka kembali. Untuk kembali ke ukuran default 520 × 680, buka Pengaturan > Tampilan lalu pilih **Reset ukuran**.

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
