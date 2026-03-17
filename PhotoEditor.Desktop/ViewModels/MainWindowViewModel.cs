using PhotoEditor.Core.Models;
using PhotoEditor.Core.Services;
using ReactiveUI;
using SkiaSharp;
using System.IO;

namespace PhotoEditor.Desktop.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private SKBitmap? _originalImage;
    private AdjustmentParameters _parameters = new();
    
    public SKBitmap? OriginalImage
    {
        get => _originalImage;
        set => this.RaiseAndSetIfChanged(ref _originalImage, value);
    }

    public AdjustmentParameters Parameters
    {
        get => _parameters;
        set => this.RaiseAndSetIfChanged(ref _parameters, value);
    }

    public float Exposure
    {
        get => Parameters.Exposure;
        set { Parameters.Exposure = value; UpdateParameters(); }
    }
    
    public float Contrast
    {
        get => Parameters.Contrast;
        set { Parameters.Contrast = value; UpdateParameters(); }
    }

    public float Saturation
    {
        get => Parameters.Saturation;
        set { Parameters.Saturation = value; UpdateParameters(); }
    }

    private void UpdateParameters()
    {
        // Trigger a re-assignment to notify UI of complex object change and prompt render
        Parameters = new AdjustmentParameters 
        { 
            Exposure = this.Exposure,
            Contrast = this.Contrast,
            Saturation = this.Saturation
        };
    }
    
    public void LoadImage(string path)
    {
        if (File.Exists(path))
        {
            var bitmap = SKBitmap.Decode(path);
            OriginalImage = bitmap;
        }
    }
}
