param(
    [string]$OllamaPath='',
    [string]$ModelsDirectory='',
    [string[]]$RequiredModels=@()
)
$ErrorActionPreference='Stop'
function Confirm-Models($tags) {
    $missing=@($RequiredModels | Where-Object { $_ -notin @($tags.models.name) })
    if($missing.Count){throw ('服务已连接，但实际模型列表缺少：'+($missing -join '、')+'。请核对 Ollama 使用的缓存目录；本脚本不会下载模型或结束已有服务。')}
    Write-Host ('Ollama 已就绪，实际列表包含 '+@($tags.models).Count+' 个模型。译窗会在翻译前核对当前选择的模型；不会自动下载。')
}
$existingTags=$null
try { $existingTags=Invoke-RestMethod 'http://127.0.0.1:11434/api/tags' -TimeoutSec 2 } catch {}
if($null -ne $existingTags){Confirm-Models $existingTags; return}
$options=$null
$optionsFile=Join-Path $env:APPDATA 'Xtranslator-NoLogin-2.9.18B\local-runtime.json'
if(Test-Path -LiteralPath $optionsFile){try{$options=Get-Content -LiteralPath $optionsFile -Raw | ConvertFrom-Json}catch{}}
$candidates=[Collections.Generic.List[string]]::new()
if($OllamaPath){$candidates.Add($OllamaPath)}
if($options.Executable){$candidates.Add($options.Executable)}
$command=Get-Command ollama.exe -ErrorAction SilentlyContinue
if($command){$candidates.Add($command.Source)}
foreach($folder in @((Join-Path $env:LOCALAPPDATA 'Programs\Ollama'),(Join-Path $env:ProgramFiles 'Ollama'))){$candidates.Add((Join-Path $folder 'ollama.exe'))}
foreach($process in @(Get-CimInstance Win32_Process -Filter "Name='ollama.exe' OR Name='ollama app.exe'" -ErrorAction SilentlyContinue)){
    if($process.ExecutablePath){$candidates.Add((Join-Path ([IO.Path]::GetDirectoryName($process.ExecutablePath)) 'ollama.exe'))}
}
foreach($key in @('HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*','HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*','HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*')){
    foreach($item in @(Get-ItemProperty -Path $key -ErrorAction SilentlyContinue | Where-Object {$_.DisplayName -like 'Ollama*' -and $_.InstallLocation})){$candidates.Add((Join-Path $item.InstallLocation 'ollama.exe'))}
}
$OllamaPath=$candidates | Where-Object {$_ -and (Test-Path -LiteralPath $_ -PathType Leaf)} | Select-Object -First 1
if(-not $OllamaPath){throw '未找到 Ollama。请打开译窗设置 → 翻译引擎 → 本地模型配置向导，安装或选择 Ollama。'}
if(-not $ModelsDirectory){$ModelsDirectory=$options.ModelsDirectory}
if(-not $ModelsDirectory){$ModelsDirectory=$env:OLLAMA_MODELS}
if(-not $ModelsDirectory){$ModelsDirectory=[Environment]::GetEnvironmentVariable('OLLAMA_MODELS','User')}
if(-not $ModelsDirectory){$ModelsDirectory=[Environment]::GetEnvironmentVariable('OLLAMA_MODELS','Machine')}
if(-not $ModelsDirectory){$ModelsDirectory=Join-Path $env:USERPROFILE '.ollama\models'}
$ModelsDirectory=[IO.Path]::GetFullPath([Environment]::ExpandEnvironmentVariables($ModelsDirectory))
[IO.Directory]::CreateDirectory($ModelsDirectory) | Out-Null
$runtimeLogs=Join-Path $env:APPDATA 'Xtranslator-NoLogin-2.9.18B\LocalRuntime'
[IO.Directory]::CreateDirectory($runtimeLogs) | Out-Null
$previousModels=$env:OLLAMA_MODELS; $previousHost=$env:OLLAMA_HOST; $previousCloud=$env:OLLAMA_NO_CLOUD
try {
    $env:OLLAMA_MODELS=$ModelsDirectory; $env:OLLAMA_HOST='127.0.0.1:11434'; $env:OLLAMA_NO_CLOUD='1'
    $suffix=Get-Date -Format 'yyyyMMdd-HHmmss'
    Start-Process -FilePath $OllamaPath -ArgumentList 'serve' -WindowStyle Hidden -RedirectStandardOutput (Join-Path $runtimeLogs ("ollama-$suffix-out.log")) -RedirectStandardError (Join-Path $runtimeLogs ("ollama-$suffix-error.log")) | Out-Null
} finally { $env:OLLAMA_MODELS=$previousModels; $env:OLLAMA_HOST=$previousHost; $env:OLLAMA_NO_CLOUD=$previousCloud }
$ready=$null
for($attempt=0;$attempt -lt 20;$attempt++) {
    try {$ready=Invoke-RestMethod 'http://127.0.0.1:11434/api/tags' -TimeoutSec 2; break} catch {Start-Sleep -Milliseconds 500}
}
if($null -eq $ready){throw ('Ollama 未能就绪，请查看 '+$runtimeLogs+' 中本次启动日志。')}
Confirm-Models $ready
