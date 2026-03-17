using System;
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

                // Build combined color matrix
                var matrix = BuildColorMatrix(parameters);
                paint.ColorFilter = SKColorFilter.CreateColorMatrix(matrix);

                canvas.DrawBitmap(source, 0, 0, paint);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("SkiaSharp Processing Error: " + ex.Message);
                return null;
            }
        });
    }

    private static float[] BuildColorMatrix(AdjustmentParameters p)
    {
        // Start with identity matrix
        float[] result = {
            1, 0, 0, 0, 0,
            0, 1, 0, 0, 0,
            0, 0, 1, 0, 0,
            0, 0, 0, 1, 0
        };

        // 1. Apply Exposure
        result = MultiplyMatrix(result, CreateExposureMatrix(p.Exposure));

        // 2. Apply Contrast
        result = MultiplyMatrix(result, CreateContrastMatrix(p.Contrast));

        // 3. Apply Saturation
        result = MultiplyMatrix(result, CreateSaturationMatrix(p.Saturation));

        return result;
    }

    private static float[] CreateExposureMatrix(double exposure)
    {
        float e = (float)Math.Pow(2, Math.Clamp(exposure, -5.0, 5.0));
        return new float[]
        {
            e, 0, 0, 0, 0,
            0, e, 0, 0, 0,
            0, 0, e, 0, 0,
            0, 0, 0, 1, 0
        };
    }

    private static float[] CreateContrastMatrix(double contrast)
    {
        float c = (float)Math.Clamp(contrast, 0.0, 2.0);
        float t = 128f * (1f - c);
        return new float[]
        {
            c, 0, 0, 0, t,
            0, c, 0, 0, t,
            0, 0, c, 0, t,
            0, 0, 0, 1, 0
        };
    }

    private static float[] CreateSaturationMatrix(double saturation)
    {
        float s = (float)Math.Clamp(saturation, 0.0, 2.0);
        float inv = 1f - s;
        float R = 0.213f * inv;
        float G = 0.715f * inv;
        float B = 0.072f * inv;
        return new float[]
        {
            R + s, G,     B,     0, 0,
            R,     G + s, B,     0, 0,
            R,     G,     B + s, 0, 0,
            0,     0,     0,     1, 0
        };
    }

    /// <summary>
    /// Multiplies two 4x5 SkiaSharp color matrices (row-major).
    /// The 5th column is the translation vector.
    /// </summary>
    private static float[] MultiplyMatrix(float[] a, float[] b)
    {
        float[] result = new float[20];
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                float sum = 0;
                for (int k = 0; k < 4; k++)
                {
                    sum += a[row * 5 + k] * b[k * 5 + col];
                }
                if (col == 4)
                {
                    sum += a[row * 5 + 4];
                }
                result[row * 5 + col] = sum;
            }
        }
        return result;
    }
}
