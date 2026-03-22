using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PhotoEditor.Desktop.ViewModels;
using PhotoEditor.Core.Services;
using PhotoEditor.Core.Models;
using PhotoEditor.Core.Processing;
using SkiaSharp;
using System;
using System.Linq;

namespace PhotoEditor.Desktop.Views;

public partial class MainWindow : Window
{
    private readonly ExportService _exportService = new();
    private readonly PresetService _presetService = new();

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OnLoadImageClick(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;
        if (vm == null) return;

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Image",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
        });

        if (files.Count >= 1)
        {
            vm.LoadImage(files[0].Path.LocalPath);
        }
    }

    private async void OnExportClick(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;
        if (vm?.OriginalImage == null) return;

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export Image",
            DefaultExtension = "png",
            FileTypeChoices = new[] { FilePickerFileTypes.ImagePng, FilePickerFileTypes.ImageJpg }
        });

        if (file != null)
        {
            var rendered = await ImageProcessor.ProcessImageAsync(vm.OriginalImage, vm.Parameters);
            if (rendered != null)
            {
                var ext = file.Path.LocalPath.ToLowerInvariant();
                var format = ext.EndsWith(".png") ? SKEncodedImageFormat.Png : SKEncodedImageFormat.Jpeg;
                _exportService.ExportImage(rendered, file.Path.LocalPath, format, 95);
            }
        }
    }

    private async void OnRemoveBackgroundClick(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;
        if (vm?.OriginalImage == null) return;

        var result = await BackgroundRemovalService.RemoveBackgroundAsync(vm.OriginalImage);
        if (result != null)
        {
            vm.OriginalImage = result;
            vm.BackgroundRemoved = true;
        }
    }

    private async void OnSavePresetClick(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;
        if (vm == null) return;

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Preset",
            DefaultExtension = "json"
        });

        if (file != null)
        {
            var preset = new Preset { Name = "Custom", Parameters = vm.Parameters };
            await _presetService.SavePresetAsync(preset, file.Path.LocalPath);
        }
    }

    private async void OnLoadPresetClick(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;
        if (vm == null) return;

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Load Preset",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            var preset = await _presetService.LoadPresetAsync(files[0].Path.LocalPath);
            if (preset != null)
            {
                vm.Parameters = preset.Parameters;
            }
        }
    }
}
