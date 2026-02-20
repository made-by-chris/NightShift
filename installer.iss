[Setup]
AppName=NightShift
AppVersion=1.1.0
AppPublisher=made-by-chris
AppPublisherURL=https://github.com/made-by-chris/NightShift
DefaultDirName={autopf}\NightShift
DefaultGroupName=NightShift
OutputBaseFilename=NightShift-Setup
OutputDir=build
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=lowest
SetupIconFile=nightshift.ico
UninstallDisplayIcon={app}\nightshift.ico
WizardStyle=modern

[Files]
Source: "build\NightShift.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "nightshift.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\NightShift"; Filename: "{app}\NightShift.exe"; IconFilename: "{app}\nightshift.ico"
Name: "{autodesktop}\NightShift"; Filename: "{app}\NightShift.exe"; IconFilename: "{app}\nightshift.ico"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"
Name: "startup"; Description: "Run NightShift on Windows startup"; GroupDescription: "Startup:"

[Registry]
Root: HKCU; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "NightShift"; ValueData: """{app}\NightShift.exe"""; Flags: uninsdeletevalue; Tasks: startup

[Run]
Filename: "{app}\NightShift.exe"; Description: "Launch NightShift"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: files; Name: "{app}\*"

[UninstallRun]
; Clean up registry run key on uninstall
Filename: "reg.exe"; Parameters: "delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"" /v NightShift /f"; Flags: runhidden; RunOnceId: "CleanupRunKey"
