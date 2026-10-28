#define AppVersion "1.5.9"
#define Root AddBackslash(SourcePath) + ".."

[Setup]
AppId={{A5940783-1830-4CCA-A2D1-7D26198A0C9B}
AppName=译窗
AppVersion={#AppVersion}
AppVerName=译窗 {#AppVersion}
VersionInfoVersion={#AppVersion}.0
VersionInfoDescription=译窗安装程序
DefaultDirName={localappdata}\Programs\YiChuang
DefaultGroupName=译窗
UninstallDisplayIcon={app}\App\YiChuang.exe
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.19045
OutputDir={#Root}\Packages
OutputBaseFilename=YiChuang-Setup-{#AppVersion}
SetupIconFile={#Root}\Source\Main\app.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
DisableProgramGroupPage=yes
AllowNoIcons=yes
CloseApplications=yes
CloseApplicationsFilter=YiChuang.exe,wxocr.exe,YiChuang-*.exe
RestartApplications=no
Uninstallable=yes
SetupLogging=yes

[Languages]
Name: "chinesesimp"; MessagesFile: "ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "快捷方式："

[Files]
Source: "{#Root}\App\*"; DestDir: "{app}\App"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#Root}\其他电脑试用说明.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Root}\{#AppVersion}更新与试用说明.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Root}\Build\Prerequisites\dotnet48.exe"; Flags: dontcopy
Source: "{#Root}\Build\Prerequisites\vc_redist.x64.exe"; Flags: dontcopy

[Icons]
Name: "{autoprograms}\译窗"; Filename: "{app}\App\YiChuang.exe"; WorkingDir: "{app}\App"
Name: "{autodesktop}\译窗"; Filename: "{app}\App\YiChuang.exe"; WorkingDir: "{app}\App"; Tasks: desktopicon

[Run]
Filename: "{app}\App\YiChuang.exe"; WorkingDir: "{app}\App"; Description: "启动译窗"; Flags: nowait postinstall skipifsilent

[Code]
function HasDotNet48: Boolean;
var Release: Cardinal;
begin
  Result := (RegQueryDWordValue(HKLM32, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release) and (Release >= 528040)) or
            (RegQueryDWordValue(HKLM64, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release) and (Release >= 528040));
end;

function HasVisualCpp: Boolean;
var Installed, Major, Minor: Cardinal;
begin
  Result := RegQueryDWordValue(HKLM64, 'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64', 'Installed', Installed) and (Installed = 1) and
            RegQueryDWordValue(HKLM64, 'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64', 'Major', Major) and
            RegQueryDWordValue(HKLM64, 'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64', 'Minor', Minor) and ((Major > 14) or ((Major = 14) and (Minor >= 29)));
end;

function InstallRuntime(FileName, Arguments, DisplayName: String; var NeedsRestart: Boolean): String;
var Code: Integer;
begin
  Result := '';
  Log('Installing prerequisite: ' + DisplayName);
  ExtractTemporaryFile(FileName);
  if not ShellExec('runas', ExpandConstant('{tmp}\') + FileName, Arguments, '', SW_SHOW, ewWaitUntilTerminated, Code) then
  begin
    Result := DisplayName + ' 尚未安装。请允许微软运行环境安装程序的管理员授权后重试。系统错误：' + IntToStr(Code);
    Exit;
  end;
  if (Code = 3010) or (Code = 1641) then
  begin
    NeedsRestart := True;
    Result := DisplayName + ' 已安装，需要先重启电脑。重启后再次运行译窗安装包即可继续。';
  end
  else if (Code <> 0) and (Code <> 1638) then
    Result := DisplayName + ' 安装未完成，错误码：' + IntToStr(Code) + '。请完成系统更新或重启电脑后重试。';
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';
  Log('Prerequisite .NET 4.8 present: ' + IntToStr(Ord(HasDotNet48)));
  Log('Prerequisite Visual C++ x64 present: ' + IntToStr(Ord(HasVisualCpp)));
  if not HasDotNet48 then
  begin
    Result := InstallRuntime('dotnet48.exe', '/passive /norestart /showfinalerror', '.NET Framework 4.8', NeedsRestart);
    if Result <> '' then Exit;
    if not HasDotNet48 then begin Result := '.NET Framework 4.8 安装后仍未就绪，请重启后重新安装译窗。'; Exit; end;
  end;
  if not HasVisualCpp then
  begin
    Result := InstallRuntime('vc_redist.x64.exe', '/install /passive /norestart', 'Microsoft Visual C++ x64', NeedsRestart);
    if Result <> '' then Exit;
    if not HasVisualCpp then begin Result := 'OCR 运行环境安装后仍未就绪，请重启后重新安装译窗。'; Exit; end;
  end;
end;

procedure InitializeWizard;
begin
  WizardForm.WelcomeLabel2.Caption := '安装译窗及中英文离线 OCR。缺少微软运行环境时，安装器将自动打开内置的官方安装程序，可能需要管理员授权或重启。' + #13#10 + #13#10 +
    '翻译模型不包含在安装包内。打开译窗即可使用网页翻译；需要本地模型时，在设置中自行添加。';
end;
