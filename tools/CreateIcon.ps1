Add-Type -AssemblyName System.Drawing
$iconDirectory = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\Assets'))
[IO.Directory]::CreateDirectory($iconDirectory) | Out-Null
$images = @()
foreach ($size in @(32, 48, 256)) {
    $bitmap = [Drawing.Bitmap]::new($size, $size)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.ScaleTransform($size / 256.0, $size / 256.0)
    $background = [Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(82, 101, 223))
    $shape = [Drawing.Drawing2D.GraphicsPath]::new()
    $shape.AddArc(0, 0, 76, 76, 180, 90)
    $shape.AddArc(180, 0, 76, 76, 270, 90)
    $shape.AddArc(180, 180, 76, 76, 0, 90)
    $shape.AddArc(0, 180, 76, 76, 90, 90)
    $shape.CloseFigure()
    $graphics.FillPath($background, $shape)
    $pen = [Drawing.Pen]::new([Drawing.Color]::White, 12)
    $pen.StartCap = [Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [Drawing.Drawing2D.LineCap]::Round
    $graphics.DrawEllipse($pen, 57, 72, 142, 142)
    $graphics.DrawLine($pen, 128, 44, 128, 67)
    $graphics.DrawLine($pen, 108, 42, 148, 42)
    $graphics.DrawLine($pen, 128, 101, 128, 144)
    $graphics.DrawLine($pen, 128, 144, 158, 144)
    $graphics.DrawLine($pen, 186, 70, 199, 57)
    $buffer = [IO.MemoryStream]::new()
    $bitmap.Save($buffer, [Drawing.Imaging.ImageFormat]::Png)
    $images += ,@{ Size = $size; Data = $buffer.ToArray() }
    $buffer.Dispose(); $pen.Dispose(); $shape.Dispose(); $background.Dispose(); $graphics.Dispose(); $bitmap.Dispose()
}
$file = [IO.File]::Create((Join-Path $iconDirectory 'alarm.ico'))
$writer = [IO.BinaryWriter]::new($file)
$writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]$images.Count)
$offset = 6 + 16 * $images.Count
foreach ($item in $images) {
    $dimension = if ($item.Size -eq 256) { 0 } else { $item.Size }
    $writer.Write([byte]$dimension); $writer.Write([byte]$dimension)
    $writer.Write([byte]0); $writer.Write([byte]0)
    $writer.Write([uint16]1); $writer.Write([uint16]32)
    $writer.Write([uint32]$item.Data.Length); $writer.Write([uint32]$offset)
    $offset += $item.Data.Length
}
foreach ($item in $images) { $writer.Write([byte[]]$item.Data) }
$writer.Dispose(); $file.Dispose()
