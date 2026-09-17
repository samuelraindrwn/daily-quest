# Daily Quest Windows installer

**English** · [Bahasa Indonesia](README.id.md)

Daily Quest uses Inno Setup to produce a per-user Windows installer around the self-contained `win-x64` executable.

## Installer behavior

- Installs without administrator access to `%LOCALAPPDATA%\Programs\Daily Quest`.
- Adds a Start Menu shortcut and offers an unchecked Desktop shortcut option.
- Uses a stable application ID so a newer installer upgrades the existing installation in place.
- Detects a running Daily Quest instance before replacing or removing files.
- Includes a standard Windows uninstaller.
- Daily Quest enables per-user launch-at-sign-in by default after the app first runs; users can turn it off in Settings.
- The uninstaller removes the startup entry only when it still points to this installed executable, so a separate portable registration is not removed accidentally.
- Preserves `%LOCALAPPDATA%\DailyQuest` and the legacy `%LOCALAPPDATA%\MorningCheckIn` folder during upgrades and uninstall, so quests, history, and preferences remain intact.
- Supports x64 Windows 10/11 and x64 emulation on compatible Windows 11 Arm64 systems.

The current installer and application are not code-signed. Windows SmartScreen may therefore show an unknown-publisher warning. Only distribute installers together with their SHA-256 checksum.

## Requirements

- Windows 10 or Windows 11
- .NET 10 SDK when publishing from source
- Inno Setup 6.3 or newer

Install the compiler for the current user:

```powershell
winget install --id JRSoftware.InnoSetup --exact --scope user
```

## Build from source

The ringtone is optional for a normal application build, but required when reproducing the official installer from source. Place it at the path documented in `Assets\ringtone\README.md`, then run:

```powershell
.\installer\build-installer.ps1
```

The script reads the version from `DailyQuest.csproj`, runs all logic tests with the ringtone validation enabled, publishes the self-contained application, compiles the installer, verifies its version metadata, and writes `SHA256SUMS-installer.txt` beside it.

To package an already-published official executable instead:

```powershell
$publishedExe = 'C:\path\to\DailyQuest.exe'
$expectedSha256 = '<SHA-256 from the official release>'
.\installer\build-installer.ps1 `
  -PublishedExe $publishedExe `
  -ExpectedPublishedExeSha256 $expectedSha256 `
  -OutputRoot .\artifacts\installer\from-release
```

For a project version of `x.y.z`, the default output is:

```text
artifacts\installer\vx.y.z\output\DailyQuest-vx.y.z-win-x64-setup.exe
artifacts\installer\vx.y.z\output\SHA256SUMS-installer.txt
```

Use a fresh `-OutputRoot` for every rebuild. The script intentionally refuses to overwrite an existing completed output. Failed builds remain in a uniquely named staging directory, so they do not block a retry.
