using System.IO;
using System.Reflection;
using System.Text.Json;
using DailyQuest.Models;

namespace DailyQuest.Services;

public sealed class StorageUsageService : IStorageUsageService
{
    private readonly string _dataDirectory;
    private readonly string? _applicationPath;

    public StorageUsageService(string? dataDirectory = null, string? applicationPath = null)
    {
        _dataDirectory = dataDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DailyQuest");
        _applicationPath = applicationPath ?? ResolveApplicationPath();
    }

    public StorageUsageSnapshot Measure(IReadOnlyCollection<DailyHistoryState> history)
    {
        var historyBytes = JsonSerializer.SerializeToUtf8Bytes(history).LongLength;
        return new StorageUsageSnapshot(
            GetFileSize(_applicationPath),
            GetDirectorySize(_dataDirectory),
            historyBytes);
    }

    private static long GetFileSize(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return 0;
        }

        try
        {
            return new FileInfo(path).Length;
        }
        catch (IOException)
        {
            return 0;
        }
        catch (UnauthorizedAccessException)
        {
            return 0;
        }
    }

    private static string? ResolveApplicationPath()
    {
        var processPath = Environment.ProcessPath;
        if (!string.Equals(
                Path.GetFileNameWithoutExtension(processPath),
                "dotnet",
                StringComparison.OrdinalIgnoreCase))
        {
            return processPath;
        }

        var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            return processPath;
        }

        var assemblyPath = Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.dll");
        return File.Exists(assemblyPath) ? assemblyPath : processPath;
    }

    private static long GetDirectorySize(string path)
    {
        if (!Directory.Exists(path))
        {
            return 0;
        }

        try
        {
            long total = 0;
            foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                total += GetFileSize(file);
            }

            return total;
        }
        catch (IOException)
        {
            return 0;
        }
        catch (UnauthorizedAccessException)
        {
            return 0;
        }
    }
}
