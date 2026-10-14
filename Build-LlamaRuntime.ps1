param([string]$CacheDirectory='')
$ErrorActionPreference='Stop'
$root=$PSScriptRoot
if(-not $CacheDirectory){$CacheDirectory=Join-Path $root 'Build\LlamaRuntime'}
$tag='b11429'
$name='llama-b11429-bin-win-vulkan-x64.zip'
$sha='1bfe78ad9168b79fa02bf67f6af9f5e17a966d824d77238517f7bef12ac73b36'
$target=Join-Path $root 'App\llama'
$busy=Get-CimInstance Win32_Process -Filter "Name='llama-server.exe'" | Where-Object {$_.ExecutablePath -eq (Join-Path $target 'llama-server.exe')}
if($busy){throw '本目录内置模型仍在运行，请先退出译窗。'}
New-Item -ItemType Directory -Force -Path $CacheDirectory | Out-Null
$archive=Join-Path $CacheDirectory $name
if(-not(Test-Path -LiteralPath $archive)){Invoke-WebRequest ('https://github.com/ggml-org/llama.cpp/releases/download/'+$tag+'/'+$name) -OutFile $archive -UseBasicParsing}
if((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ne $sha){throw 'llama.cpp 官方发布归档 SHA256 不匹配；未更新 App。'}
$unpacked=Join-Path $CacheDirectory ('verified-'+$tag)
if(-not(Test-Path -LiteralPath $unpacked)){Expand-Archive -LiteralPath $archive -DestinationPath $unpacked}
New-Item -ItemType Directory -Force -Path $target | Out-Null
$files=Get-ChildItem -LiteralPath $unpacked -File | Where-Object {$_.Name -match '^(ggml.*\.dll|llama\.dll|llama-common\.dll|llama-server(-impl\.dll|\.exe)|mtmd\.dll|libomp\.dll|LICENSE-LLVM-OpenMP)$'}
if(-not($files.Name -contains 'llama-server.exe')){throw '运行器归档结构不正确。'}
$files | Copy-Item -Destination $target -Force
Invoke-WebRequest ('https://raw.githubusercontent.com/ggml-org/llama.cpp/'+$tag+'/LICENSE') -OutFile (Join-Path $target 'LICENSE-llama.cpp.txt') -UseBasicParsing
$manifest=[ordered]@{version=$tag;source=('https://github.com/ggml-org/llama.cpp/releases/tag/'+$tag);archive=$name;archiveSha256=$sha;files=@(Get-ChildItem -LiteralPath $target -File | Where-Object {$_.Name -ne 'runtime-manifest.json'} | ForEach-Object {[ordered]@{name=$_.Name;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash}})}
$manifest | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $target 'runtime-manifest.json') -Encoding UTF8
Write-Host ('Pinned llama.cpp runtime ready: '+$target)
