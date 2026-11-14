; Build with build.ps1 (publishes the app to installer\publish, then runs ISCC.exe on this file).
#define AppName "PDF Reader"
#ifndef AppVersion
  #define AppVersion "1.2.0" ; normally passed by build.ps1 from PdfReader.csproj
#endif
#define AppExe "PdfReader.exe"

[Setup]
AppId={{00DBCAEF-E6BD-40BA-883B-36916F7E94C1}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=PDF Reader
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=output
OutputBaseFilename=PDFReader-Setup-{#AppVersion}
SetupIconFile=..\Assets\AppIcon.ico
UninstallDisplayIcon={app}\{#AppExe}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
ChangesAssociations=yes

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked
Name: "pdfassoc"; Description: "Show {#AppName} in the ""Open with"" list for .pdf files"; GroupDescription: "File associations:"

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Registry]
Root: HKA; Subkey: "Software\Classes\.pdf\OpenWithProgids"; ValueType: string; ValueName: "PdfReader.pdf"; ValueData: ""; Flags: uninsdeletevalue; Tasks: pdfassoc
Root: HKA; Subkey: "Software\Classes\PdfReader.pdf"; ValueType: string; ValueData: "PDF Document"; Flags: uninsdeletekey; Tasks: pdfassoc
Root: HKA; Subkey: "Software\Classes\PdfReader.pdf\DefaultIcon"; ValueType: string; ValueData: "{app}\{#AppExe},0"; Tasks: pdfassoc
Root: HKA; Subkey: "Software\Classes\PdfReader.pdf\shell\open\command"; ValueType: string; ValueData: """{app}\{#AppExe}"" ""%1"""; Tasks: pdfassoc

[Run]
Filename: "{app}\{#AppExe}"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent
Filename: "{app}\{#AppExe}"; Flags: nowait; Check: LaunchAfterSilentInstall

[Code]
// The in-app updater runs this installer silently with /LAUNCH=1 so the app reopens after updating.
function LaunchAfterSilentInstall: Boolean;
begin
  Result := WizardSilent and (ExpandConstant('{param:launch|0}') = '1');
end;

// The Restart Manager cannot close this app: it found running copies but could not shut them down, and in a
// silent update the suppressed "close the applications" prompt defaults to Abort, so the update rolled back.
// So Setup closes the app itself before installing:
//  - a silent install (the in-app updater) asks every copy running from the install folder to close, as clicking
//    X would (open tabs and the window state are saved), and ends whatever is still running after 10 seconds;
//  - an interactive install only ends copies without a window, which are stuck while exiting and would keep the
//    files locked; copies with a window are left to the usual "close the applications" page.
function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  Script, Params: String;
  ResultCode: Integer;
begin
  Script :=
    'param([string]$Dir, [switch]$CloseAll)' + #13#10 +
    '$Dir = $Dir.TrimEnd(''\'') + ''\''' + #13#10 +
    'function Running { Get-Process PdfReader -ErrorAction SilentlyContinue | Where-Object {' + #13#10 +
    '  $_.Path -and $_.Path.StartsWith($Dir, [StringComparison]::OrdinalIgnoreCase) -and ($CloseAll -or $_.MainWindowHandle -eq 0) } }' + #13#10 +
    'Running | Where-Object { $_.MainWindowHandle -ne 0 } | ForEach-Object { Write-Output "Asking PdfReader $($_.Id) to close"; [void]$_.CloseMainWindow() }' + #13#10 +
    '$deadline = (Get-Date).AddSeconds(10)' + #13#10 +
    'while ((Running) -and (Get-Date) -lt $deadline) { Start-Sleep -Milliseconds 250 }' + #13#10 +
    'Running | ForEach-Object { Write-Output "Ending PdfReader $($_.Id)"; Stop-Process -Id $_.Id -Force }' + #13#10;
  if SaveStringToFile(ExpandConstant('{tmp}\close-app.ps1'), Script, False) then
  begin
    Params := '-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "' + ExpandConstant('{tmp}\close-app.ps1') +
      '" -Dir "' + ExpandConstant('{app}') + '"';
    if WizardSilent then Params := Params + ' -CloseAll';
    Log('Closing running copies of the app in ' + ExpandConstant('{app}') + ' (silent: ' + IntToStr(Ord(WizardSilent)) + ')');
    if not Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'), Params, '', SW_HIDE,
      ewWaitUntilTerminated, ResultCode) then
      Log('Could not run the close script');
  end;
  Result := '';
end;
