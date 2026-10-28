param([string]$CompilerPath='')
$ErrorActionPreference='Stop'
$root=$PSScriptRoot
if(-not $CompilerPath){$CompilerPath=Join-Path $root 'Build\InstallerTools\Inno\tools\ISCC.exe'}
if(-not(Test-Path -LiteralPath $CompilerPath)){throw '需要 Inno Setup 6.7 或更新版本；使用 -CompilerPath 指定 ISCC.exe。'}
$exe=Join-Path $root 'App\YiChuang.exe'
$version=[Diagnostics.FileVersionInfo]::GetVersionInfo($exe).ProductVersion
$installerScript=Join-Path $root 'Installer\YiChuang.iss'
$installerVersion=[regex]::Match([IO.File]::ReadAllText($installerScript),'(?m)^#define AppVersion "([0-9]+\.[0-9]+\.[0-9]+)"').Groups[1].Value
if(-not $installerVersion -or ($version -ne $installerVersion -and $version -ne ($installerVersion+'.0'))){throw '主程序版本与安装脚本版本不一致，请先构建本次版本。'}
$runtimeRoot=Join-Path $root 'App\llama'
$runtimeManifest=Join-Path $runtimeRoot 'runtime-manifest.json'
if(-not(Test-Path -LiteralPath $runtimeManifest)){throw '缺少内置 llama.cpp，请先运行 Build-LlamaRuntime.ps1。'}
$runtime=Get-Content -LiteralPath $runtimeManifest -Raw | ConvertFrom-Json
if($runtime.version -ne 'b11429' -or @($runtime.files).Count -lt 5){throw '内置运行组件清单不正确。'}
foreach($entry in $runtime.files){
    if([IO.Path]::GetFileName($entry.name) -ne $entry.name){throw '运行组件清单包含非法路径。'}
    $runtimeFile=Join-Path $runtimeRoot $entry.name
    if(-not(Test-Path -LiteralPath $runtimeFile) -or (Get-FileHash -LiteralPath $runtimeFile -Algorithm SHA256).Hash -ne $entry.sha256){throw ('运行组件缺失或被修改：'+$entry.name)}
}

$output=Join-Path $root ('Packages\YiChuang-Setup-'+$installerVersion+'.exe')
if(Test-Path -LiteralPath $output){throw '安装包已存在；请先提升版本号，保留原交付文件。'}
$manifest=@()
foreach($name in @('dotnet48.exe','vc_redist.x64.exe')){
    $path=Join-Path $root ('Build\Prerequisites\'+$name)
    if(-not(Test-Path -LiteralPath $path)){throw ('缺少微软离线安装器：'+$path+'。请参阅 Installer\README.txt。')}
    $signature=Get-AuthenticodeSignature -LiteralPath $path
    if($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notmatch 'O=Microsoft Corporation'){throw ('微软安装器签名不正确：'+$name)}
    $manifest+=[pscustomobject]@{path=$name;sha256=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash;publisher=$signature.SignerCertificate.Subject}
}
foreach($file in Get-ChildItem -LiteralPath (Join-Path $root 'App') -Recurse -File){
    $relative=$file.FullName.Substring((Join-Path $root 'App').Length+1)
    if($file.Extension -match '^\.(gguf|safetensors|pt|pth|tmp|log)$' -or $relative -match '(^|\\)(setting\.txt|local-models\.json|local-runtime\.json|USER_RESOURCES\.md|AGENTS\.md)$'){throw ('App 中含非分发文件：'+$relative)}
    $manifest+=[pscustomobject]@{path=('App/'+$relative.Replace('\','/'));sha256=(Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash}
}
[IO.Directory]::CreateDirectory((Join-Path $root 'Evidence')) | Out-Null
[IO.File]::WriteAllText((Join-Path $root 'Evidence\settings-cleanup-installer-payload-manifest.json'),($manifest|ConvertTo-Json -Depth 4),[Text.UTF8Encoding]::new($false))
& $CompilerPath /Qp (Join-Path $root 'Installer\YiChuang.iss')
if($LASTEXITCODE -ne 0){throw '安装包编译失败。'}
Get-FileHash -LiteralPath $output -Algorithm SHA256 | ConvertTo-Json | Set-Content -LiteralPath ($output+'.sha256.json') -Encoding UTF8
Write-Host ('安装包已生成：'+$output)
