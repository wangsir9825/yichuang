param([string]$CompilerPath)
$ErrorActionPreference='Stop'
$packageRoot=$PSScriptRoot
if (-not $CompilerPath) {
    $vswhere=Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if(Test-Path -LiteralPath $vswhere){$vs=& $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -property installationPath; if($vs){$CompilerPath=Join-Path $vs 'MSBuild\Current\Bin\Roslyn\csc.exe'}}
}
if (-not $CompilerPath -or -not (Test-Path -LiteralPath $CompilerPath)){throw 'Install Visual Studio 2022 Build Tools, or supply -CompilerPath with Roslyn csc.exe.'}
$buildDir=Join-Path $packageRoot 'Build'
[IO.Directory]::CreateDirectory($buildDir) | Out-Null
$framework=Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319'
$appDir=Join-Path $packageRoot 'App'
function Assert-AppStopped {
    $busy=@(Get-CimInstance Win32_Process | Where-Object {$_.ExecutablePath -and ([IO.Path]::GetDirectoryName($_.ExecutablePath) -eq $appDir) -and $_.Name -match '^(YiChuang|wxocr|YiChuang-.+-mode)\.exe$'})
    if($busy.Count){throw '本目录程序或辅助工具仍在运行；构建已停止，现有App未更改。请先关闭译窗。'}
}
Assert-AppStopped
$stageDir=Join-Path $buildDir ('stage-'+[Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($stageDir) | Out-Null
function Invoke-Rebuild([string]$Name,[string]$SourceDir,[string]$Platform,[string]$Language,[string]$MainType,[bool]$IsMain) {
    $arguments=[Collections.Generic.List[string]]::new()
    foreach($value in @('/nologo','/target:winexe',('/platform:'+$Platform),'/unsafe+',('/langversion:'+$Language),('/main:'+$MainType),('/out:'+(Join-Path $stageDir $Name)),('/win32icon:'+(Join-Path $SourceDir 'app.ico')))) {$arguments.Add($value)}
    $references=@('mscorlib','System','System.Core','System.Drawing','System.Windows.Forms','System.Xml')
    if($IsMain){$references+=@('System.Data','System.Web','System.Web.Extensions','Microsoft.CSharp','System.Configuration','System.Management','System.Security')}
    foreach($reference in $references){$arguments.Add('/reference:'+(Join-Path $framework ($reference+'.dll')))}
    $dependencies=@('Newtonsoft.Json')
    if($IsMain){$dependencies+=@('DSkin','Noesis.Javascript');$arguments.Add('/win32manifest:'+(Join-Path $SourceDir 'app.manifest'))}
    else{$dependencies+=@('Tesseract')}
    foreach($dependency in $dependencies){$arguments.Add('/reference:'+(Join-Path $appDir ($dependency+'.dll')))}
    if($IsMain){
        foreach($resource in Get-ChildItem -LiteralPath (Join-Path $packageRoot 'Resources') -Filter '*.resources'){$arguments.Add('/resource:'+$resource.FullName+','+$resource.Name)}
        $arguments.Add('/resource:'+(Join-Path $packageRoot 'Resources\delegates-full.json')+',Rebuild.delegates.json')
    }
    foreach($source in Get-ChildItem -LiteralPath $SourceDir -Recurse -Filter '*.cs'){$arguments.Add($source.FullName)}
    $response=Join-Path $buildDir ($Name+'.rsp')
    [IO.File]::WriteAllLines($response,($arguments | ForEach-Object {'"'+$_+'"'}),[Text.UTF8Encoding]::new($true))
    & $CompilerPath ('@'+$response) 2>&1 | Tee-Object -FilePath (Join-Path $buildDir ($Name+'.log'))
    if($LASTEXITCODE -ne 0){throw ('Compilation failed: '+$Name)}
}
& (Join-Path $packageRoot 'Build-UiAssets.ps1')
Invoke-Rebuild 'YiChuang.exe' (Join-Path $packageRoot 'Source\Main') 'x86' '7.3' 'YiChuang.Program' $true
Invoke-Rebuild 'wxocr.exe' (Join-Path $packageRoot 'Source\OCR') 'x64' '12' 'wxocr.Program' $false
foreach($mode in @('select','F1','doubleR')){Copy-Item -LiteralPath (Join-Path $appDir ('YiChuang-'+$mode+'-mode.exe')) -Destination $stageDir}
& (Join-Path $packageRoot 'Build-InputHelpers.ps1') -AppDirectory $stageDir
Assert-AppStopped
$restoreDir=Join-Path $stageDir 'previous'
[IO.Directory]::CreateDirectory($restoreDir) | Out-Null
$installed=[Collections.Generic.List[string]]::new()
try {
    foreach($file in Get-ChildItem -LiteralPath $stageDir -File -Filter '*.exe') {
        $target=Join-Path $appDir $file.Name
        Copy-Item -LiteralPath $target -Destination $restoreDir
        $installed.Add($file.Name)
        Copy-Item -LiteralPath $file.FullName -Destination $target -Force
    }
} catch {
    foreach($name in $installed){Copy-Item -LiteralPath (Join-Path $restoreDir $name) -Destination (Join-Path $appDir $name) -Force}
    throw
}
Write-Host ('Built successfully: '+$appDir)
