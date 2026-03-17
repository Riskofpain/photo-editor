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
        });
    }
    
    private static float[] CreateContrastExposureMatrix(float contrast, float exposure)
    {
        float t = (1.0f - contrast) / 2.0f;
        float expMult = (float)System.Math.Pow(2, exposure);
        
        return new float[]
        {
            contrast * expMult, 0, 0, 0, t * 255 * expMult,
            0, contrast * expMult, 0, 0, t * 255 * expMult,
            0, 0, contrast * expMult, 0, t * 255 * expMult,
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
