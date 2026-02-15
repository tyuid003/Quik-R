#define MyAppName "Quik-R"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Quik-R"
#define MyAppExeName "QuickReply.exe"
#define MyAppId "2A3A08F1-4A8A-4E56-8F87-4C1D147A81C1"

[Setup]
AppId={{{#MyAppId}}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\Quik-R
DefaultGroupName=Quik-R
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
OutputDir=..\dist
OutputBaseFilename=Quik-R-Setup
SetupIconFile=..\Assets\app.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"; Flags: unchecked

[Files]
Source: "..\bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Quik-R"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\Quik-R"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch Quik-R"; Flags: nowait postinstall skipifsilent
