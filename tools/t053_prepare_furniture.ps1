$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent
$source = Join-Path $root 'Evidence\25d\T01\furniture-v1.png'
$expected = '2926CC4B2D9AA0ABF1D8330F36CB265D541B7902F0A6F253BD49840899A70797'
if ((Get-FileHash $source -Algorithm SHA256).Hash -ne $expected) { throw 'T01.5 source mismatch' }
$folder = Join-Path $root 'Assets\Art\Illustrated25D\Environment\Furniture'
$im = [Drawing.Bitmap]::FromFile($source)
try {
    # Main cabinet only; lower inset is never imported as a second cabinet.
    $body = $im.Clone([Drawing.Rectangle]::new(0,200,1774,390), [Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $body.Save((Join-Path $folder 'furniture-body-v1.png'), [Drawing.Imaging.ImageFormat]::Png)
    $body.Dispose()
    # Opaque interior wood patch; dimensions remain unchanged (UV tiling happens on the authored surface).
    $top = $im.Clone([Drawing.Rectangle]::new(120,113,1510,45), [Drawing.Imaging.PixelFormat]::Format32bppArgb)
    # Source interior has alpha251-255; normalize only this known solid wooden material patch.
    for ($y=0; $y -lt $top.Height; $y++) { for ($x=0; $x -lt $top.Width; $x++) {
        $c=$top.GetPixel($x,$y)
        if($c.A -lt 240) { throw 'Patch intersects transparent exterior; do not fill it' }
        $top.SetPixel($x,$y,[Drawing.Color]::FromArgb(255,$c.R,$c.G,$c.B))
    }}
    $top.Save((Join-Path $folder 'furniture-top-wood-v1.png'), [Drawing.Imaging.ImageFormat]::Png)
    $top.Dispose()
} finally { $im.Dispose() }
$files = @('furniture-body-v1.png','furniture-top-wood-v1.png') | ForEach-Object {
    $p = Join-Path $folder $_
    $img = [Drawing.Bitmap]::FromFile($p)
    try {
        $minimum = 255; $visible = 0
        for ($y=0; $y -lt $img.Height; $y++) { for ($x=0; $x -lt $img.Width; $x++) {
            $a=$img.GetPixel($x,$y).A; if ($a -lt $minimum) {$minimum=$a}; if($a -gt 0){$visible++}
        }}
        [ordered]@{path=$p.Substring($root.Length+1);width=$img.Width;height=$img.Height;minAlpha=$minimum;visiblePixels=$visible;sha256=(Get-FileHash $p -Algorithm SHA256).Hash.ToLower()}
    } finally { $img.Dispose() }
}
if ($files[1].minAlpha -ne 255) { throw 'Support texture must be opaque' }
[ordered]@{session='2026-11-13-copilot-t053-b29a5ec7';source='Evidence/25d/T01/furniture-v1.png';sourceSHA256=$expected.ToLower();method='System.Drawing crop; RGB unchanged, interior wood alpha251-255 normalized to255, rejecting alpha<240; body original alpha; no generation or resizing';runtimeValidated=$false;humanReview='pending';files=$files} | ConvertTo-Json -Depth 6 | Set-Content (Join-Path $root 'Evidence\25d\T05.3\manifest.json') -Encoding UTF8
Write-Output 'T053 approved-source crops and alpha audit complete'
