using PhotoEditor.Core.Models;
using ReactiveUI;
using SkiaSharp;
using System.IO;

namespace PhotoEditor.Desktop.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private SKBitmap? _originalImage;
    private AdjustmentParameters _parameters = new();

    private double _exposure = 0.0;
    private double _brightness = 0.0;
    private double _contrast = 0.0;
    private double _saturation = 0.0;
    private bool _backgroundRemoved = false;

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
            if (_brightness != value.Brightness)
                this.RaiseAndSetIfChanged(ref _brightness, value.Brightness, nameof(Brightness));
            if (_contrast != value.Contrast)
                this.RaiseAndSetIfChanged(ref _contrast, value.Contrast, nameof(Contrast));
            if (_saturation != value.Saturation)
                this.RaiseAndSetIfChanged(ref _saturation, value.Saturation, nameof(Saturation));
        }
    }

    public double Exposure
    {
        get => _exposure;
        set { this.RaiseAndSetIfChanged(ref _exposure, value); UpdateParameters(); }
    }

    public double Brightness
    {
        get => _brightness;
        set { this.RaiseAndSetIfChanged(ref _brightness, value); UpdateParameters(); }
    }

    public double Contrast
    {
        get => _contrast;
        set { this.RaiseAndSetIfChanged(ref _contrast, value); UpdateParameters(); }
    }

    public double Saturation
    {
        get => _saturation;
        set { this.RaiseAndSetIfChanged(ref _saturation, value); UpdateParameters(); }
    }

    public bool BackgroundRemoved
    {
        get => _backgroundRemoved;
        set => this.RaiseAndSetIfChanged(ref _backgroundRemoved, value);
    }

    private void UpdateParameters()
    {
        Parameters = new AdjustmentParameters
        {
            Exposure = _exposure,
            Brightness = _brightness,
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
            BackgroundRemoved = false;
        }
    }
}
