$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$bitmap=[Drawing.Bitmap]::new(64,64)
$graphics=[Drawing.Graphics]::FromImage($bitmap)
try {
    $graphics.SmoothingMode=[Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.Clear([Drawing.Color]::FromArgb(24,120,109))
    $pen=[Drawing.Pen]::new([Drawing.Color]::White,3)
    try {$graphics.DrawRectangle($pen,15,14,34,36);$graphics.DrawLine($pen,15,24,49,24);$graphics.DrawLine($pen,30,24,30,50)} finally {$pen.Dispose()}
    $png=[IO.MemoryStream]::new()
    try {
        $bitmap.Save($png,[Drawing.Imaging.ImageFormat]::Png);$payload=$png.ToArray()
        $output=[IO.File]::Create((Join-Path $PSScriptRoot 'Source\Main\app.ico'));$writer=[IO.BinaryWriter]::new($output)
        try {
            $writer.Write([uint16]0);$writer.Write([uint16]1);$writer.Write([uint16]1)
            $writer.Write([byte]64);$writer.Write([byte]64);$writer.Write([byte]0);$writer.Write([byte]0)
            $writer.Write([uint16]1);$writer.Write([uint16]32);$writer.Write([uint32]$payload.Length);$writer.Write([uint32]22);$writer.Write($payload)
        } finally {$writer.Dispose();$output.Dispose()}
    } finally {$png.Dispose()}
} finally {$graphics.Dispose();$bitmap.Dispose()}
