# Daily Quest Q&A / Tanya Jawab

[English](#english) · [Bahasa Indonesia](#bahasa-indonesia)

## English

This Q&A covers Daily Quest v1.8.0.

### Does Daily Quest require an account or internet connection?

No. The checklist, labels, sorting, quest timers and overtime, future-quest schedule, history, theme and language settings, and window preferences work locally without an account or internet connection. The optional **Q&A** and **Report a bug** actions open GitHub in your default browser, so those links require internet access.

### Where is my data stored?

Daily Quest stores its state in:

```text
%LOCALAPPDATA%\DailyQuest\state.json
```

The app does not send this file to a server and does not include telemetry. The state includes label definitions and assignments, sort and manual-order data, quest timer durations and countdown or overtime state, running-timer timestamps, and saved settings such as theme, language, Windows startup, overtime mode, pin preference, and window size.

### What is the difference between compact mode and minimize?

Compact mode instantly switches to a smaller Daily Quest window in the top-right corner. It prioritizes the quest whose timer is currently running; when no timer is running, it shows the first unfinished quest in the currently selected sort order. Completing it advances to an unchecked next quest. Pin and expand controls remain available in the compact card.

The `−` button keeps the native Windows behavior: it minimizes Daily Quest to the taskbar. Compact mode has its own separate button. Compact mode is temporary for the current session; a new launch starts in the full view.

### How do I change the quest order?

Choose **Manual**, **Label**, **Shortest**, or **Longest** from the sort control. **Label** follows the order configured in Settings and puts unlabeled unfinished quests last. The duration modes put untimed unfinished quests last. Completed quests always remain below unfinished quests in every mode.

Drag and drop is available only in **Manual** mode. The manual order is saved separately and determines which unfinished quest appears first in compact mode while **Manual** is selected. Automatic sorting does not overwrite it, so switching back to **Manual** restores the saved order. A running timer still takes priority in compact mode.

### How do I create and manage quest labels?

Open Settings to add, rename, recolor, or delete labels. Drag a label by its six-dot handle to change its priority order; any unsaved name or color draft stays in place while it moves. You can keep at most 12 labels; each name must be unique and no longer than 24 characters, and each color must use the `#RRGGBB` format. Assign a label in the composer or select the label chip on an existing quest to change it.

Fresh installations and state migrated from a version before labels begin with **Important**, **Personal**, and **Routine**. These are editable starter labels. If you delete every label, Daily Quest keeps the list empty instead of recreating them. Deleting a label only detaches it from active, scheduled, and historical quests; it does not delete any quest.

### Does a quest repeat every day?

Yes. Every active quest is a Daily Quest by default. At the date change, Daily Quest archives the previous day's result, keeps the quest in the active list, and resets its checkbox. An unfinished quest therefore carries into tomorrow as the same active quest, not a new scheduled copy, so rollover does not duplicate it. Completed active quests reset too unless you clear or remove them. A future quest follows the same daily behavior after its scheduled date arrives and it becomes active.

### How do I edit a quest's text or timer?

Right-click the active quest and choose **Edit quest**. You can change its text, enter 1-480 minutes to add or change its timer, or leave the timer field blank to remove it. Changing or removing a duration stops that quest's running or overtime timer and resets the new duration to its full value. Its completion state, label, and identity remain attached to the same quest. Use **Copy to** only when you want a separate quest on Today or another date.

### How do I add and control a quest timer?

Choose a timer in the composer before adding the quest. You can select **No timer**, a 5, 10, 15, 25, 30, 45, or 60-minute preset, or a custom whole-number duration from 1 through 480 minutes.

After adding the quest, use its controls to start or pause the countdown, resume it, or reset it to the full duration. Only one quest timer can run at a time. Reaching zero does not automatically complete the quest; use its checkbox when the work is actually done.

### What happens to a running timer when I close the window or exit the app?

Selecting `X` or pressing `Alt+F4` hides Daily Quest in the system tray instead of ending its process. A running countdown or overtime timer keeps advancing, and its alarm can still fire. Right-click the tray icon and choose **Open Daily Quest** to restore the window.

To stop Daily Quest completely, right-click the tray icon and choose **Exit**. Daily Quest advances the active timer to that moment, pauses it, saves the result, and then closes. The timer remains paused on the next launch. A forced process termination cannot perform this graceful save and pause.

### When will the timer alarm and notification work?

When a timer reaches zero while the Daily Quest process is running, the official Windows release loops its bundled facility-alarm ringtone for up to 60 seconds and shows one native notification. Source builds without the optional ringtone asset use alternating Windows system sounds. This works when the app is in its full view, compact mode, minimized to the taskbar, or hidden in the system tray. The sound stops sooner when the quest is reset, completed, or deleted; when overtime is turned off in Settings; at daily rollover; or when the app exits. With overtime disabled, which is the default, there is no **Overtime** action.

Daily Quest can keep its own process running in the background after you hide the window, but it does not install a separate Windows service. After tray **Exit** or a forced process termination, no live process remains to play the alarm or deliver a timer notification. A forced termination may be reconciled from the last saved timer timestamp on the next launch, but it cannot notify while the process is stopped. An expired timer still does not auto-complete its quest.

### How does overtime mode work?

Enable overtime in Settings before a timer expires. At zero, the same one-minute-maximum alarm plays and the quest offers **Overtime**. Selecting **Overtime** silences the alarm immediately and continues a cumulative timer upward in red from its configured duration, so a one-minute timer begins overtime at `+01:00`. It keeps counting until you pause or reset it, or complete the quest. Reaching zero or entering overtime never completes the quest automatically.

If overtime is disabled, Daily Quest still uses the one-minute-maximum expiry alarm but does not show the **Overtime** action. The overtime preference and timer state are saved locally.

### How do I schedule a quest for another day?

Type the quest in the composer, then choose **Tomorrow** or one of the following seven dates before adding it. The picker supports **Today** through **D+8** (shown as Hari ini through H+8 in Indonesian), based on the local date reported by Windows. Keep **Today** selected when the quest should be active immediately.

A scheduled quest stays in the upcoming queue and does not affect Today's checklist, progress, compact mode, or History before it is due. You can open the upcoming list and cancel it while it is still queued.

### How do I copy an existing quest to another day?

Right-click an active quest, open **Copy to**, and choose **Today**, **Tomorrow**, or **D+2** through **D+8**. Choose **All upcoming days** to create one copy on every date from **Tomorrow** through **D+8**; Today is excluded. The source quest is not changed.

The copy keeps the original text, label, and configured timer duration. It is always created as a fresh unchecked quest with its timer reset and idle, even when the source is completed, running, paused, expired, or in overtime. A copy made for **Today** appears in the active checklist; a copy made for a future date appears in **Upcoming** until it is due. Repeating a copy action appends another fresh copy to each selected destination instead of replacing or merging an existing quest.

### How do I copy every quest from one date to another?

Open **Schedule for**, right-click any date tile—not only **Today**—open **Copy all quests to**, and choose a destination from **Today** through **D+8**. Choose **All upcoming days** to copy the complete source-day snapshot to every date from **Tomorrow** through **D+8**, with Today excluded. When **Today** is the source, Daily Quest copies every quest in the currently visible active list, including completed quests. For a future source date, it copies every quest explicitly scheduled for that exact date.

The source is never moved or changed. Each copy keeps its text, label, and configured timer duration, but starts as a fresh unchecked quest with its timer reset and idle. An empty source is a no-op. If the source and destination are the same date, Daily Quest duplicates the snapshot that existed when the action began exactly once. With **All upcoming days**, a future source date is also one of the destinations, so it receives one fresh copy of its starting snapshot. Daily Quest captures that snapshot once, preventing the newly created copies from cascading into later destinations. Repeating the action appends another fresh set to all eight future dates.

### What happens if Daily Quest is not running on the scheduled date?

The overdue quest is added to Today, unchecked, the next time Daily Quest opens. It is activated only once. Scheduling does not create a Windows notification or background service. Separately, **Launch at startup** is enabled by default, so Daily Quest normally opens when you sign in to Windows unless you turn that preference off.

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

Language, Windows startup, theme, label definitions and order, sort mode, overtime mode, pin preference, and expanded window size are also saved locally and restored on the next launch.

### How do I control whether Daily Quest launches with Windows?

Open Settings and use **Launch at startup**. It is **On** by default and registers Daily Quest only for the current Windows user, without administrator access. Choosing **Off** removes that registration. If you move the portable executable while the setting is on, launch it manually once from the new location so Daily Quest can refresh the saved path. Uninstalling the installed copy removes its matching startup entry without deleting quest data.

### Can I resize the window, and is its size saved?

Yes. Drag an edge of the expanded window to resize it from 390 × 500 up to 1200 × 1200. Daily Quest restores that size after you close and reopen the app. To return to the 520 × 680 default, open Settings > Appearance and select **Reset size**.

### What is shown in the footer?

The footer identifies Daily Quest and its installed version.

### How do I back up or reset Daily Quest?

Right-click the Daily Quest tray icon and choose **Exit** before copying or changing its data folder. Closing the window with `X` or `Alt+F4` only hides the still-running app and is not sufficient.

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

Tanya jawab ini membahas Daily Quest v1.8.0.

### Apakah Daily Quest membutuhkan akun atau koneksi internet?

Tidak. Checklist, label, pengurutan, timer quest dan overtime, jadwal quest mendatang, riwayat, pilihan tema dan bahasa, serta preferensi jendela bekerja secara lokal tanpa akun atau koneksi internet. Tindakan opsional **Q&A** dan **Laporkan bug** membuka GitHub melalui browser bawaan sehingga kedua tautan tersebut membutuhkan internet.

### Di mana data saya disimpan?

Daily Quest menyimpan state di:

```text
%LOCALAPPDATA%\DailyQuest\state.json
```

Aplikasi tidak mengirim file ini ke server dan tidak memiliki telemetri. State tersebut mencakup definisi dan penetapan label, data mode urutan dan urutan manual, durasi serta state hitung mundur atau overtime, timestamp timer yang berjalan, dan pengaturan tersimpan seperti tema, bahasa, startup Windows, mode overtime, preferensi pin, serta ukuran jendela.

### Apa perbedaan mode ringkas dan minimize?

Mode ringkas langsung mengubah Daily Quest menjadi jendela yang lebih kecil di pojok kanan atas. Mode ini memprioritaskan quest dengan timer yang sedang berjalan; jika tidak ada timer berjalan, mode ringkas menampilkan quest belum selesai pertama berdasarkan mode urutan yang sedang dipilih. Menyelesaikannya akan menampilkan quest berikutnya dalam kondisi belum dicentang. Tombol pin dan perluas tetap tersedia pada kartu ringkas.

Tombol `−` tetap menjalankan fungsi bawaan Windows: meminimalkan Daily Quest ke taskbar. Mode ringkas memiliki tombolnya sendiri. Mode ringkas hanya berlaku sementara selama sesi berjalan; aplikasi dibuka kembali dalam tampilan penuh pada peluncuran berikutnya.

### Bagaimana cara mengubah urutan quest?

Pilih **Manual**, **Label**, **Tersingkat**, atau **Terlama** melalui kontrol urutan. **Label** mengikuti urutan label di Pengaturan dan menempatkan quest belum selesai tanpa label paling akhir. Kedua mode durasi menempatkan quest belum selesai tanpa timer paling akhir. Quest selesai selalu berada di bawah quest belum selesai pada setiap mode.

Drag and drop hanya tersedia dalam mode **Manual**. Urutan manual disimpan secara terpisah dan menentukan quest belum selesai yang pertama kali tampil dalam mode ringkas selama **Manual** dipilih. Urutan otomatis tidak menimpanya sehingga kembali ke **Manual** akan memulihkan urutan tersimpan. Timer yang sedang berjalan tetap mendapat prioritas dalam mode ringkas.

### Bagaimana cara membuat dan mengelola label quest?

Buka Pengaturan untuk menambah, mengganti nama atau warna, maupun menghapus label. Tarik label melalui handle enam titik untuk mengubah urutan prioritasnya; draft nama atau warna yang belum disimpan tetap dipertahankan saat label berpindah. Kamu dapat menyimpan maksimal 12 label; setiap nama harus unik dan maksimal 24 karakter, sedangkan setiap warna harus menggunakan format `#RRGGBB`. Tetapkan label melalui composer atau pilih chip label pada quest yang sudah ada untuk mengubahnya.

Instalasi baru dan state yang dimigrasikan dari versi sebelum dukungan label dimulai dengan **Important**, **Personal**, dan **Routine**. Ketiganya merupakan label awal yang dapat diubah. Jika semua label dihapus, Daily Quest mempertahankan daftar kosong dan tidak membuatnya kembali. Menghapus label hanya melepaskannya dari quest aktif, terjadwal, dan riwayat; tindakan ini tidak menghapus quest apa pun.

### Apakah quest berulang setiap hari?

Ya. Setiap quest aktif menjadi Daily Quest secara default. Saat tanggal berganti, Daily Quest mengarsipkan hasil hari sebelumnya, mempertahankan quest dalam daftar aktif, lalu mengosongkan centangnya. Quest yang belum selesai otomatis pindah ke besok sebagai quest aktif yang sama, bukan salinan terjadwal baru, sehingga pergantian hari tidak membuat duplikat. Quest aktif yang sudah selesai juga di-reset kecuali kamu membersihkan atau menghapusnya. Quest mendatang mengikuti perilaku harian yang sama setelah tanggal jadwalnya tiba dan quest tersebut menjadi aktif.

### Bagaimana cara mengubah teks atau timer quest?

Klik kanan quest aktif lalu pilih **Ubah quest**. Kamu dapat mengubah teksnya, mengisi 1-480 menit untuk menambah atau mengganti timer, atau mengosongkan kolom timer untuk menghapusnya. Mengganti atau menghapus durasi akan menghentikan timer berjalan maupun overtime pada quest itu dan me-reset durasi baru ke nilai penuh. Status selesai, label, dan identitasnya tetap melekat pada quest yang sama. Gunakan **Salin ke** hanya jika kamu ingin membuat quest terpisah untuk Hari ini atau tanggal lain.

### Bagaimana cara menambahkan dan mengontrol timer quest?

Pilih timer pada composer sebelum menambahkan quest. Kamu dapat memilih **Tanpa timer**, preset 5, 10, 15, 25, 30, 45, atau 60 menit, maupun durasi khusus berupa bilangan bulat dari 1 sampai 480 menit.

Setelah quest ditambahkan, gunakan kontrolnya untuk memulai atau menjeda hitung mundur, melanjutkannya, atau me-reset ke durasi penuh. Hanya satu timer quest yang dapat berjalan dalam satu waktu. Timer yang mencapai nol tidak otomatis menyelesaikan quest; gunakan checkbox setelah pekerjaannya benar-benar selesai.

### Apa yang terjadi pada timer berjalan saat jendela ditutup atau aplikasi dihentikan?

Memilih `X` atau menekan `Alt+F4` akan menyembunyikan Daily Quest ke system tray, bukan menghentikan prosesnya. Hitung mundur atau timer overtime yang sedang berjalan tetap bertambah dan alarmnya tetap dapat berbunyi. Klik kanan ikon tray lalu pilih **Buka Daily Quest** untuk memulihkan jendela.

Untuk menghentikan Daily Quest sepenuhnya, klik kanan ikon tray lalu pilih **Keluar**. Daily Quest memperbarui timer aktif sampai saat itu, menjedanya, menyimpan hasil, lalu menutup proses. Timer tetap dijeda saat aplikasi dibuka berikutnya. Proses yang dihentikan paksa tidak dapat menjalankan penyimpanan dan jeda secara normal ini.

### Kapan alarm dan notifikasi timer dapat bekerja?

Saat timer mencapai nol selama proses Daily Quest masih berjalan, rilis resmi Windows memutar berulang ringtone facility-alarm bawaan selama maksimal 60 detik dan menampilkan satu notifikasi bawaan. Build dari source tanpa aset ringtone opsional memakai bunyi sistem Windows bergantian. Fitur ini bekerja ketika aplikasi menggunakan tampilan penuh, mode ringkas, diminimalkan ke taskbar, atau tersembunyi di system tray. Bunyi berhenti lebih awal saat quest di-reset, diselesaikan, atau dihapus; saat overtime dimatikan melalui Pengaturan; ketika hari berganti; atau ketika aplikasi dihentikan. Saat overtime nonaktif—yang merupakan pengaturan default—tindakan **Overtime** tidak tersedia.

Daily Quest dapat mempertahankan prosesnya di latar belakang setelah jendela disembunyikan, tetapi tidak memasang layanan Windows terpisah. Setelah memilih **Keluar** dari tray atau proses dihentikan paksa, tidak ada proses aktif yang dapat memutar alarm atau mengirim notifikasi timer. Penghentian paksa mungkin direkonsiliasi dari timestamp timer terakhir yang tersimpan saat aplikasi berikutnya dibuka, tetapi aplikasi tidak dapat mengirim notifikasi selama prosesnya berhenti. Timer yang habis tetap tidak otomatis menyelesaikan quest.

### Bagaimana cara kerja mode overtime?

Aktifkan overtime di Pengaturan sebelum timer habis. Saat mencapai nol, alarm yang sama berbunyi selama maksimal satu menit dan quest menawarkan **Overtime**. Memilih **Overtime** akan langsung mematikan alarm dan melanjutkan timer sebagai hitungan naik kumulatif berwarna merah dari durasi yang ditetapkan, sehingga timer satu menit memulai overtime pada `+01:00`. Hitungan berlanjut sampai kamu menjeda atau me-reset timer, atau menyelesaikan quest. Mencapai nol maupun memasuki overtime tidak pernah otomatis menyelesaikan quest.

Jika overtime nonaktif, Daily Quest tetap menggunakan alarm kedaluwarsa maksimal satu menit tetapi tidak menampilkan tindakan **Overtime**. Preferensi overtime dan state timer disimpan secara lokal.

### Bagaimana cara menjadwalkan quest untuk hari lain?

Ketik quest di composer, lalu pilih **Besok** atau salah satu dari tujuh tanggal berikutnya sebelum menambahkannya. Pemilih tanggal mendukung **Hari ini** sampai **H+8**, berdasarkan tanggal lokal yang dilaporkan Windows. Pertahankan pilihan **Hari ini** jika quest harus langsung aktif.

Quest terjadwal tetap berada di antrean mendatang dan tidak memengaruhi checklist Hari ini, progres, mode ringkas, atau Riwayat sebelum jatuh tempo. Kamu dapat membuka daftar mendatang dan membatalkannya selama quest masih berada dalam antrean.

### Bagaimana cara menyalin quest yang sudah ada ke hari lain?

Klik kanan quest aktif, buka **Salin ke**, lalu pilih **Hari ini**, **Besok**, atau **H+2** hingga **H+8**. Pilih **Semua hari mendatang** untuk membuat satu salinan pada setiap tanggal dari **Besok** hingga **H+8**; Hari ini dikecualikan. Quest sumber tidak berubah.

Salinan mempertahankan teks, label, dan durasi timer yang diatur pada quest sumber. Salinan selalu dibuat sebagai quest baru tanpa centang dengan timer yang di-reset dan belum berjalan, meskipun quest sumber sudah selesai, sedang berjalan, dijeda, habis, atau dalam overtime. Salinan untuk **Hari ini** muncul di checklist aktif; salinan untuk tanggal mendatang muncul di **Mendatang** sampai waktunya tiba. Mengulangi tindakan salin akan menambahkan salinan baru lagi pada setiap tujuan yang dipilih, bukan mengganti atau menggabungkan quest yang sudah ada.

### Bagaimana cara menyalin semua quest dari satu tanggal ke tanggal lain?

Buka **Jadwalkan untuk**, klik kanan kartu tanggal mana pun—bukan hanya **Hari ini**—buka **Salin semua quest ke**, lalu pilih tujuan dari **Hari ini** hingga **H+8**. Pilih **Semua hari mendatang** untuk menyalin snapshot lengkap hari sumber ke setiap tanggal dari **Besok** hingga **H+8**, dengan Hari ini dikecualikan. Jika **Hari ini** menjadi sumber, Daily Quest menyalin semua quest dalam daftar aktif yang sedang ditampilkan, termasuk quest selesai. Untuk tanggal sumber mendatang, aplikasi menyalin semua quest yang dijadwalkan secara khusus untuk tanggal tersebut.

Sumber tidak pernah dipindahkan atau diubah. Setiap salinan mempertahankan teks, label, dan durasi timer yang diatur, tetapi dimulai sebagai quest baru tanpa centang dengan timer yang di-reset dan belum berjalan. Sumber kosong tidak melakukan apa pun. Jika sumber dan tujuan adalah tanggal yang sama, Daily Quest menduplikasi snapshot yang ada saat tindakan dimulai tepat satu kali. Dengan **Semua hari mendatang**, tanggal sumber mendatang juga termasuk tujuan sehingga tanggal tersebut menerima satu salinan baru dari snapshot awalnya. Daily Quest hanya mengambil snapshot itu satu kali agar salinan yang baru dibuat tidak ikut berantai ke tujuan berikutnya. Mengulangi tindakan ini menambahkan satu kumpulan baru ke delapan tanggal mendatang.

### Apa yang terjadi jika Daily Quest tidak berjalan pada tanggal yang dijadwalkan?

Quest yang lewat jatuh tempo ditambahkan ke Hari ini tanpa centang saat Daily Quest berikutnya dibuka. Setiap quest hanya diaktifkan satu kali. Penjadwalan tidak membuat notifikasi Windows atau layanan latar belakang. Secara terpisah, **Jalankan saat startup** aktif secara default sehingga Daily Quest biasanya terbuka saat kamu masuk ke Windows, kecuali preferensi tersebut dimatikan.

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

Bahasa, startup Windows, tema, definisi dan urutan label, mode urutan quest, mode overtime, preferensi pin, serta ukuran jendela penuh juga disimpan secara lokal dan dipulihkan pada peluncuran berikutnya.

### Bagaimana cara mengatur agar Daily Quest dibuka bersama Windows?

Buka Pengaturan lalu gunakan **Jalankan saat startup**. Fitur ini **Aktif** secara default dan hanya mendaftarkan Daily Quest untuk pengguna Windows saat ini tanpa akses administrator. Memilih **Nonaktif** akan menghapus pendaftaran tersebut. Jika executable portable dipindahkan saat pengaturan aktif, jalankan sekali secara manual dari lokasi baru agar Daily Quest memperbarui path tersimpan. Uninstall versi terpasang menghapus entri startup yang cocok tanpa menghapus data quest.

### Apakah ukuran jendela dapat diubah dan tetap tersimpan?

Ya. Tarik tepi jendela penuh untuk mengubah ukurannya mulai dari 390 × 500 hingga 1200 × 1200. Daily Quest akan memulihkan ukuran tersebut setelah aplikasi ditutup dan dibuka kembali. Untuk kembali ke ukuran default 520 × 680, buka Pengaturan > Tampilan lalu pilih **Reset ukuran**.

### Apa yang ditampilkan pada footer?

Footer menampilkan identitas Daily Quest dan versi yang terpasang.

### Bagaimana cara membuat backup atau me-reset Daily Quest?

Klik kanan ikon tray Daily Quest lalu pilih **Keluar** sebelum menyalin atau mengubah folder datanya. Menutup jendela dengan `X` atau `Alt+F4` hanya menyembunyikan aplikasi yang masih berjalan dan tidak cukup untuk langkah ini.

- Untuk backup, salin `%LOCALAPPDATA%\DailyQuest` ke lokasi aman.
- Untuk memulihkan, kembalikan folder backup ke lokasi yang sama saat aplikasi tertutup.
- Untuk memulai dari awal tanpa langsung menghapus data lama, ubah nama folder tersebut. Daily Quest akan membuat folder baru saat diluncurkan berikutnya.

Jika file state tidak dapat dibaca, Daily Quest mempertahankan salinan bertanda waktu `state.json.broken-*` sebelum membuat state bersih.

### Mengapa Windows SmartScreen menampilkan peringatan?

Build saat ini belum ditandatangani secara digital. Unduh hanya dari [halaman Releases](https://github.com/samuelraindrwn/daily-quest/releases) resmi repositori dan bandingkan file dengan checksum `SHA256SUMS.txt` yang disediakan sebelum menjalankannya.

### Bagaimana cara melaporkan bug?

Gunakan **Laporkan bug** di Pengaturan atau buka [formulir laporan bug](https://github.com/samuelraindrwn/daily-quest/issues/new?template=bug_report.yml). Sertakan versi Daily Quest dari footer, versi Windows, langkah reproduksi yang jelas, dan hasil yang diharapkan. Jangan sertakan teks quest pribadi atau file sensitif.
