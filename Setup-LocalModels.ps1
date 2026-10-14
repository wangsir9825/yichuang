param([string]$OllamaPath='')
$ErrorActionPreference='Stop'
if (-not $OllamaPath -or -not (Test-Path -LiteralPath $OllamaPath)) {
    $command = Get-Command ollama -ErrorAction SilentlyContinue
    if (-not $command) { throw 'Install Ollama first, or provide its executable with -OllamaPath.' }
    $OllamaPath = $command.Source
}
& (Join-Path $PSScriptRoot 'Start-LocalService.ps1') -OllamaPath $OllamaPath
& $OllamaPath pull 'kaelri/hy-mt2:7b'
if ($LASTEXITCODE -ne 0) { throw 'Hy-MT2 download failed. Run this script again to resume.' }
& $OllamaPath create 'yichuang-hy-mt2:7b-q6' -f (Join-Path $PSScriptRoot 'Local-Hy-MT2.Modelfile')
if ($LASTEXITCODE -ne 0) { throw 'Hy-MT2 prompt-template setup failed.' }
& $OllamaPath pull 'translategemma:12b'
if ($LASTEXITCODE -ne 0) { throw 'TranslateGemma download failed. Run this script again to resume.' }
Write-Host 'Both local translation models are ready. Enable the desired local engines in YiChuang settings, then select one above the translation pane.'
