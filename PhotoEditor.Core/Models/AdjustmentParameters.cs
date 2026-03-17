using System.Collections.Generic;

namespace PhotoEditor.Core.Models;

public class AdjustmentParameters
{
    // All values stored as -100 to +100 (Lightroom-style)
    // Default 0 = no change
    public double Exposure { get; set; } = 0.0;
    public double Contrast { get; set; } = 0.0;
    public double Highlights { get; set; } = 0.0;
    public double Shadows { get; set; } = 0.0;
    public double Whites { get; set; } = 0.0;
    public double Blacks { get; set; } = 0.0;

    public double Temperature { get; set; } = 0.0;
    public double Tint { get; set; } = 0.0;
    public double Vibrance { get; set; } = 0.0;
    public double Saturation { get; set; } = 0.0;

    public ToneCurve ToneCurve { get; set; } = new ToneCurve();
    public List<HslAdjustment> HslAdjustments { get; set; } = new List<HslAdjustment>();
}

public class ToneCurve { /* skeletal */ }
public class HslAdjustment { /* skeletal */ }
