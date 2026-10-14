$ErrorActionPreference='Stop'
# The app starts a local service only when a local engine is selected.
$program=Join-Path $PSScriptRoot 'App\YiChuang.exe'
if(-not(Test-Path -LiteralPath $program)){throw 'App\YiChuang.exe 不存在，请完整解压软件包。'}
Start-Process -FilePath $program -WorkingDirectory (Join-Path $PSScriptRoot 'App')
