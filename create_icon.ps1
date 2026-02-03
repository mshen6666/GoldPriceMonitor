Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap(64, 64)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.FillRectangle([System.Drawing.Brushes]::Gold, 0, 0, 64, 64)
$g.DrawRectangle([System.Drawing.Pens]::Black, 2, 2, 60, 60)
$g.DrawString("Au", [System.Drawing.SystemFonts]::CaptionFont, [System.Drawing.Brushes]::Black, 10, 10)
$bmp.Save("D:\workspace\GoldPriceMonitor\Resources\gold.ico", [System.Drawing.Imaging.ImageFormat]::Icon)
$bmp.Dispose()
$g.Dispose()
Write-Host "图标已创建: D:\workspace\GoldPriceMonitor\Resources\gold.ico"
