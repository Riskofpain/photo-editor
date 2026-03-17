namespace PhotoEditor.Core.Models;

public class Preset
{
    public string Name { get; set; } = string.Empty;
    public AdjustmentParameters Parameters { get; set; } = new();
}
