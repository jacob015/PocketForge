param(
    [Parameter(Mandatory = $true)]
    [string]$InputPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath,

    [Parameter(Mandatory = $true)]
    [int]$TargetWidth,

    [Parameter(Mandatory = $true)]
    [int]$TargetHeight,

    [int]$BackgroundTolerance = 70,
    [int]$CropPadding = 2
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Drawing

function Get-ColorDistanceSquared {
    param(
        [System.Drawing.Color]$Color,
        [System.Drawing.Color]$Reference
    )

    $red = [int]$Color.R - [int]$Reference.R
    $green = [int]$Color.G - [int]$Reference.G
    $blue = [int]$Color.B - [int]$Reference.B
    return ($red * $red) + ($green * $green) + ($blue * $blue)
}

$source = [System.Drawing.Bitmap]::FromFile((Resolve-Path -LiteralPath $InputPath))
try {
    $working = New-Object System.Drawing.Bitmap($source.Width, $source.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($working)
    try {
        $graphics.DrawImageUnscaled($source, 0, 0)
    }
    finally {
        $graphics.Dispose()
    }

    $background = $working.GetPixel(0, 0)
    $toleranceSquared = $BackgroundTolerance * $BackgroundTolerance
    $visited = New-Object 'bool[]' ($working.Width * $working.Height)
    $queue = [System.Collections.Generic.Queue[int]]::new()

    for ($x = 0; $x -lt $working.Width; $x++) {
        $queue.Enqueue($x)
        $queue.Enqueue((($working.Height - 1) * $working.Width) + $x)
    }
    for ($y = 1; $y -lt ($working.Height - 1); $y++) {
        $queue.Enqueue($y * $working.Width)
        $queue.Enqueue(($y * $working.Width) + $working.Width - 1)
    }

    while ($queue.Count -gt 0) {
        $index = $queue.Dequeue()
        if ($visited[$index]) {
            continue
        }

        $visited[$index] = $true
        $x = $index % $working.Width
        $y = [math]::Floor($index / $working.Width)
        $color = $working.GetPixel($x, $y)
        $distanceSquared = Get-ColorDistanceSquared -Color $color -Reference $background
        if ($distanceSquared -gt $toleranceSquared) {
            continue
        }

        $working.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(0, $color.R, $color.G, $color.B))

        if ($x -gt 0) { $queue.Enqueue($index - 1) }
        if ($x + 1 -lt $working.Width) { $queue.Enqueue($index + 1) }
        if ($y -gt 0) { $queue.Enqueue($index - $working.Width) }
        if ($y + 1 -lt $working.Height) { $queue.Enqueue($index + $working.Width) }
    }

    $minX = $working.Width
    $minY = $working.Height
    $maxX = -1
    $maxY = -1
    for ($y = 0; $y -lt $working.Height; $y++) {
        for ($x = 0; $x -lt $working.Width; $x++) {
            if ($working.GetPixel($x, $y).A -eq 0) {
                continue
            }

            $minX = [math]::Min($minX, $x)
            $minY = [math]::Min($minY, $y)
            $maxX = [math]::Max($maxX, $x)
            $maxY = [math]::Max($maxY, $y)
        }
    }

    if ($maxX -lt $minX -or $maxY -lt $minY) {
        throw "No foreground pixels remained after background removal: $InputPath"
    }

    $minX = [math]::Max(0, $minX - $CropPadding)
    $minY = [math]::Max(0, $minY - $CropPadding)
    $maxX = [math]::Min($working.Width - 1, $maxX + $CropPadding)
    $maxY = [math]::Min($working.Height - 1, $maxY + $CropPadding)
    $crop = [System.Drawing.Rectangle]::new($minX, $minY, ($maxX - $minX + 1), ($maxY - $minY + 1))

    $destination = New-Object System.Drawing.Bitmap($TargetWidth, $TargetHeight, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $destination.SetResolution(96, 96)
    $destinationGraphics = [System.Drawing.Graphics]::FromImage($destination)
    try {
        $destinationGraphics.Clear([System.Drawing.Color]::Transparent)
        $destinationGraphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
        $destinationGraphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
        $destinationGraphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $destinationGraphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $destinationGraphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
        $target = [System.Drawing.Rectangle]::new(0, 0, $TargetWidth, $TargetHeight)
        $destinationGraphics.DrawImage($working, $target, $crop, [System.Drawing.GraphicsUnit]::Pixel)
    }
    finally {
        $destinationGraphics.Dispose()
    }

    $outputDirectory = Split-Path -Parent $OutputPath
    if (-not (Test-Path -LiteralPath $outputDirectory)) {
        New-Item -ItemType Directory -Path $outputDirectory | Out-Null
    }
    $destination.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $destination.Dispose()

    Write-Output "Processed $InputPath -> $OutputPath; crop=$($crop.X),$($crop.Y),$($crop.Width),$($crop.Height); target=${TargetWidth}x${TargetHeight}"
}
finally {
    if ($null -ne $working) {
        $working.Dispose()
    }
    $source.Dispose()
}
