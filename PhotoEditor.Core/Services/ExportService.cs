using System;
using System.IO;
using SkiaSharp;

namespace PhotoEditor.Core.Services;

public class ExportService
{
    public void ExportImage(SKBitmap bitmap, string outputPath, SKEncodedImageFormat format, int quality, float scale = 1.0f)
    {
        if (bitmap == null) throw new ArgumentNullException(nameof(bitmap));

        SKBitmap toExport = bitmap;
        
        if (scale != 1.0f)
        {
            var info = new SKImageInfo((int)(bitmap.Width * scale), (int)(bitmap.Height * scale));
            toExport = bitmap.Resize(info, SKFilterQuality.High);
        }

        using var image = SKImage.FromBitmap(toExport);
        using var data = image.Encode(format, quality);
        using var stream = File.OpenWrite(outputPath);
        data.SaveTo(stream);
        
        if (toExport != bitmap)
            toExport.Dispose();
    }
}
