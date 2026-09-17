using System.IO;
using System.Reflection;
using System.Security;
using Microsoft.Win32;

namespace DailyQuest.Services;

public sealed class WindowsRunAtStartupService : IRunAtStartupService
{
    internal const string DefaultRunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    internal const string DefaultValueName = "DailyQuest";

    private readonly Func<string?> _executablePathProvider;
    private readonly Func<bool> _isExpectedApplicationHost;
    private readonly string _runKeyPath;
    private readonly string _valueName;

    public WindowsRunAtStartupService()
        : this(
            () => Environment.ProcessPath,
            DefaultRunKeyPath,
            DefaultValueName,
            IsDailyQuestApplicationHost)
    {
    }

    internal WindowsRunAtStartupService(
        Func<string?> executablePathProvider,
        string runKeyPath,
        string valueName,
        Func<bool> isExpectedApplicationHost)
    {
        _executablePathProvider = executablePathProvider;
        _runKeyPath = runKeyPath;
        _valueName = valueName;
        _isExpectedApplicationHost = isExpectedApplicationHost;
    }

    public bool TrySetEnabled(bool enabled)
    {
        try
        {
            // The WPF assembly is also loaded by UI/test hosts. Never let one of those
            // processes register its own executable as Daily Quest or remove the real entry.
            if (!_isExpectedApplicationHost())
            {
                return false;
            }

            if (!enabled)
            {
                using var existingKey = Registry.CurrentUser.OpenSubKey(_runKeyPath, writable: true);
                existingKey?.DeleteValue(_valueName, throwOnMissingValue: false);
                return true;
            }

            var executablePath = _executablePathProvider()?.Trim();
            if (string.IsNullOrWhiteSpace(executablePath) ||
                !Path.IsPathFullyQualified(executablePath) ||
                !string.Equals(Path.GetExtension(executablePath), ".exe", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Path.GetFileNameWithoutExtension(executablePath), "dotnet", StringComparison.OrdinalIgnoreCase) ||
                !File.Exists(executablePath))
            {
                return false;
            }

            using var runKey = Registry.CurrentUser.CreateSubKey(_runKeyPath, writable: true);
            if (runKey is null)
            {
                return false;
            }

            runKey.SetValue(
                _valueName,
                CreateStartupCommand(executablePath),
                RegistryValueKind.String);
            return true;
        }
        catch (Exception exception) when (
            exception is IOException or SecurityException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    internal static string CreateStartupCommand(string executablePath) =>
        $"\"{executablePath}\" --startup";

    private static bool IsDailyQuestApplicationHost() =>
        string.Equals(
            Assembly.GetEntryAssembly()?.GetName().Name,
            typeof(WindowsRunAtStartupService).Assembly.GetName().Name,
            StringComparison.Ordinal);
}
