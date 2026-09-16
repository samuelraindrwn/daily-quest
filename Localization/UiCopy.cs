using System.Globalization;

namespace DailyQuest.Localization;

public sealed record UiCopy
{
    public required CultureInfo Culture { get; init; }

    public string WindowTitle { get; init; } = "Daily Quest";
    public string TodayTab { get; init; } = "Hari ini";
    public string HistoryTab { get; init; } = "Riwayat";
    public string LanguageTooltip { get; init; } = "Ganti bahasa aplikasi";
    public string PinWidgetAutomation { get; init; } = "Sematkan widget";
    public string PinOffTooltip { get; init; } = "Lepas dari paling depan";
    public string PinOnTooltip { get; init; } = "Selalu tampil paling depan";
    public string Minimize { get; init; } = "Minimalkan";
    public string Close { get; init; } = "Tutup";
    public string DeleteActivity { get; init; } = "Hapus aktivitas";
    public string EmptyTitle { get; init; } = "Daftar masih kosong";
    public string EmptyHint { get; init; } = "Tambahkan satu kebiasaan kecil di bawah.";
    public string NewItemTooltip { get; init; } = "Ketik aktivitas baru, lalu tekan Enter";
    public string NewItemPlaceholder { get; init; } = "Tambah aktivitas...";
    public string AddActivity { get; init; } = "Tambah aktivitas";
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
        LanguageTooltip = "Switch app language",
        PinWidgetAutomation = "Pin widget",
        PinOffTooltip = "Turn off always on top",
        PinOnTooltip = "Always on top",
        Minimize = "Minimize",
        Close = "Close",
        DeleteActivity = "Delete activity",
        EmptyTitle = "Your list is empty",
        EmptyHint = "Add one small habit below.",
        NewItemTooltip = "Type a new activity, then press Enter",
        NewItemPlaceholder = "Add an activity...",
        AddActivity = "Add activity",
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
        HistorySummaryFormat = "{0} of {1} done"
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
