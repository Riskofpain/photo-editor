using System.Collections.Generic;

namespace PhotoEditor.Core.Models;

public class AdjustmentParameters
{
    public float Exposure { get; set; } = 0f; // Range: -5 to 5
    public float Contrast { get; set; } = 1f; // Range: 0 to 2
    public float Highlights { get; set; } = 0f; // Range: -1 to 1
    public float Shadows { get; set; } = 0f; // Range: -1 to 1
    public float Whites { get; set; } = 0f; // Range: -1 to 1
    public float Blacks { get; set; } = 0f; // Range: -1 to 1
    
    public float Temperature { get; set; } = 0f; // Range: -1 to 1
    public float Tint { get; set; } = 0f; // Range: -1 to 1
    public float Vibrance { get; set; } = 0f; // Range: -1 to 1
    public float Saturation { get; set; } = 1f; // Range: 0 to 2
    
    // Future skeleton for Advanced features
    public ToneCurve ToneCurve { get; set; } = new ToneCurve();
    public List<HslAdjustment> HslAdjustments { get; set; } = new List<HslAdjustment>();
}

public class ToneCurve { /* skeletal */ }
public class HslAdjustment { /* skeletal */ }
