using System.Globalization;

namespace DailyQuest.Localization;

public sealed record UiCopy
{
    public required CultureInfo Culture { get; init; }

    public string WindowTitle { get; init; } = "Daily Quest";
    public string TodayTab { get; init; } = "Hari ini";
    public string HistoryTab { get; init; } = "Riwayat";
    public string UpcomingTab { get; init; } = "Mendatang";
    public string SettingsTab { get; init; } = "Pengaturan";
    public string PinWidgetAutomation { get; init; } = "Sematkan widget";
    public string PinOffTooltip { get; init; } = "Lepas dari paling depan";
    public string PinOnTooltip { get; init; } = "Selalu tampil paling depan";
    public string CompactMode { get; init; } = "Mode ringkas";
    public string ExpandMode { get; init; } = "Kembali ke tampilan lengkap";
    public string Minimize { get; init; } = "Minimalkan";
    public string Close { get; init; } = "Tutup";
    public string DeleteActivity { get; init; } = "Hapus aktivitas";
    public string ReorderActivity { get; init; } = "Tarik untuk mengubah urutan";
    public string CompactAllDone { get; init; } = "Semua quest selesai";
    public string EmptyTitle { get; init; } = "Daftar masih kosong";
    public string EmptyHint { get; init; } = "Tambahkan satu kebiasaan kecil di bawah.";
    public string NewItemTooltip { get; init; } = "Ketik aktivitas baru, lalu tekan Enter";
    public string NewItemPlaceholder { get; init; } = "Tambah aktivitas...";
    public string AddActivity { get; init; } = "Tambah aktivitas";
    public string ScheduleFor { get; init; } = "Jadwalkan untuk";
    public string ScheduleToday { get; init; } = "Hari ini";
    public string ScheduleTomorrow { get; init; } = "Besok";
    public string ScheduleOffsetFormat { get; init; } = "H+{0}";
    public string ScheduleOptionFormat { get; init; } = "{0} · {1}";
    public string AutoSaveStatus { get; init; } = "Tersimpan · reset harian";
    public string ClearCompleted { get; init; } = "Hapus selesai";
    public string Reset { get; init; } = "Reset";
    public string ResetTooltip { get; init; } = "Kosongkan semua centang hari ini";
    public string MorningGreeting { get; init; } = "Selamat pagi!";
    public string NoonGreeting { get; init; } = "Selamat siang!";
    public string AfternoonGreeting { get; init; } = "Selamat sore!";
    public string EveningGreeting { get; init; } = "Selamat malam!";
    public string ProgressEmpty { get; init; } = "Belum ada aktivitas";
    public string ProgressFormat { get; init; } = "{0} dari {1} selesai";
    public string EncourageEmpty { get; init; } = "Mulai dari satu hal kecil buat pagi ini.";
    public string EncourageDone { get; init; } = "Semua beres. Hari ini punya lo!";
    public string EncourageNone { get; init; } = "Pelan-pelan, mulai dari yang paling gampang.";
    public string EncourageRemainingFormat { get; init; } = "Tinggal {0} lagi. Lanjut!";
    public string HistoryEmptyTitle { get; init; } = "Belum ada riwayat";
    public string HistoryEmptyHint { get; init; } = "Riwayat akan muncul setelah kamu mulai mengisi checklist.";
    public string HistorySummaryFormat { get; init; } = "{0} dari {1} selesai";
    public string UpcomingTitle { get; init; } = "Quest mendatang";
    public string UpcomingHint { get; init; } = "Quest akan masuk ke daftar aktif saat harinya tiba.";
    public string UpcomingEmptyTitle { get; init; } = "Belum ada quest mendatang";
    public string UpcomingEmptyHint { get; init; } = "Pilih Besok sampai H+8 saat menambahkan quest.";
    public string RemoveScheduledQuest { get; init; } = "Hapus quest terjadwal";
    public string UpcomingDateFormat { get; init; } = "ddd, d MMM";
    public string SettingsTitle { get; init; } = "Pengaturan";
    public string SettingsSubtitle { get; init; } = "Atur pengalaman Daily Quest sesuai kebutuhanmu.";
    public string LanguageSetting { get; init; } = "Bahasa";
    public string IndonesianLanguage { get; init; } = "Bahasa Indonesia";
    public string EnglishLanguage { get; init; } = "English";
    public string StorageTitle { get; init; } = "Penyimpanan";
    public string ApplicationStorage { get; init; } = "Aplikasi";
    public string DataStorage { get; init; } = "Data tersimpan";
    public string HistoryStorage { get; init; } = "Riwayat";
    public string StorageHint { get; init; } = "Data tersimpan hanya di perangkat ini.";
    public string ClearHistory { get; init; } = "Hapus riwayat";
    public string ClearHistoryHint { get; init; } = "Menghapus riwayat tanpa menghapus quest aktif atau terjadwal.";
    public string ClearHistoryConfirmTitle { get; init; } = "Hapus seluruh riwayat?";
    public string ClearHistoryConfirmMessage { get; init; } = "Semua catatan riwayat akan dihapus permanen. Quest aktif dan mendatang tetap aman.";
    public string HelpTitle { get; init; } = "Bantuan";
    public string Qna { get; init; } = "Q&A";
    public string QnaHint { get; init; } = "Jawaban singkat untuk pertanyaan umum.";
    public string ReportBug { get; init; } = "Laporkan bug";
    public string ReportBugHint { get; init; } = "Buka formulir GitHub Issue untuk melaporkan masalah.";
    public string FooterFormat { get; init; } = "Daily Quest · v{0}";
    public string SettingsFooter { get; init; } = "Dibuat untuk membantu satu quest kecil pada satu waktu.";
}

public static class UiCopyCatalog
{
    public const string IndonesianCode = "id-ID";
    public const string EnglishCode = "en-US";

    public static UiCopy Indonesian { get; } = new()
    {
        Culture = CultureInfo.GetCultureInfo(IndonesianCode)
    };

    public static UiCopy English { get; } = new()
    {
        Culture = CultureInfo.GetCultureInfo(EnglishCode),
        TodayTab = "Today",
        HistoryTab = "History",
        UpcomingTab = "Upcoming",
        SettingsTab = "Settings",
        PinWidgetAutomation = "Pin widget",
        PinOffTooltip = "Turn off always on top",
        PinOnTooltip = "Always on top",
        CompactMode = "Compact mode",
        ExpandMode = "Return to full view",
        Minimize = "Minimize",
        Close = "Close",
        DeleteActivity = "Delete activity",
        ReorderActivity = "Drag to change order",
        CompactAllDone = "All quests complete",
        EmptyTitle = "Your list is empty",
        EmptyHint = "Add one small habit below.",
        NewItemTooltip = "Type a new activity, then press Enter",
        NewItemPlaceholder = "Add an activity...",
        AddActivity = "Add activity",
        ScheduleFor = "Schedule for",
        ScheduleToday = "Today",
        ScheduleTomorrow = "Tomorrow",
        ScheduleOffsetFormat = "D+{0}",
        ScheduleOptionFormat = "{0} · {1}",
        AutoSaveStatus = "Autosaved · daily reset",
        ClearCompleted = "Clear done",
        Reset = "Reset",
        ResetTooltip = "Uncheck everything for today",
        MorningGreeting = "Good morning!",
        NoonGreeting = "Good afternoon!",
        AfternoonGreeting = "Good evening!",
        EveningGreeting = "Good evening!",
        ProgressEmpty = "No activities yet",
        ProgressFormat = "{0} of {1} done",
        EncourageEmpty = "Start with one small thing this morning.",
        EncourageDone = "All done. Today is yours!",
        EncourageNone = "Take it easy—start with the simplest one.",
        EncourageRemainingFormat = "{0} left. Keep going!",
        HistoryEmptyTitle = "No history yet",
        HistoryEmptyHint = "History will appear after you start using the checklist.",
        HistorySummaryFormat = "{0} of {1} done",
        UpcomingTitle = "Upcoming quests",
        UpcomingHint = "A quest moves into your active list when its day arrives.",
        UpcomingEmptyTitle = "No upcoming quests",
        UpcomingEmptyHint = "Choose Tomorrow through D+8 when adding a quest.",
        RemoveScheduledQuest = "Remove scheduled quest",
        UpcomingDateFormat = "ddd, MMM d",
        SettingsTitle = "Settings",
        SettingsSubtitle = "Shape Daily Quest around the way you work.",
        LanguageSetting = "Language",
        IndonesianLanguage = "Indonesian",
        EnglishLanguage = "English",
        StorageTitle = "Storage",
        ApplicationStorage = "Application",
        DataStorage = "Saved data",
        HistoryStorage = "History",
        StorageHint = "Saved data stays only on this device.",
        ClearHistory = "Clear history",
        ClearHistoryHint = "Deletes history without removing active or scheduled quests.",
        ClearHistoryConfirmTitle = "Clear all history?",
        ClearHistoryConfirmMessage = "Every history record will be permanently removed. Active and upcoming quests will stay safe.",
        HelpTitle = "Help",
        Qna = "Q&A",
        QnaHint = "Quick answers to common questions.",
        ReportBug = "Report a bug",
        ReportBugHint = "Open a GitHub Issue form to report a problem.",
        FooterFormat = "Daily Quest · v{0}",
        SettingsFooter = "Made to help with one small quest at a time."
    };

    public static string NormalizeLanguageCode(string? code) => code?.Trim().ToLowerInvariant() switch
    {
        "en" or "en-us" => EnglishCode,
        "id" or "id-id" => IndonesianCode,
        _ => IndonesianCode
    };

    public static UiCopy For(string? code) =>
        NormalizeLanguageCode(code) == EnglishCode ? English : Indonesian;
}
