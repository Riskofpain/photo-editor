using PhotoEditor.Core.Models;
using ReactiveUI;
using SkiaSharp;
using System.IO;

namespace PhotoEditor.Desktop.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private SKBitmap? _originalImage;
    private AdjustmentParameters _parameters = new();

    // All slider values are -100 to +100, default 0
    private double _exposure = 0.0;
    private double _contrast = 0.0;
    private double _saturation = 0.0;

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

            if (_exposure != value.Exposure)
                this.RaiseAndSetIfChanged(ref _exposure, value.Exposure, nameof(Exposure));
            if (_contrast != value.Contrast)
                this.RaiseAndSetIfChanged(ref _contrast, value.Contrast, nameof(Contrast));
            if (_saturation != value.Saturation)
                this.RaiseAndSetIfChanged(ref _saturation, value.Saturation, nameof(Saturation));
        }
    }

    public double Exposure
    {
        get => _exposure;
        set
        {
            this.RaiseAndSetIfChanged(ref _exposure, value);
            UpdateParameters();
        }
    }

    public double Contrast
    {
        get => _contrast;
        set
        {
            this.RaiseAndSetIfChanged(ref _contrast, value);
            UpdateParameters();
        }
    }

    public double Saturation
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
            Exposure = _exposure,
            Contrast = _contrast,
            Saturation = _saturation
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
