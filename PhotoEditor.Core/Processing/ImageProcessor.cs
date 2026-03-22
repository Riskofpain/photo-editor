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
        float[] result = {
            1, 0, 0, 0, 0,
            0, 1, 0, 0, 0,
            0, 0, 1, 0, 0,
            0, 0, 0, 1, 0
        };

        // Apply in order: Exposure → Brightness → Contrast → Saturation
        result = MultiplyMatrix(result, CreateExposureMatrix(p.Exposure));
        result = MultiplyMatrix(result, CreateBrightnessMatrix(p.Brightness));
        result = MultiplyMatrix(result, CreateContrastMatrix(p.Contrast));
        result = MultiplyMatrix(result, CreateSaturationMatrix(p.Saturation));

        return result;
    }

    /// <summary>
    /// Exposure: -100 to +100 maps to -2 EV to +2 EV.
    /// 0 = no change (multiplier 1.0).
    /// </summary>
    private static float[] CreateExposureMatrix(double exposure)
    {
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
    /// Brightness: -100 to +100 maps to a linear offset of -0.5 to +0.5.
    /// This is a simple additive shift (unlike Exposure which is multiplicative).
    /// </summary>
    private static float[] CreateBrightnessMatrix(double brightness)
    {
        // Map -100..+100 → -0.5..+0.5 (normalized 0-1 color space)
        float b = (float)(brightness / 200.0);
        return new float[]
        {
            1, 0, 0, 0, b,
            0, 1, 0, 0, b,
            0, 0, 1, 0, b,
            0, 0, 0, 1, 0
        };
    }

    /// <summary>
    /// Contrast: -100 to +100 maps to multiplier 0.5 to 1.5.
    /// 0 = no change. Pivots around mid-gray (0.5 normalized).
    /// </summary>
    private static float[] CreateContrastMatrix(double contrast)
    {
        float c = (float)(1.0 + (contrast / 200.0));
        float t = 0.5f * (1f - c);
        return new float[]
        {
            c, 0, 0, 0, t,
            0, c, 0, 0, t,
            0, 0, c, 0, t,
            0, 0, 0, 1, 0
        };
    }

    /// <summary>
    /// Saturation: -100 to +100 maps to 0.0 to 2.0.
    /// At -100, fully desaturated. At +100, double saturation.
    /// </summary>
    private static float[] CreateSaturationMatrix(double saturation)
    {
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
