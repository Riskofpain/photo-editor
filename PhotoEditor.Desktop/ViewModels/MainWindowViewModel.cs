using PhotoEditor.Core.Models;
using ReactiveUI;
using SkiaSharp;
using System.IO;

namespace PhotoEditor.Desktop.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private SKBitmap? _originalImage;
    private AdjustmentParameters _parameters = new();
    
    private float _exposure = 0f;
    private float _contrast = 1f;
    private float _saturation = 1f;

    public SKBitmap? OriginalImage
    {
        get => _originalImage;
        set => this.RaiseAndSetIfChanged(ref _originalImage, value);
    }

    public AdjustmentParameters Parameters
    {
        get => _parameters;
        set
        {
            this.RaiseAndSetIfChanged(ref _parameters, value);
            
            // Sync individual slider properties when Parameters 
            // object is fully replaced (e.g., loading a preset)
            if (_exposure != value.Exposure)
                this.RaiseAndSetIfChanged(ref _exposure, value.Exposure, nameof(Exposure));
            if (_contrast != value.Contrast)
                this.RaiseAndSetIfChanged(ref _contrast, value.Contrast, nameof(Contrast));
            if (_saturation != value.Saturation)
                this.RaiseAndSetIfChanged(ref _saturation, value.Saturation, nameof(Saturation));
        }
    }

    public float Exposure
    {
        get => _exposure;
        set 
        { 
            this.RaiseAndSetIfChanged(ref _exposure, value);
            UpdateParameters();
        }
    }
    
    public float Contrast
    {
        get => _contrast;
        set 
        { 
            this.RaiseAndSetIfChanged(ref _contrast, value);
            UpdateParameters();
        }
    }

    public float Saturation
    {
        get => _saturation;
        set 
        { 
            this.RaiseAndSetIfChanged(ref _saturation, value);
            UpdateParameters();
        }
    }

    private void UpdateParameters()
    {
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
