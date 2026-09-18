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

This guide covers Daily Quest v1.8.0.

<p align="center">
  <img src="docs/images/daily-quest-en.png" width="360" alt="Daily Quest main window in English showing a running quest timer">
</p>

## Highlights

- Starts with an empty checklist—your routine stays yours.
- Add, complete, and remove activities in a few clicks.
- Add a quest to Today immediately, or schedule it for any date from Tomorrow through D+8.
- Right-click an active quest to edit its text and timer, copy it to one date through D+8, or copy it to every future date from Tomorrow through D+8 at once.
- Right-click any date in **Schedule for** to copy every quest from that date to one destination, or to all future dates except Today.
- Assign a customizable color label when adding a quest, or change it later from the quest card.
- Give a new quest no timer, a 5/10/15/25/30/45/60-minute preset, or a custom duration from 1 to 480 minutes.
- Start, pause, resume, or reset a quest countdown, with at most one timer running at a time.
- Close the window with `X` or `Alt+F4` to hide Daily Quest in the system tray while active timers and alarms keep running.
- Reopen the hidden window from the tray, or choose **Exit** there to pause the active countdown or overtime timer and save its latest value before the process closes.
- In the official Windows release, hear the bundled facility-alarm ringtone for up to one minute and receive one native notification when time runs out while Daily Quest is visible, compact, minimized, or hidden in the tray; optionally continue into red overtime after silencing it.
- See today's greeting, completion count, percentage, and progress bar at a glance.
- Carry every unfinished quest into tomorrow automatically as the same active quest—never as a duplicate—and reset its completion for the new day.
- Move a completed quest to the bottom automatically, keeping unfinished priorities at the top.
- Review previous days in History; select a date card to reveal its activity details.
- Preserve completed entries in history after clearing them from today's active list.
- Keep a saved manual quest order, or sort unfinished quests by label, shortest duration, or longest duration; completed quests always stay at the bottom.
- Shrink the widget into a smaller top-right compact view that prioritizes the actively timed quest, then shows the next unfinished quest, with pin and expand controls always available.
- Start new installations in English, then choose English or Indonesian from Settings; existing saved preferences stay unchanged.
- Choose a Light or Dark theme, manage labels, enable or disable overtime, and control Windows startup from Settings; every preference is saved locally.
- Launch automatically when you sign in to Windows by default, with an explicit Off/On setting.
- Pin the widget above other windows, minimize it to the taskbar, or move and resize the expanded window freely.
- Open at the top-right of the primary work area with a comfortable edge gap, while restoring the saved size, language, theme, and pin preference. Reset the expanded window to its 520 × 680 default from Settings when needed.
- Check local storage usage, clear history, open the Q&A, or report a bug from Settings.
- See the application identity and version in the main-view footer.
- Run a single instance entirely offline, with no account, telemetry, or network connection required.

## Download and install

1. Open the [latest release](https://github.com/samuelraindrwn/daily-quest/releases/latest).
2. Download and run the `win-x64-setup.exe` asset for the standard Windows install experience.
3. For portable use, download and extract `win-x64.zip`, or download the standalone `win-x64.exe` asset.

All release options are self-contained for 64-bit Windows 10/11, so the .NET runtime does not need to be installed separately. The installer adds Start Menu and uninstall entries, offers an optional Desktop shortcut, and upgrades in place. Daily Quest enables launch-at-sign-in after its first run; it can be disabled in Settings. Portable users can update by right-clicking the tray icon, choosing **Exit**, and then replacing the old executable. Either method preserves data in Local AppData.

> [!NOTE]
> The installer and application are not code-signed yet, so Windows may show a SmartScreen warning. Continue only when the file came from this repository's official Releases page. You can verify it with the included `SHA256SUMS.txt`.

## How to use it

| Control | Action |
| --- | --- |
| Activity field | Type a new activity. Press `Enter` or select `+` to add it. |
| Date choices in the composer | Keep **Today** to add the quest immediately, or choose **Tomorrow** through **D+8**. |
| Timer choices in the composer | Keep **No timer**, choose 5, 10, 15, 25, 30, 45, or 60 minutes, or enter a custom duration from 1 to 480 minutes. |
| Label choice in the composer | Keep **No label** or assign one of your configured labels to the new quest. |
| Checkbox | Mark an activity as complete or incomplete. A completed quest automatically moves to the bottom. |
| Timer controls on a quest | Start or pause the countdown, resume a paused timer, or reset it to the quest's full duration. |
| **Overtime** on an expired quest | When overtime mode is enabled, silence the alarm immediately and continue a cumulative red count-up from the configured duration until paused, reset, or completed. |
| Label on a quest | Change or remove the label assigned to an existing quest. |
| Right-click an active quest | Choose **Edit quest** to change its text and add, change, or remove its timer; changing the duration resets that timer. Or open **Copy to** and choose one date from **Today** through **D+8**, or **All upcoming days** to create one fresh copy on every date from **Tomorrow** through **D+8**. |
| Right-click a date in **Schedule for** | Open **Copy all quests to**, then choose one destination from **Today** through **D+8**, or **All upcoming days** to copy the source snapshot to every date from **Tomorrow** through **D+8**. This works on every date in the picker, not only Today. |
| Sort control | Use the saved manual order, label order, shortest duration, or longest duration. |
| Drag handle beside an activity | In **Manual** sort mode, drag and drop the activity to change its saved order. |
| `×` beside an activity | Remove that activity from the active checklist. |
| **Clear done** | Remove completed activities from the active list while keeping their completed history. |
| **Reset** | Uncheck every activity for today. |
| **Today** | Return to the active checklist. |
| **Upcoming** | Review or cancel quests scheduled for a future date. |
| **History** | View daily completion summaries. Select a card to expand its details. |
| Settings button | Open language, Windows startup, theme, label, overtime, storage, history, Q&A, and bug-report options. |
| Compact button | Instantly shrink the widget at the top-right and show the actively timed quest, or the next unfinished quest when no timer is running. Complete it to advance to an unchecked next quest. |
| Pin button in compact mode | Keep the compact widget above other windows or return it to normal stacking. |
| Expand button | Return from compact mode to the full widget. |
| Pin button | Toggle always-on-top mode. |
| `−` button | Minimize the window to the Windows taskbar. This is separate from compact mode. |
| `X` button or `Alt+F4` | Hide the window in the system tray without stopping a running timer or its alarm. |
| Right-click the tray icon | Choose **Open Daily Quest** to restore the window, or **Exit** to pause timers, save state, and fully close the process. |
| Header and window edges | Drag the header to move the widget, or drag an edge to resize it. The expanded size is saved after closing the app. |

Every active quest is a **Daily Quest**. When the date changes, Daily Quest archives the previous day, keeps the active quest list, and resets every checkbox for the new day. An unfinished quest therefore moves into tomorrow as the same quest, not a second scheduled copy, so rollover cannot duplicate it. Completed active quests also reset for the new day unless you clear or remove them. A quest continues to appear each day until you remove it.

### Timing a quest

- Choose **No timer** or a duration while adding the quest. Presets cover 5, 10, 15, 25, 30, 45, and 60 minutes; a custom timer accepts any whole number from 1 through 480 minutes.
- To update an existing quest, right-click it and choose **Edit quest**. Enter 1-480 minutes to add or change its timer, or leave the timer field blank to remove it. A changed duration starts again at its full value in a paused state.
- Use the quest's timer controls to start, pause, resume, or reset its countdown. Only one quest timer can run at a time.
- Selecting `X` or pressing `Alt+F4` hides the window in the system tray; the Daily Quest process stays active, so a running countdown or overtime timer continues normally.
- Selecting **Exit** from the tray advances an active timer to that moment, pauses it, saves the result, and then closes the process. The saved timer remains paused the next time Daily Quest starts.
- Reaching zero does not mark the quest complete. Complete the quest separately with its checkbox.
- Overtime mode is off by default. With it off, an expired timer plays the same one-minute-maximum alarm and shows no **Overtime** action.
- In the official Windows release, every expired timer loops the bundled ringtone for up to 60 seconds and shows one native notification. Source builds without the optional ringtone asset use alternating Windows system sounds instead. The sound stops sooner when the quest is reset, completed, or deleted; when overtime is turned off in Settings; at daily rollover; or when Daily Quest exits.
- When overtime mode is enabled, an expired timer offers **Overtime**. Selecting it silences the alarm immediately and starts a red cumulative count-up from the configured duration—a one-minute timer begins overtime at `+01:00`—until you pause or reset the timer, or complete the quest.
- Timer alarms and native Windows notifications work in the full view, compact mode, while the window is minimized, and while it is hidden in the system tray, as long as the Daily Quest process is running.
- Daily Quest does not run a separate Windows service. After **Exit** or a forced process termination, no live process remains to play the alarm or show a timer notification; a forcibly terminated app cannot notify again until it is launched.

### Labels and sorting

- Fresh installations and state migrated from a version before label support begin with **Important**, **Personal**, and **Routine**. They are starter labels, not permanent system labels: you can rename, recolor, drag them into a new priority order, or delete them, and a deliberately empty label list stays empty.
- You can keep up to 12 labels. Each name must be unique and no longer than 24 characters; colors use the `#RRGGBB` hexadecimal format.
- Label order controls **Label** sorting. Unlabeled unfinished quests appear after labeled unfinished quests.
- In Settings, drag a label by its six-dot handle to change that priority order. Name and color drafts stay in place while labels move.
- **Shortest** and **Longest** sort by the quest's configured timer duration. Untimed unfinished quests appear after timed unfinished quests in either duration mode.
- Completed quests always remain below unfinished quests in every sort mode.
- Drag and drop is available only in **Manual** mode. Automatic sorting does not overwrite the saved manual order, so switching back to **Manual** restores it.
- Deleting a label only detaches it from active, scheduled, and historical quests; it never deletes a quest.

### Scheduling future quests

- The composer offers **Today**, **Tomorrow**, and **D+2** through **D+8**, based on the local date reported by Windows.
- The progress summary hides while the date picker is open so the schedule remains visually clear.
- To reuse an active quest, right-click its card, open **Copy to**, and choose **Today** through **D+8**. Choose **All upcoming days** to create one copy on each date from **Tomorrow** through **D+8** while leaving Today unchanged. The copy keeps the quest text, label, and configured duration, but is created as a fresh unchecked quest with its timer reset and idle. Copies for a future date appear in **Upcoming** until they are due. Repeating either copy action appends another set of fresh copies; it does not replace or merge existing quests.
- To reuse a whole day, open **Schedule for**, right-click any date tile, open **Copy all quests to**, and choose any destination from **Today** through **D+8**. Choose **All upcoming days** to copy that day's complete snapshot to every date from **Tomorrow** through **D+8**, with Today excluded. Today as a source uses every quest in the currently visible active list, including completed quests; a future source date uses every quest explicitly scheduled for that exact date.
- Bulk copying leaves the source unchanged. Every copy is fresh and unchecked, with its timer reset and idle. An empty source does nothing. Choosing the same source and destination duplicates the source snapshot exactly once. With **All upcoming days**, a future source date is also one of the destinations, so it receives one fresh copy of its starting snapshot; that snapshot is captured once, which prevents newly created copies from cascading into later destinations. Repeating the action appends another fresh set to all eight future dates.
- A future quest is stored in a separate upcoming queue. Before it is due, it does not affect Today's checklist, progress, compact mode, or History.
- When its date arrives, the quest is added to Today unchecked after the previous day has been archived. It then behaves like a regular active quest and follows the normal daily reset until you remove it.
- If the Daily Quest process was not running on the scheduled date, the overdue quest is activated the next time the app opens. It is activated only once.
- Open the upcoming list to review or cancel a scheduled quest before it becomes active.

Scheduling by itself does not create a Windows notification or run a separate background service. Daily Quest can still launch at Windows sign-in through the default-on startup preference described below. Timer expiry notifications follow the behavior described above.

### Settings and support

- **Theme:** choose Light or Dark. The change applies immediately and is saved locally for the next launch.
- **Window size:** resize the expanded window from 390 × 500 up to 1200 × 1200. Select **Reset size** under Appearance to restore its 520 × 680 default.
- **Language:** choose Indonesian or English explicitly. The choice is saved locally.
- **Launch at startup:** enabled by default. Choose Off to remove Daily Quest from the current user's Windows startup list, or On to register the executable at its current location.
- **Labels:** add, rename, recolor, drag to reorder, or delete up to 12 quest labels. Label names, colors, and priority order are saved locally.
- **Overtime:** enable the **Overtime** action for expired timers, or leave the default off. The one-minute-maximum expiry alarm works in either mode.
- **Storage:** view the size of the application executable, Daily Quest's local data folder, and the serialized history data. These values are local estimates and may be rounded in the interface.
- **Clear history:** permanently removes archived history while leaving active and scheduled quests intact. A new current-day history entry can be created after the checklist changes again.
- **Q&A:** open the bilingual [Frequently Asked Questions](docs/FAQ.md).
- **Report a bug:** open the repository's pre-filled GitHub issue form in the default browser.

The footer on the full Today view shows the app name and installed version (v1.8.0 for this release).

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

The state file contains active and scheduled quest text and order, label definitions and assignments, the selected sort mode and saved manual order, timer durations and countdown or overtime state, running-timer timestamps, scheduled target dates, daily history, theme, language, startup, overtime, always-on-top preferences, and window size. Daily Quest does not require an account, include telemetry, or upload this data anywhere. Compact mode does not replace the native minimize action and is not stored as a separate checklist state.

When **Launch at startup** is on, Daily Quest stores one command for the current user under `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`. It contains only the quoted local executable path and the `--startup` marker. Moving a portable executable requires one manual launch from its new location so this path can be refreshed. Turning the setting off removes only Daily Quest's entry; uninstalling the installed copy also removes its matching entry without deleting quest data.

The storage panel reads file sizes from the local application and data locations. **Saved data** includes the future-quest queue, while **History** estimates only serialized history records. **Clear history** removes history records only; it does not remove active or scheduled quests. The current day's history may be generated again after a later checklist change.

Daily Quest's core checklist works without a network connection. Selecting **Q&A** or **Report a bug** opens a GitHub page in your default browser; that optional action requires internet access and is then subject to GitHub's privacy practices.

For a manual backup, right-click the Daily Quest tray icon and choose **Exit** before copying the `%LOCALAPPDATA%\DailyQuest` folder. To start over without immediately deleting data, use tray **Exit** first and then rename that folder; a clean one will be created on the next launch. Closing the window with `X` or `Alt+F4` is not enough because it only hides the still-running app.

If a state file cannot be read, Daily Quest keeps a timestamped `state.json.broken-*` copy before starting with a clean state.

### Upgrading from Morning Check-in

If `%LOCALAPPDATA%\DailyQuest\state.json` does not exist, Daily Quest validates and copies the former `%LOCALAPPDATA%\MorningCheckIn\state.json` file to the new location. The original legacy file remains untouched as a rollback copy. If both files already exist, the Daily Quest state takes priority.

## Build from source

Requirements:

- Windows 10 or Windows 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git

The official release embeds Mixkit's **Facility alarm sound** under the Mixkit Sound Effects Free License. Its raw WAV is intentionally excluded from this source repository. A regular source build remains fully functional and uses Windows system sounds as its timer fallback. To reproduce the official release audio, download the sound directly from Mixkit and save it as `Assets\ringtone\mixkit-facility-alarm-sound-999.wav` before building. See [Third-Party Notices](THIRD_PARTY_NOTICES.md).

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

The dependency-free logic test harness covers checklist mutations and text/timer editing, labels and their migrations, manual and automatic sort modes, completed-item ordering, timer duration validation and persistence, start/pause/resume/reset and overtime behavior, graceful-shutdown timer pausing, the single-running-timer rule, timestamp-based crash recovery and expiry alarms, Windows startup registration, future-date validation and scheduling, due and overdue activation, compact-mode selection logic, duplicate-free daily quest rollover, history retention and clearing, persisted settings, storage reporting, JSON persistence, corrupt-state recovery, and legacy-state migration.

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

## Create a Windows installer

The reproducible Inno Setup configuration creates a per-user installer with upgrade and uninstall support while preserving local quest data:

```powershell
winget install --id JRSoftware.InnoSetup --exact --scope user
.\installer\build-installer.ps1
```

See the [Windows installer guide](installer/README.md) for prerequisites, output paths, and packaging an existing official executable.

## Project structure

```text
Assets/          App icons, logo, and optional local timer-ringtone instructions
Infrastructure/ Command helpers
installer/      Reproducible per-user Windows installer scripts and documentation
Localization/   English and Indonesian UI copy
Models/          Persisted active, scheduled, and historical quest data
Services/        JSON state storage, migration, usage reporting, and Windows ringtone alarms
ViewModels/      Checklist, labels, sorting, timer/overtime, scheduling, progress, settings, language, and history logic
tests/           Dependency-free logic test runner
```

## Troubleshooting

- **Windows shows “unknown publisher”:** the app is not code-signed yet. Use only the official release and verify its SHA-256 checksum.
- **Opening the app again does not create another window:** Daily Quest allows one instance and restores the existing window instead.
- **The compact widget is not in the taskbar:** compact mode keeps a small quest window visible. Use the `−` button when you want the native Windows minimize behavior.
- **The window closed but Daily Quest is still running:** `X` and `Alt+F4` hide the window in the system tray so active timers and alarms can continue. Right-click the tray icon and choose **Open Daily Quest** to restore it, or **Exit** to pause timers, save state, and fully close the process.
- **There is no Overtime button:** enable overtime mode in Settings before the timer expires. With overtime disabled, the alarm still runs for up to one minute, but the timer does not offer overtime.
- **Cleared history returns for today:** the active checklist is intentionally preserved, so the current-day summary can be written again after a quest changes. Clear history after finishing changes if you want the History view to stay empty for the moment.
- **The source project reports a missing SDK:** install the .NET 10 SDK, then confirm it appears in `dotnet --list-sdks`.
- **The app starts with a clean state unexpectedly:** check the data folder for a `state.json.broken-*` backup created from unreadable JSON.
- **Legacy migration fails:** the old Morning Check-in state remains untouched, and the failed copy is retained as `state.json.migration-broken-*` in the Daily Quest data folder.

## Contributing

Bug reports and focused pull requests are welcome. Use the in-app **Report a bug** shortcut or [open a bug report](https://github.com/samuelraindrwn/daily-quest/issues/new?template=bug_report.yml) with clear reproduction steps, and run the test command above before submitting code changes. For usage questions, check the [bilingual Q&A](docs/FAQ.md) first.
