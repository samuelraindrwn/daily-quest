#define AppName "Daily Quest"
#define AppExeName "DailyQuest.exe"
#define AppPublisher "Samuel Rayy"
#define AppUrl "https://github.com/samuelraindrwn/daily-quest"

#ifndef AppVersion
  #error AppVersion must be supplied by build-installer.ps1
#endif

#ifndef SourceExe
  #error SourceExe must be supplied by build-installer.ps1
#endif

[Setup]
AppId={{9F5BE714-FBF2-4A86-BD6F-A563ECF7BDA3}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppUrl}
AppSupportURL={#AppUrl}/issues
AppUpdatesURL={#AppUrl}/releases/latest
AppCopyright=Copyright (C) 2026 {#AppPublisher}
DefaultDirName={localappdata}\Programs\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
AllowNoIcons=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
AppMutex=Local\MorningCheckIn.SingleInstance.8F915C25
CloseApplications=yes
RestartApplications=no
SetupIconFile=..\Assets\DailyQuest.ico
UninstallDisplayIcon={app}\{#AppExeName}
UninstallDisplayName={#AppName}
WizardStyle=modern
Compression=lzma2/max
SolidCompression=yes
SetupLogging=yes
UninstallLogging=yes
UsePreviousAppDir=yes
UsePreviousGroup=yes
UsePreviousLanguage=yes
UsePreviousTasks=yes
OutputDir=.
OutputBaseFilename=DailyQuest-v{#AppVersion}-win-x64-setup
VersionInfoCompany={#AppPublisher}
VersionInfoDescription={#AppName} Installer
VersionInfoProductName={#AppName}
VersionInfoProductTextVersion={#AppVersion}
VersionInfoProductVersion={#AppVersion}.0
VersionInfoTextVersion={#AppVersion}
VersionInfoVersion={#AppVersion}.0

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceExe}"; DestDir: "{app}"; DestName: "{#AppExeName}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"; WorkingDir: "{app}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent

[Code]
const
  UninstallRegistryKey = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{9F5BE714-FBF2-4A86-BD6F-A563ECF7BDA3}_is1';
  StartupRegistryKey = 'Software\Microsoft\Windows\CurrentVersion\Run';
  StartupRegistryValue = 'DailyQuest';

function InitializeSetup: Boolean;
var
  InstalledVersionText: String;
  InstalledVersion: Int64;
  SetupVersion: Int64;
begin
  Result := True;

  if RegQueryStringValue(HKEY_CURRENT_USER, UninstallRegistryKey, 'DisplayVersion', InstalledVersionText) and
     StrToVersion(InstalledVersionText + '.0', InstalledVersion) and
     StrToVersion('{#AppVersion}.0', SetupVersion) and
     (ComparePackedVersion(InstalledVersion, SetupVersion) > 0) then
  begin
    SuppressibleMsgBox(
      'Daily Quest ' + InstalledVersionText + ' is already installed. ' +
      'This v{#AppVersion} installer cannot replace a newer version.',
      mbError,
      MB_OK,
      IDOK);
    Result := False;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  StartupCommand: String;
  InstalledExecutable: String;
  InstalledStartupCommand: String;
begin
  if CurUninstallStep <> usUninstall then
  begin
    Exit;
  end;

  InstalledExecutable := ExpandConstant('{app}\{#AppExeName}');
  InstalledStartupCommand := '"' + InstalledExecutable + '" --startup';
  if RegQueryStringValue(
       HKEY_CURRENT_USER,
       StartupRegistryKey,
       StartupRegistryValue,
       StartupCommand) and
     ((CompareText(Trim(StartupCommand), InstalledStartupCommand) = 0) or
      (CompareText(Trim(StartupCommand), '"' + InstalledExecutable + '"') = 0)) then
  begin
    RegDeleteValue(HKEY_CURRENT_USER, StartupRegistryKey, StartupRegistryValue);
  end;
end;
