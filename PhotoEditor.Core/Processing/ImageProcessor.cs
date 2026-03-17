using System;
using System.Threading.Tasks;
using PhotoEditor.Core.Models;
using SkiaSharp;

namespace PhotoEditor.Core.Processing;

public class ImageProcessor
{
    /// <summary>
    /// All parameters are expected in -100 to +100 range (Lightroom style).
    /// 0 = no change for all sliders.
    /// </summary>
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
        // Start with identity
        float[] result = {
            1, 0, 0, 0, 0,
            0, 1, 0, 0, 0,
            0, 0, 1, 0, 0,
            0, 0, 0, 1, 0
        };

        // Apply in order: Exposure → Contrast → Saturation
        result = MultiplyMatrix(result, CreateExposureMatrix(p.Exposure));
        result = MultiplyMatrix(result, CreateContrastMatrix(p.Contrast));
        result = MultiplyMatrix(result, CreateSaturationMatrix(p.Saturation));

        return result;
    }

    /// <summary>
    /// Exposure: -100 to +100 maps to approximately -2 EV to +2 EV.
    /// 0 = no change (multiplier 1.0).
    /// </summary>
    private static float[] CreateExposureMatrix(double exposure)
    {
        // Map -100..+100 → -2..+2 EV stops
        double ev = (exposure / 100.0) * 2.0;
        float e = (float)Math.Pow(2.0, ev);

        return new float[]
        {
            e, 0, 0, 0, 0,
            0, e, 0, 0, 0,
            0, 0, e, 0, 0,
            0, 0, 0, 1, 0
        };
    }

    /// <summary>
    /// Contrast: -100 to +100 maps to multiplier range 0.5 to 1.5.
    /// 0 = no change (multiplier 1.0).
    /// At -100, contrast = 0.5 (washed out). At +100, contrast = 1.5 (punchy).
    /// </summary>
    private static float[] CreateContrastMatrix(double contrast)
    {
        // Map -100..+100 → 0.5..1.5
        float c = (float)(1.0 + (contrast / 200.0));
        float t = 128f * (1f - c);

        return new float[]
        {
            c, 0, 0, 0, t,
            0, c, 0, 0, t,
            0, 0, c, 0, t,
            0, 0, 0, 1, 0
        };
    }

    /// <summary>
    /// Saturation: -100 to +100 maps to multiplier 0.0 to 2.0.
    /// 0 = no change (multiplier 1.0).
    /// At -100, fully desaturated (grayscale). At +100, double saturation.
    /// </summary>
    private static float[] CreateSaturationMatrix(double saturation)
    {
        // Map -100..+100 → 0.0..2.0
        float s = (float)(1.0 + (saturation / 100.0));
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
