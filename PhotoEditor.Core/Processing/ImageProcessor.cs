using System.Threading.Tasks;
using PhotoEditor.Core.Models;
using SkiaSharp;

namespace PhotoEditor.Core.Processing;

public class ImageProcessor
{
    public static async Task<SKBitmap?> ProcessImageAsync(SKBitmap? source, AdjustmentParameters parameters)
    {
        if (source == null) return null;

        return await Task.Run(() => 
        {
            try
            {
                var info = new SKImageInfo(source.Width, source.Height, SKColorType.Bgra8888);
                var result = new SKBitmap(info);
                
                using var canvas = new SKCanvas(result);
                using var paint = new SKPaint();
                
                // 1. Exposure and Contrast Matrix
                var contrastAndExposure = CreateContrastExposureMatrix(parameters.Contrast, parameters.Exposure);
                
                // 2. Saturation
                var saturation = SKColorFilter.CreateColorMatrix(CreateSaturationMatrix(parameters.Saturation));
                
                paint.ColorFilter = SKColorFilter.CreateCompose(saturation, SKColorFilter.CreateColorMatrix(contrastAndExposure));
                
                canvas.DrawBitmap(source, 0, 0, paint);
                
                return result;
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("SkiaSharp Processing Error: " + ex.Message);
                return null; // Don't crash the whole app
            }
        });
    }
    
    private static float[] CreateContrastExposureMatrix(float contrast, float exposure)
    {
        // Skia matrices expect values around 1.0f as identity for scaling.
        // And translation offsets (-255 to 255) for shifting.
        
        // 1. Calculate exposure multiplier. 0 exposure = 1x (no change)
        float expMult = (float)System.Math.Pow(2, exposure);
        
        // 2. Calculate contrast. 1.0 = normal. > 1.0 = high contrast. < 1.0 = low contrast.
        float c = contrast;
        
        // The translation offset dynamically shifts the midpoint (128) back to the center 
        // after scaling the contrast, so colors stretch evenly rather than just getting brighter/darker.
        float t = (1.0f - c) * 128.0f; // Alternatively: (1.0f - c) / 2.0f * 255.0f

        // The matrix essentially answers: NewColor = (OldColor * contrast * exposure) + translation
        return new float[]
        {
            c * expMult, 0, 0, 0, t * expMult,
            0, c * expMult, 0, 0, t * expMult,
            0, 0, c * expMult, 0, t * expMult,
            0, 0, 0, 1, 0
        };
    }
    
    private static float[] CreateSaturationMatrix(float sat)
    {
        float invSat = 1 - sat;
        float R = 0.213f * invSat;
        float G = 0.715f * invSat;
        float B = 0.072f * invSat;

        return new float[]
        {
            R + sat, G, B, 0, 0,
            R, G + sat, B, 0, 0,
            R, G, B + sat, 0, 0,
            0, 0, 0, 1, 0
        };
    }
}
