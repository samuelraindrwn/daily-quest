<p align="center">
  <img src="Assets/DailyQuest-logo.png" width="112" alt="Daily Quest logo">
</p>

<h1 align="center">Daily Quest</h1>

<p align="center">
  A lightweight, local-first daily checklist widget for Windows, wrapped in a minimal liquid-glass interface.
</p>

<p align="center">
  <strong>English</strong> · <a href="README.id.md">Bahasa Indonesia</a>
</p>

<p align="center">
  <a href="https://github.com/samuelraindrwn/daily-quest/releases/latest"><img src="https://img.shields.io/github/v/release/samuelraindrwn/daily-quest?label=release" alt="Latest release"></a>
  <img src="https://img.shields.io/badge/platform-Windows%2010%2F11-2563EB" alt="Windows 10/11">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4" alt=".NET 10">
</p>

Daily Quest keeps today's priorities visible without turning them into a complicated project-management system. It combines quick check-ins, customizable labels, optional quest timers, flexible sorting, automatic local saving, progress tracking, and expandable daily history in a compact desktop widget.

This guide covers Daily Quest v1.5.0.

<p align="center">
  <img src="docs/images/daily-quest-en.png" width="360" alt="Daily Quest main window in English showing a running quest timer">
</p>

## Highlights

- Starts with an empty checklist—your routine stays yours.
- Add, complete, and remove activities in a few clicks.
- Add a quest to Today immediately, or schedule it for any date from Tomorrow through D+8.
- Assign a customizable color label when adding a quest, or change it later from the quest card.
- Give a new quest no timer, a 5/10/15/25/30/45/60-minute preset, or a custom duration from 1 to 480 minutes.
- Start, pause, resume, or reset a quest countdown, with at most one timer running at a time.
- Hear a Windows alarm and receive a native notification when time runs out while Daily Quest is open, compact, or minimized; optionally continue into red overtime after silencing a repeating alarm.
- See today's greeting, completion count, percentage, and progress bar at a glance.
- Treat every active quest as a Daily Quest: keep it for the next day and reset its completion automatically.
- Move a completed quest to the bottom automatically, keeping unfinished priorities at the top.
- Review previous days in History; select a date card to reveal its activity details.
- Preserve completed entries in history after clearing them from today's active list.
- Keep a saved manual quest order, or sort unfinished quests by label, shortest duration, or longest duration; completed quests always stay at the bottom.
- Shrink the widget into a smaller top-right compact view that prioritizes the actively timed quest, then shows the next unfinished quest, with pin and expand controls always available.
- Start new installations in English, then choose English or Indonesian from Settings; existing saved preferences stay unchanged.
- Choose a Light or Dark theme, manage labels, and enable or disable overtime from Settings; every preference is saved locally.
- Pin the widget above other windows, minimize it to the taskbar, or move and resize the expanded window freely.
- Open at the top-right of the primary work area with a comfortable edge gap, while restoring the saved size, language, theme, and pin preference. Reset the expanded window to its 520 × 680 default from Settings when needed.
- Check local storage usage, clear history, open the Q&A, or report a bug from Settings.
- See the application identity and version in the main-view footer.
- Run a single instance entirely offline, with no account, telemetry, or network connection required.

## Download and install

1. Open the [latest release](https://github.com/samuelraindrwn/daily-quest/releases/latest).
2. Download the `win-x64.zip` asset (recommended) and extract it, or download the standalone `.exe` asset.
3. Run `DailyQuest.exe`.

The release is portable and self-contained for 64-bit Windows 10/11, so there is no installer and the .NET runtime does not need to be installed separately. To update, close Daily Quest and replace the old executable; your data in Local AppData remains intact.

> [!NOTE]
> The current executable is not code-signed, so Windows may show a SmartScreen warning. Continue only when the file came from this repository's official Releases page. You can verify it with the included `SHA256SUMS.txt`.

## How to use it

| Control | Action |
| --- | --- |
| Activity field | Type a new activity. Press `Enter` or select `+` to add it. |
| Date choices in the composer | Keep **Today** to add the quest immediately, or choose **Tomorrow** through **D+8**. |
| Timer choices in the composer | Keep **No timer**, choose 5, 10, 15, 25, 30, 45, or 60 minutes, or enter a custom duration from 1 to 480 minutes. |
| Label choice in the composer | Keep **No label** or assign one of your configured labels to the new quest. |
| Checkbox | Mark an activity as complete or incomplete. A completed quest automatically moves to the bottom. |
| Timer controls on a quest | Start or pause the countdown, resume a paused timer, or reset it to the quest's full duration. |
| **Overtime** on an expired quest | When overtime mode is enabled, silence the repeating alarm and continue counting upward in red until paused, reset, or completed. |
| Label on a quest | Change or remove the label assigned to an existing quest. |
| Sort control | Use the saved manual order, label order, shortest duration, or longest duration. |
| Drag handle beside an activity | In **Manual** sort mode, drag and drop the activity to change its saved order. |
| `×` beside an activity | Remove that activity from the active checklist. |
| **Clear done** | Remove completed activities from the active list while keeping their completed history. |
| **Reset** | Uncheck every activity for today. |
| **Today** | Return to the active checklist. |
| **Upcoming** | Review or cancel quests scheduled for a future date. |
| **History** | View daily completion summaries. Select a card to expand its details. |
| Settings button | Open language, theme, label, overtime, storage, history, Q&A, and bug-report options. |
| Compact button | Instantly shrink the widget at the top-right and show the actively timed quest, or the next unfinished quest when no timer is running. Complete it to advance to an unchecked next quest. |
| Pin button in compact mode | Keep the compact widget above other windows or return it to normal stacking. |
| Expand button | Return from compact mode to the full widget. |
| Pin button | Toggle always-on-top mode. |
| `−` button | Minimize the window to the Windows taskbar. This is separate from compact mode. |
| Header and window edges | Drag the header to move the widget, or drag an edge to resize it. The expanded size is saved after closing the app. |

Every active quest is a **Daily Quest**. When the date changes, Daily Quest archives the previous day, keeps the active quest list, and resets every checkbox for the new day. A quest continues to appear each day until you remove it.

### Timing a quest

- Choose **No timer** or a duration while adding the quest. Presets cover 5, 10, 15, 25, 30, 45, and 60 minutes; a custom timer accepts any whole number from 1 through 480 minutes.
- Use the quest's timer controls to start, pause, resume, or reset its countdown. Only one quest timer can run at a time.
- A running countdown is saved with a timestamp. If Daily Quest is closed and reopened, elapsed time is calculated from that timestamp instead of restarting the timer.
- Reaching zero does not mark the quest complete. Complete the quest separately with its checkbox.
- Overtime mode is off by default. With it off, an expired timer plays a finite alarm and shows no **Overtime** action.
- When overtime mode is enabled, an expired timer rings repeatedly and offers **Overtime**. Selecting it silences the alarm and starts a red count-up that continues until you pause or reset the timer, or complete the quest.
- Timer alarms and native Windows notifications work in the full view, compact mode, and while the window is minimized, as long as the Daily Quest process is running.
- Daily Quest does not run a background service, so it cannot play the alarm or show the notification while its process is fully closed.

### Labels and sorting

- Fresh installations and state migrated from a version before label support begin with **Important**, **Personal**, and **Routine**. They are starter labels, not permanent system labels: you can rename, recolor, reorder, or delete them, and a deliberately empty label list stays empty.
- You can keep up to 12 labels. Each name must be unique and no longer than 24 characters; colors use the `#RRGGBB` hexadecimal format.
- Label order controls **Label** sorting. Unlabeled unfinished quests appear after labeled unfinished quests.
- **Shortest** and **Longest** sort by the quest's configured timer duration. Untimed unfinished quests appear after timed unfinished quests in either duration mode.
- Completed quests always remain below unfinished quests in every sort mode.
- Drag and drop is available only in **Manual** mode. Automatic sorting does not overwrite the saved manual order, so switching back to **Manual** restores it.
- Deleting a label only detaches it from active, scheduled, and historical quests; it never deletes a quest.

### Scheduling future quests

- The composer offers **Today**, **Tomorrow**, and **D+2** through **D+8**, based on the local date reported by Windows.
- The progress summary hides while the date picker is open so the schedule remains visually clear.
- A future quest is stored in a separate upcoming queue. Before it is due, it does not affect Today's checklist, progress, compact mode, or History.
- When its date arrives, the quest is added to Today unchecked after the previous day has been archived. It then behaves like a regular active quest and follows the normal daily reset until you remove it.
- If Daily Quest was closed on the scheduled date, the overdue quest is activated the next time the app opens. It is activated only once.
- Open the upcoming list to review or cancel a scheduled quest before it becomes active.

Scheduling by itself does not create a Windows notification, run a background service, or launch Daily Quest automatically. Timer expiry notifications follow the behavior described above.

### Settings and support

- **Theme:** choose Light or Dark. The change applies immediately and is saved locally for the next launch.
- **Window size:** resize the expanded window from 390 × 500 up to 1200 × 1200. Select **Reset size** under Appearance to restore its 520 × 680 default.
- **Language:** choose Indonesian or English explicitly. The choice is saved locally.
- **Labels:** add, rename, recolor, reorder, or delete up to 12 quest labels. Label names and colors are saved locally.
- **Overtime:** enable repeating expiry alarms and the **Overtime** action, or leave the default off for a finite alarm without overtime.
- **Storage:** view the size of the application executable, Daily Quest's local data folder, and the serialized history data. These values are local estimates and may be rounded in the interface.
- **Clear history:** permanently removes archived history while leaving active and scheduled quests intact. A new current-day history entry can be created after the checklist changes again.
- **Q&A:** open the bilingual [Frequently Asked Questions](docs/FAQ.md).
- **Report a bug:** open the repository's pre-filled GitHub issue form in the default browser.

The footer on the full Today view shows the app name and installed version (v1.5.0 for this release).

### Keyboard navigation

| Key | Action |
| --- | --- |
| `Enter` | Add the current activity while the input is focused. |
| `Esc` | Clear the current input without adding it. |
| `Tab` / `Shift+Tab` | Move focus forward or backward. |
| `Space` | Activate the focused checkbox or button. |

Daily Quest does not register global keyboard shortcuts.

## Data and privacy

All data stays on the device in:

```text
%LOCALAPPDATA%\DailyQuest\state.json
```

The state file contains active and scheduled quest text and order, label definitions and assignments, the selected sort mode and saved manual order, timer durations and countdown or overtime state, running-timer timestamps, scheduled target dates, daily history, theme, language, overtime preference, always-on-top preference, and window size. Daily Quest does not require an account, include telemetry, or upload this data anywhere. Compact mode does not replace the native minimize action and is not stored as a separate checklist state.

The storage panel reads file sizes from the local application and data locations. **Saved data** includes the future-quest queue, while **History** estimates only serialized history records. **Clear history** removes history records only; it does not remove active or scheduled quests. The current day's history may be generated again after a later checklist change.

Daily Quest's core checklist works without a network connection. Selecting **Q&A** or **Report a bug** opens a GitHub page in your default browser; that optional action requires internet access and is then subject to GitHub's privacy practices.

For a manual backup, close the app and copy the `%LOCALAPPDATA%\DailyQuest` folder. To start over without immediately deleting data, close the app and rename that folder; a clean one will be created on the next launch.

If a state file cannot be read, Daily Quest keeps a timestamped `state.json.broken-*` copy before starting with a clean state.

### Upgrading from Morning Check-in

If `%LOCALAPPDATA%\DailyQuest\state.json` does not exist, Daily Quest validates and copies the former `%LOCALAPPDATA%\MorningCheckIn\state.json` file to the new location. The original legacy file remains untouched as a rollback copy. If both files already exist, the Daily Quest state takes priority.

## Build from source

Requirements:

- Windows 10 or Windows 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git

```powershell
git clone https://github.com/samuelraindrwn/daily-quest.git
cd daily-quest
dotnet restore .\DailyQuest.csproj
dotnet build .\DailyQuest.csproj -c Release
dotnet run --project .\DailyQuest.csproj
```

## Run the tests

```powershell
dotnet run --project .\tests\DailyQuest.LogicTests\DailyQuest.LogicTests.csproj -c Release
```

The dependency-free test harness covers checklist mutations, labels and their migrations, manual and automatic sort modes, completed-item ordering, timer duration validation and persistence, start/pause/resume/reset and overtime behavior, the single-running-timer rule, timestamp-based countdown recovery and expiry alarms, future-date validation and scheduling, due and overdue activation, compact-mode selection logic, daily quest rollover, history retention and clearing, persisted settings, storage reporting, JSON persistence, corrupt-state recovery, and legacy-state migration.

## Create a portable build

```powershell
dotnet publish .\DailyQuest.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o .\artifacts\DailyQuest-win-x64
```

The resulting portable application is written to `artifacts\DailyQuest-win-x64\DailyQuest.exe`.

## Project structure

```text
Assets/          App icons and logo
Infrastructure/ Command helpers
Localization/   English and Indonesian UI copy
Models/          Persisted active, scheduled, and historical quest data
Services/        JSON state storage, migration, usage reporting, and Windows timer alarms
ViewModels/      Checklist, labels, sorting, timer/overtime, scheduling, progress, settings, language, and history logic
tests/           Dependency-free logic test runner
```

## Troubleshooting

- **Windows shows “unknown publisher”:** the app is not code-signed yet. Use only the official release and verify its SHA-256 checksum.
- **Opening the app again does not create another window:** Daily Quest allows one instance and restores the existing window instead.
- **The compact widget is not in the taskbar:** compact mode keeps a small quest window visible. Use the `−` button when you want the native Windows minimize behavior.
- **A timer expired without an alarm while the app was closed:** the countdown is restored from its saved timestamp at the next launch, but Daily Quest cannot play a sound or deliver a notification while its process is not running.
- **There is no Overtime button:** enable overtime mode in Settings before the timer expires. With overtime disabled, expiry intentionally uses a finite alarm and does not offer overtime.
- **Cleared history returns for today:** the active checklist is intentionally preserved, so the current-day summary can be written again after a quest changes. Clear history after finishing changes if you want the History view to stay empty for the moment.
- **The source project reports a missing SDK:** install the .NET 10 SDK, then confirm it appears in `dotnet --list-sdks`.
- **The app starts with a clean state unexpectedly:** check the data folder for a `state.json.broken-*` backup created from unreadable JSON.
- **Legacy migration fails:** the old Morning Check-in state remains untouched, and the failed copy is retained as `state.json.migration-broken-*` in the Daily Quest data folder.

## Contributing

Bug reports and focused pull requests are welcome. Use the in-app **Report a bug** shortcut or [open a bug report](https://github.com/samuelraindrwn/daily-quest/issues/new?template=bug_report.yml) with clear reproduction steps, and run the test command above before submitting code changes. For usage questions, check the [bilingual Q&A](docs/FAQ.md) first.
