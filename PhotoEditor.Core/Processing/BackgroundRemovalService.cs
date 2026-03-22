using System;
using System.Threading.Tasks;
using SkiaSharp;

namespace PhotoEditor.Core.Processing;

/// <summary>
/// Removes the background from an image using edge-based segmentation.
/// Uses a GrabCut-inspired approach with SkiaSharp:
///  1. Detect dominant background color from image borders
///  2. Build a foreground/background mask based on color distance
///  3. Refine edges with a blur pass
///  4. Apply the mask as alpha channel
/// </summary>
public class BackgroundRemovalService
{
    /// <summary>
    /// Removes the background and returns a new bitmap with transparent background (RGBA).
    /// </summary>
    public static async Task<SKBitmap?> RemoveBackgroundAsync(SKBitmap? source)
    {
        if (source == null) return null;

        return await Task.Run(() =>
        {
            try
            {
                int w = source.Width;
                int h = source.Height;

                // Step 1: Sample border pixels to estimate background color
                var bgColor = EstimateBackgroundColor(source);

                // Step 2: Build a soft mask based on color distance from background
                var mask = new byte[w * h];
                float threshold = 0.18f;  // Distance threshold for "background"
                float softEdge = 0.12f;   // Soft transition range

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        var pixel = source.GetPixel(x, y);
                        float dist = ColorDistance(pixel, bgColor);

                        if (dist < threshold)
                            mask[y * w + x] = 0; // Background
                        else if (dist < threshold + softEdge)
                        {
                            // Soft edge transition
                            float alpha = (dist - threshold) / softEdge;
                            mask[y * w + x] = (byte)(alpha * 255);
                        }
                        else
                            mask[y * w + x] = 255; // Foreground
                    }
                }

                // Step 3: Simple box blur on the mask for smoother edges
                var blurredMask = BoxBlurMask(mask, w, h, 3);

                // Step 4: Apply mask as alpha channel
                var result = new SKBitmap(new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul));

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        var pixel = source.GetPixel(x, y);
                        byte alpha = blurredMask[y * w + x];
                        result.SetPixel(x, y, new SKColor(pixel.Red, pixel.Green, pixel.Blue, alpha));
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Background Removal Error: " + ex.Message);
                return null;
            }
        });
    }

    /// <summary>
    /// Samples pixels from the image border to estimate the dominant background color.
    /// </summary>
    private static SKColor EstimateBackgroundColor(SKBitmap bmp)
    {
        int w = bmp.Width, h = bmp.Height;
        long totalR = 0, totalG = 0, totalB = 0;
        int count = 0;

        int sampleDepth = Math.Max(1, Math.Min(w, h) / 20); // Sample from ~5% of edges

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (x < sampleDepth || x >= w - sampleDepth || y < sampleDepth || y >= h - sampleDepth)
                {
                    var p = bmp.GetPixel(x, y);
                    totalR += p.Red;
                    totalG += p.Green;
                    totalB += p.Blue;
                    count++;
                }
            }
        }

        if (count == 0) return SKColors.White;

        return new SKColor(
            (byte)(totalR / count),
            (byte)(totalG / count),
            (byte)(totalB / count)
        );
    }

    /// <summary>
    /// Calculates the normalized Euclidean distance between two colors (0-1 range).
    /// </summary>
    private static float ColorDistance(SKColor a, SKColor b)
    {
        float dr = (a.Red - b.Red) / 255f;
        float dg = (a.Green - b.Green) / 255f;
        float db = (a.Blue - b.Blue) / 255f;
        return (float)Math.Sqrt(dr * dr + dg * dg + db * db);
    }

    /// <summary>
    /// Applies a simple box blur to the mask for smoother edges.
    /// </summary>
    private static byte[] BoxBlurMask(byte[] mask, int w, int h, int radius)
    {
        var result = new byte[w * h];
        int kernelSize = (2 * radius + 1) * (2 * radius + 1);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int sum = 0;
                int count = 0;

                for (int ky = -radius; ky <= radius; ky++)
                {
                    for (int kx = -radius; kx <= radius; kx++)
                    {
                        int nx = x + kx, ny = y + ky;
                        if (nx >= 0 && nx < w && ny >= 0 && ny < h)
                        {
                            sum += mask[ny * w + nx];
                            count++;
                        }
                    }
                }

                result[y * w + x] = (byte)(sum / count);
            }
        }

        return result;
    }
}
