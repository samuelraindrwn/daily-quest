using System.Text.Json;
using System.IO;
using DailyQuest.Models;

namespace DailyQuest.Services;

public sealed class JsonStateStore : IStateStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly string _statePath;
    private readonly string? _legacyStatePath;

    public JsonStateStore(string? statePath = null)
    {
        if (statePath is not null)
        {
            _statePath = statePath;
            return;
        }

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _statePath = Path.Combine(localAppData, "DailyQuest", "state.json");
        _legacyStatePath = Path.Combine(localAppData, "MorningCheckIn", "state.json");
    }

    public JsonStateStore(string statePath, string legacyStatePath)
    {
        _statePath = statePath;
        _legacyStatePath = legacyStatePath;
    }

    public AppState? Load()
    {
        MigrateLegacyStateIfNeeded();

        if (!File.Exists(_statePath))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(_statePath);
            return JsonSerializer.Deserialize<AppState>(json, JsonOptions)
                ?? throw new JsonException("State JSON contained a null root value.");
        }
        catch (JsonException)
        {
            BackUpUnreadableState();
            return null;
        }
    }

    public void Save(AppState state)
    {
        var directory = Path.GetDirectoryName(_statePath)
            ?? throw new InvalidOperationException("Lokasi penyimpanan state tidak valid.");
        Directory.CreateDirectory(directory);

        var temporaryPath = _statePath + ".tmp";
        try
        {
            var json = JsonSerializer.Serialize(state, JsonOptions);
            File.WriteAllText(temporaryPath, json);
            File.Move(temporaryPath, _statePath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private void BackUpUnreadableState()
    {
        var backupPath = _statePath + $".broken-{DateTime.Now:yyyyMMdd-HHmmss-fff}-{Guid.NewGuid():N}";
        File.Copy(_statePath, backupPath, false);
    }

    private void MigrateLegacyStateIfNeeded()
    {
        if (_legacyStatePath is null ||
            File.Exists(_statePath) ||
            !File.Exists(_legacyStatePath))
        {
            return;
        }

        var targetDirectory = Path.GetDirectoryName(_statePath)
            ?? throw new InvalidOperationException("Lokasi penyimpanan state tidak valid.");
        Directory.CreateDirectory(targetDirectory);

        var migrationId = Guid.NewGuid().ToString("N");
        var temporaryPath = _statePath + $".migration-{migrationId}.tmp";

        try
        {
            File.Copy(_legacyStatePath, temporaryPath, false);

            try
            {
                var legacyJson = File.ReadAllText(temporaryPath);
                _ = JsonSerializer.Deserialize<AppState>(legacyJson, JsonOptions)
                    ?? throw new JsonException("Legacy state JSON contained a null root value.");
            }
            catch (JsonException)
            {
                var brokenPath = _statePath + $".migration-broken-{migrationId}";
                File.Move(temporaryPath, brokenPath, false);
                return;
            }

            try
            {
                File.Move(temporaryPath, _statePath, false);
            }
            catch (IOException) when (File.Exists(_statePath))
            {
                // Another process completed migration first; its target is authoritative.
            }
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
