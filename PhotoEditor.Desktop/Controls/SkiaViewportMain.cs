using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using PhotoEditor.Core.Models;
using PhotoEditor.Core.Processing;
using SkiaSharp;
using System;
using System.Threading.Tasks;

namespace PhotoEditor.Desktop.Controls;

public class SkiaViewportMain : Control
{
    private SKBitmap? _originalImage;
    private SKBitmap? _renderedImage;
    
    private double _zoom = 1.0;
    private Point _offset = new Point(0, 0);
    private Point _lastMousePosition;
    private bool _isPanning;
    private bool _isRendering;
    
    private AdjustmentParameters _currentParameters = new();

    public static readonly DirectProperty<SkiaViewportMain, SKBitmap?> OriginalImageProperty =
        AvaloniaProperty.RegisterDirect<SkiaViewportMain, SKBitmap?>(
            nameof(OriginalImage), o => o.OriginalImage, (o, v) => o.OriginalImage = v);

    public static readonly DirectProperty<SkiaViewportMain, AdjustmentParameters> ParametersProperty =
        AvaloniaProperty.RegisterDirect<SkiaViewportMain, AdjustmentParameters>(
            nameof(Parameters), o => o.Parameters, (o, v) => o.Parameters = v);

    public SKBitmap? OriginalImage
    {
        get => _originalImage;
        set
        {
            SetAndRaise(OriginalImageProperty, ref _originalImage, value);
            _zoom = 1.0;
            _offset = new Point(0, 0);
            RequestRender();
        }
    }

    public AdjustmentParameters Parameters
    {
        get => _currentParameters;
        set
        {
            SetAndRaise(ParametersProperty, ref _currentParameters, value);
            RequestRender();
        }
    }

    private void RequestRender()
    {
        if (_originalImage == null) return;
        
        // Cancel previous render entirely if it's still running
        if (_isRendering) return;
        
        _isRendering = true;
        
        // Capture parameters locally to prevent them changing mid-render
        var parametersSnapshot = new AdjustmentParameters 
        {
            Exposure = _currentParameters.Exposure,
            Contrast = _currentParameters.Contrast,
            Saturation = _currentParameters.Saturation
        };
        
        Task.Run(async () => 
        {
            try 
            {
                var newRender = await ImageProcessor.ProcessImageAsync(_originalImage, parametersSnapshot);
                
                Dispatcher.UIThread.Post(() => 
                {
                    var old = _renderedImage;
                    _renderedImage = newRender;
                    
                    InvalidateVisual();
                    
                    // We cannot dispose the old bitmap immediately because Avalonia's 
                    // render thread might still be drawing it this exact millisecond.
                    // Delaying disposal ensures the old frame clears the pipeline.
                    if (old != null)
                    {
                        Task.Delay(100).ContinueWith(_ => old.Dispose());
                    }
                    
                    _isRendering = false;
                    
                    // If parameters changed while we were rendering, trigger again
                    if (_currentParameters.Exposure != parametersSnapshot.Exposure ||
                        _currentParameters.Contrast != parametersSnapshot.Contrast ||
                        _currentParameters.Saturation != parametersSnapshot.Saturation)
                    {
                        RequestRender();
                    }
                });
            }
            catch 
            {
                Dispatcher.UIThread.Post(() => _isRendering = false);
            }
        });
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        if (_renderedImage == null) return;

        double zoomFactor = 1.1;
        double oldZoom = _zoom;
        
        if (e.Delta.Y > 0)
            _zoom *= zoomFactor;
        else
            _zoom /= zoomFactor;

        _zoom = Math.Max(0.01, Math.Min(_zoom, 100.0));

        var pointerPos = e.GetPosition(this);
        var relX = (pointerPos.X - _offset.X) / oldZoom;
        var relY = (pointerPos.Y - _offset.Y) / oldZoom;

        _offset = new Point(
            pointerPos.X - (relX * _zoom),
            pointerPos.Y - (relY * _zoom)
        );

        InvalidateVisual();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isPanning = true;
            _lastMousePosition = e.GetPosition(this);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_isPanning)
        {
            var currentPos = e.GetPosition(this);
            var delta = currentPos - _lastMousePosition;
            _offset = new Point(_offset.X + delta.X, _offset.Y + delta.Y);
            _lastMousePosition = currentPos;
            InvalidateVisual();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _isPanning = false;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        
        if (_renderedImage != null)
        {
            context.Custom(new CustomDrawOperation(Bounds, _renderedImage, _zoom, _offset));
        }
    }

    private class CustomDrawOperation : ICustomDrawOperation
    {
        private readonly Rect _bounds;
        private readonly SKBitmap _bitmap;
        private readonly double _zoom;
        private readonly Point _offset;

        public CustomDrawOperation(Rect bounds, SKBitmap bitmap, double zoom, Point offset)
        {
            _bounds = bounds;
            _bitmap = bitmap;
            _zoom = zoom;
            _offset = offset;
        }

        public Rect Bounds => _bounds;
        public void Dispose() { }
        public bool Equals(ICustomDrawOperation? other) => false;
        public bool HitTest(Point p) => _bounds.Contains(p);

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null) return;
            
            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;
            
            canvas.Save();
            
            canvas.Translate((float)_offset.X, (float)_offset.Y);
            canvas.Scale((float)_zoom);

            canvas.DrawBitmap(_bitmap, 0, 0);
            
            canvas.Restore();
        }
    }
}
