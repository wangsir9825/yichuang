param([string]$HyGguf='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$target=[IO.Path]::GetFullPath((Join-Path $root 'Evidence\installer-sandbox-1.5.5'))
$evidence=Join-Path $root 'Evidence'
if(-not $target.StartsWith([IO.Path]::GetFullPath($evidence)+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Sandbox outside Evidence.'}
if(Test-Path -LiteralPath $target){throw 'Use a fresh test directory; existing files must not be replaced.'}
$registration='HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{A5940783-1830-4CCA-A2D1-7D26198A0C9B}_is1'
if(Test-Path -LiteralPath $registration){throw 'A registered installation already exists; do not alter it for this test.'}
$shortcut=Join-Path ([Environment]::GetFolderPath('StartMenu')) 'Programs\译窗.lnk'
if(Test-Path -LiteralPath $shortcut){throw 'An existing YiChuang Start Menu shortcut must be preserved; use a clean test profile.'}
$dotnet=Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full'
$vc=Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64'
if($dotnet.Release -lt 528040 -or $vc.Installed -ne 1 -or $vc.Major -lt 14 -or $vc.Minor -lt 29){throw 'This test requires preinstalled runtimes and must not install system prerequisites.'}
function Get-PrivateHashes {
    $folders=@((Join-Path $env:APPDATA 'YiChuang'))
    $folders+=@(Get-ChildItem -LiteralPath $env:APPDATA -Directory -Filter '*-NoLogin-2.9.18B' | ForEach-Object {$_.FullName})
    $items=@();foreach($folder in $folders){foreach($name in @('setting.txt','setting.txt.bak','local-models.json','local-runtime.json')){
        $file=Join-Path $folder $name
        if(Test-Path -LiteralPath $file){$items+=([IO.Path]::GetFileName($folder)+'/'+$name+'='+(Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash)}
    }}
    return ($items -join ';')
}
function Get-ModelList {return (((Invoke-RestMethod 'http://127.0.0.1:11434/api/tags' -TimeoutSec 5).models | Sort-Object name | ForEach-Object {$_.name+'='+$_.digest}) -join ';')}
$beforeSettings=Get-PrivateHashes
$beforeModels=Get-ModelList
$setup=Join-Path $root 'Packages\YiChuang-Setup-1.5.5.exe'
$log=Join-Path $evidence 'release155-installer-install.log'
$arguments='/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /NOICONS /TASKS="" /DIR="'+$target+'" /LOG="'+$log+'"'
$process=Start-Process -FilePath $setup -ArgumentList $arguments -PassThru -WindowStyle Hidden
if(-not $process.WaitForExit(90000)){throw 'Installation test timed out; inspect the owned installer before proceeding.'}
if($process.ExitCode -ne 0){throw ('Setup exit code: '+$process.ExitCode)}
$mismatches=@();$count=0
foreach($file in Get-ChildItem -LiteralPath (Join-Path $root 'App') -Recurse -File){
    $relative=$file.FullName.Substring((Join-Path $root 'App').Length+1)
    $installed=Join-Path (Join-Path $target 'App') $relative
    if(-not(Test-Path -LiteralPath $installed) -or (Get-FileHash -LiteralPath $file.FullName).Hash -ne (Get-FileHash -LiteralPath $installed).Hash){$mismatches+=$relative};$count++
}
if($mismatches.Count){throw ('Installed file mismatch: '+($mismatches -join ','))}
if(-not(Test-Path -LiteralPath $registration)){throw 'Per-user uninstall registration missing.'}
if((Get-ItemProperty -LiteralPath $registration).InstallLocation.TrimEnd('\') -ne $target){throw 'Unexpected uninstall registration path.'}
if((Get-PrivateHashes) -ne $beforeSettings -or (Get-ModelList) -ne $beforeModels){throw 'Install modified private configuration or model list.'}
& (Join-Path $root 'Tests\IndependentOcrRegression.exe') (Join-Path $target 'App\wxocr.exe') (Join-Path $evidence 'release155-installer-independent-ocr')
if($LASTEXITCODE -ne 0){throw 'Installed OCR failed.'}
$llamaResult='not run; supply -HyGguf for real CPU inference'
if($HyGguf){
    $probeFolder=Join-Path $evidence 'release155-installer-cpu'
    & (Join-Path $root 'Tests\LlamaCpuExitProbe.exe') (Join-Path $target 'App\YiChuang.exe') $probeFolder $HyGguf
    if($LASTEXITCODE -ne 0){throw 'Installed llama.cpp CPU inference failed.'}
    $childPid=[int](Get-Content -LiteralPath (Join-Path $probeFolder 'child-pid.txt'))
    Start-Sleep -Milliseconds 800
    if(Get-Process -Id $childPid -ErrorAction SilentlyContinue){throw 'Owned llama.cpp child remains after host exit.'}
    $llamaResult='CPU translation passed; child reclaimed after abrupt host exit'
}

$uninstall=Join-Path $target 'unins000.exe'
if(-not (Get-Item -LiteralPath $uninstall).FullName.StartsWith($target+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Invalid uninstaller scope.'}
$uninstallLog=Join-Path $evidence 'release155-installer-uninstall.log'
$process=Start-Process -FilePath $uninstall -ArgumentList ('/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /LOG="'+$uninstallLog+'"') -PassThru -WindowStyle Hidden
if(-not $process.WaitForExit(60000)){throw 'Uninstall test timed out.'}
if($process.ExitCode -ne 0){throw ('Uninstall exit code: '+$process.ExitCode)}
if(Test-Path -LiteralPath (Join-Path $target 'App\YiChuang.exe')){throw 'Installed executable remains after uninstall.'}
if(Test-Path -LiteralPath $registration){throw 'Temporary uninstall registration remains.'}
if((Get-PrivateHashes) -ne $beforeSettings -or (Get-ModelList) -ne $beforeModels){throw 'Uninstall modified private configuration or model list.'}
$result=[pscustomobject]@{status='passed';installer=$setup;installedFilesVerified=$count;installExit=0;uninstallExit=0;ocrFromInstalledCopy='Chinese and English passed';llamaFromInstalledCopy=$llamaResult;privateSettingsPreserved=$true;modelListPreserved=$true;sharedRuntimes='already installed; missing-runtime branch not exercised';cleanComputer='not yet verified'}
$result | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $evidence 'release155-installer-roundtrip-result.json') -Encoding UTF8
$result | ConvertTo-Json
