param([string]$CompilerPath,[string]$EvidenceDirectory,[string]$AppDirectory,[string]$StartAt)
$ErrorActionPreference='Stop'
$packageRoot=$PSScriptRoot
if(-not $AppDirectory){$AppDirectory=Join-Path $packageRoot 'App'}
$AppDirectory=[IO.Path]::GetFullPath($AppDirectory)
if(-not $EvidenceDirectory){$EvidenceDirectory=Join-Path $packageRoot 'Evidence'}
$EvidenceDirectory=[IO.Path]::GetFullPath($EvidenceDirectory)
$running=Get-CimInstance Win32_Process -Filter "Name='YiChuang.exe'" | Where-Object {$_.ExecutablePath -eq (Join-Path $AppDirectory 'YiChuang.exe')}
if($running){throw '请先退出本目录的 YiChuang 再运行隔离测试，避免取词/OCR 管道冲突。'}
if (-not $CompilerPath) {
    $vswhere=Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if(Test-Path -LiteralPath $vswhere){$vs=& $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -property installationPath; if($vs){$CompilerPath=Join-Path $vs 'MSBuild\Current\Bin\Roslyn\csc.exe'}}
}
if (-not $CompilerPath -or -not (Test-Path -LiteralPath $CompilerPath)){throw 'Install Visual Studio 2022 Build Tools, or supply -CompilerPath with Roslyn csc.exe.'}
$compiler=$CompilerPath
$started=[string]::IsNullOrWhiteSpace($StartAt)
foreach($case in @(@('WindowActivationRegression','x86','YiChuang.exe'),@('NamingMigrationRegression','x86','YiChuang.exe'),@('NoLoginSmoke','x86','YiChuang.exe'),@('SlimUiRegression','x86','YiChuang.exe'),@('LayoutHotkeyRegression','x86','YiChuang.exe'),@('DeepLRegression','x86','YiChuang.exe'),@('FontPersistenceRegression','x86','YiChuang.exe'),@('LocalModelRegression','x86','YiChuang.exe'),@('SentenceHoverRegression','x86','YiChuang.exe'),@('CloudEngineRegression','x86','YiChuang.exe'),@('UiRepairRegression','x86','YiChuang.exe'),@('MouseCaptureRegression','x86','YiChuang.exe'),@('CodeAuditRegression','x86','YiChuang.exe'),@('EngineManagerRegression','x86','YiChuang.exe'),@('LanguageDirectionRegression','x86','YiChuang.exe'),@('OcrSmoke','x64','wxocr.exe'))){
    $name=$case[0]
    if(-not $started){if($name -ne $StartAt){continue};$started=$true}
    $dpiArguments=@();if($name -in @('UiRepairRegression','EngineManagerRegression','LanguageDirectionRegression')){$dpiArguments=@('/win32manifest:'+(Join-Path $packageRoot 'Source\Main\app.manifest'))}
    & $compiler @dpiArguments /nologo /target:exe ('/platform:'+$case[1]) ('/out:'+(Join-Path $packageRoot ('Tests\'+$name+'.exe'))) /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll (Join-Path $packageRoot ('Tests\'+$name+'.cs'))
    if($LASTEXITCODE -ne 0){throw ('Compile failed: '+$name)}
    & (Join-Path $packageRoot ('Tests\'+$name+'.exe')) (Join-Path $AppDirectory $case[2]) (Join-Path $EvidenceDirectory ('verification-'+$name))
    if($LASTEXITCODE -ne 0){throw ('Verification failed: '+$name)}
    if($name -eq 'FontPersistenceRegression') {
        $fontEvidence=Join-Path $EvidenceDirectory 'verification-FontPersistenceRegression'
        $fontData=[IO.File]::ReadAllText((Join-Path $fontEvidence 'restart-data-path.txt'))
        & (Join-Path $packageRoot 'Tests\FontPersistenceRegression.exe') (Join-Path $AppDirectory 'YiChuang.exe') (Join-Path $fontEvidence 'restart-probe') --read $fontData
        if($LASTEXITCODE -ne 0){throw 'Font persistence restart probe failed'}
    }
    if($name -eq 'LanguageDirectionRegression') {
        $directionEvidence=Join-Path $EvidenceDirectory 'verification-LanguageDirectionRegression'
        $directionData=[IO.File]::ReadAllText((Join-Path $directionEvidence 'restart-data-path.txt'))
        & (Join-Path $packageRoot 'Tests\LanguageDirectionRegression.exe') (Join-Path $AppDirectory 'YiChuang.exe') (Join-Path $directionEvidence 'restart-probe') --read $directionData
        if($LASTEXITCODE -ne 0){throw 'Language/visibility/order fresh-process persistence failed'}
    }
    if($name -eq 'EngineManagerRegression') {
        $engineEvidence=Join-Path $EvidenceDirectory 'verification-EngineManagerRegression'
        $engineData=[IO.File]::ReadAllText((Join-Path $engineEvidence 'restart-data-path.txt'))
        & (Join-Path $packageRoot 'Tests\EngineManagerRegression.exe') (Join-Path $AppDirectory 'YiChuang.exe') (Join-Path $engineEvidence 'restart-probe') --read $engineData
        if($LASTEXITCODE -ne 0){throw 'Engine manager fresh-process persistence failed'}
    }
    if($name -eq 'LayoutHotkeyRegression') {
        $layoutEvidence=Join-Path $EvidenceDirectory 'verification-LayoutHotkeyRegression'
        $layoutData=[IO.File]::ReadAllText((Join-Path $layoutEvidence 'restart-data-path.txt'))
        & (Join-Path $packageRoot 'Tests\LayoutHotkeyRegression.exe') (Join-Path $AppDirectory 'YiChuang.exe') (Join-Path $layoutEvidence 'restart-probe') --read $layoutData
        if($LASTEXITCODE -ne 0){throw 'Layout/shortcut fresh-process persistence failed'}
    }
}
foreach($case in @(@('LocalSetupRegression','x86','YiChuang.exe'),@('IndependentOcrRegression','x64','wxocr.exe'),@('LlamaRuntimeRegression','x86','YiChuang.exe'),@('CustomLocalModelsRegression','x86','YiChuang.exe'),@('LocalSimplifyRegression','x86','YiChuang.exe'),@('LlamaOfflinePreflightRegression','x86','YiChuang.exe'),@('SettingsActionsRegression','x86','YiChuang.exe'),@('FontDiscardRegression','x86','YiChuang.exe'),@('NetworkProxyRegression','x86','YiChuang.exe'),@('SettingsRootActionsRegression','x86','YiChuang.exe'),@('SettingsButtonLayoutRegression','x86','YiChuang.exe'),@('ParagraphCaptureRegression','x86','YiChuang.exe'))){
    $name=$case[0]
    if(-not $started){if($name -ne $StartAt){continue};$started=$true}
    & $compiler /nologo /target:exe ('/platform:'+$case[1]) ('/win32manifest:'+(Join-Path $packageRoot 'Source\Main\app.manifest')) ('/out:'+(Join-Path $packageRoot ('Tests\'+$name+'.exe'))) /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll (Join-Path $packageRoot ('Tests\'+$name+'.cs'))
    if($LASTEXITCODE -ne 0){throw ('Compile failed: '+$name)}
    & (Join-Path $packageRoot ('Tests\'+$name+'.exe')) (Join-Path $AppDirectory $case[2]) (Join-Path $EvidenceDirectory ('verification-'+$name))
    if($LASTEXITCODE -ne 0){throw ('Verification failed: '+$name)}
}
& (Join-Path $packageRoot 'Tests\SettingsButtonLayoutRegression.exe') (Join-Path $AppDirectory 'YiChuang.exe') (Join-Path $EvidenceDirectory 'verification-SettingsButtons-96') --96
if($LASTEXITCODE -ne 0){throw 'Settings buttons clip at 96 DPI'}
$llamaEvidence=Join-Path $EvidenceDirectory 'verification-LlamaRuntimeRegression'
$llamaData=[IO.File]::ReadAllText((Join-Path $llamaEvidence 'restart-data-path.txt'))
& (Join-Path $packageRoot 'Tests\LlamaRuntimeRegression.exe') (Join-Path $AppDirectory 'YiChuang.exe') (Join-Path $llamaEvidence 'restart-probe') $llamaData
if($LASTEXITCODE -ne 0){throw 'llama.cpp settings did not survive a new process'}
$customEvidence=Join-Path $EvidenceDirectory 'verification-CustomLocalModelsRegression'
$customData=[IO.File]::ReadAllText((Join-Path $customEvidence 'restart-data-path.txt'))
& (Join-Path $packageRoot 'Tests\CustomLocalModelsRegression.exe') (Join-Path $AppDirectory 'YiChuang.exe') (Join-Path $customEvidence 'restart-probe') --read $customData
if($LASTEXITCODE -ne 0){throw 'Custom local model configuration did not survive a new process'}
$fontDiscardEvidence=Join-Path $EvidenceDirectory 'verification-FontDiscardRegression'
$fontDiscardData=[IO.File]::ReadAllText((Join-Path $fontDiscardEvidence 'restart-data-path.txt'))
& (Join-Path $packageRoot 'Tests\FontDiscardRegression.exe') (Join-Path $AppDirectory 'YiChuang.exe') (Join-Path $fontDiscardEvidence 'restart-probe') --read $fontDiscardData
if($LASTEXITCODE -ne 0){throw 'Font discard/save configuration did not survive a new process'}
$proxyEvidence=Join-Path $EvidenceDirectory 'verification-NetworkProxyRegression'
$proxyData=[IO.File]::ReadAllText((Join-Path $proxyEvidence 'restart-data-path.txt'))
& (Join-Path $packageRoot 'Tests\NetworkProxyRegression.exe') (Join-Path $AppDirectory 'YiChuang.exe') (Join-Path $proxyEvidence 'restart-probe') --read $proxyData
if($LASTEXITCODE -ne 0){throw 'Global proxy fresh-process persistence failed'}
if($StartAt){Write-Host ('Remaining suites from '+$StartAt+' and restart probes passed.')}else{Write-Host 'All 28 UI/API/proxy/local-runtime/OCR suites passed.'}
