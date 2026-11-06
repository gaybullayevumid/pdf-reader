; Build with build.ps1 (publishes the app to installer\publish, then runs ISCC.exe on this file).
#define AppName "PDF Reader"
#define AppVersion "1.1.0"
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
