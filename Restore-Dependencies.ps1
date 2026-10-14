$ErrorActionPreference='Stop'
$manifest=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'DEPENDENCIES.json') -Raw | ConvertFrom-Json
$work=Join-Path $PSScriptRoot 'Build\dependencies'
New-Item -ItemType Directory -Path $work -Force | Out-Null
$archive=Join-Path $work 'portable.zip'
$url='https://github.com/wangsir9825/yichuang/releases/download/v1.5.4/yichuang-v1.5.4-windows-x64-portable.zip'
if(-not(Test-Path -LiteralPath $archive)){[Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12;Invoke-WebRequest -UseBasicParsing -Uri $url -OutFile $archive}
if((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ne $manifest.portable_sha256){throw 'Dependency archive SHA-256 mismatch.'}
if(Test-Path -LiteralPath (Join-Path $PSScriptRoot 'App')){throw 'App already exists. Use a fresh checkout to avoid replacing your program.'}
Expand-Archive -LiteralPath $archive -DestinationPath (Join-Path $work 'extracted') -Force
foreach($item in $manifest.files){$file=Join-Path (Join-Path $work 'extracted') $item.path;if((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -ne $item.sha256){throw ('Dependency mismatch: '+$item.path)}}
Copy-Item -LiteralPath (Join-Path $work 'extracted\App') -Destination $PSScriptRoot -Recurse
Write-Host 'Dependencies verified and restored. No translation models were downloaded.'
