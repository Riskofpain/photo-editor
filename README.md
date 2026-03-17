# PhotoEditor

A high-performance, professional-grade desktop photo editor built for Windows. The application focuses on non-destructive image editing, complex adjustment pipelines, preset management, and a highly responsive canvas. 

Built with **C# 11+**, **.NET 8**, **Avalonia UI**, and **SkiaSharp** for GPU-accelerated rendering.

## 🚀 Features

* **Advanced Viewport**: Smooth, low-latency editing canvas powered by SkiaSharp. Supports dynamic zooming (mouse wheel) and panning (dragging).
* **Non-Destructive Adjustments**:
  * **Basic**: Exposure, Contrast (implemented via SkiaSharp Color Matrices).
  * **Color**: Saturation.
  * *(Architecture ready for Highlights, Shadows, Whites, Blacks, Temp, Tint, Tone Curves, and HSL)*
* **Preset System**: Save and load your favorite adjustment pipelines using a robust JSON serialization engine.
* **Export Engine**: Export your processed images to standard formats (JPEG, PNG) with custom resolution scaling.
* **Clean Architecture**: Strict separation between the `PhotoEditor.Core` (Processing, I/O, Models) and `PhotoEditor.Desktop` (Avalonia MVVM UI).

## 🛠️ Technology Stack

* **Framework**: .NET 8.0
* **UI**: Avalonia UI 11.0.10 (MVVM using ReactiveUI)
* **Graphics/Rendering**: SkiaSharp 2.88.8 
* **Package Management**: Centralized via `Directory.Packages.props`

## 📦 Prerequisites

* [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or newer
* IDE (Optional but recommended): Visual Studio 2022, JetBrains Rider, or VS Code with C# Dev Kit.

## 🏃 Getting Started

### 1. Clone the repository
```bash
git clone https://github.com/your-username/photo-editor.git
cd photo-editor
```

### 2. Build the solution
Restore dependencies and build the projects:
```bash
dotnet build
```

### 3. Run the application
Run the Avalonia desktop project directly:
```bash
dotnet run --project PhotoEditor.Desktop
```

## 🏗️ Project Structure

```text
PhotoEditor/
├── PhotoEditor.Core/              # Core business logic and image processing
│   ├── Models/                    # Data records (Presets, Adjustments)
│   ├── Processing/                # SkiaSharp rendering pipelines
│   └── Services/                  # File I/O and Export logic
│
├── PhotoEditor.Desktop/           # Avalonia UI application
│   ├── Controls/                  # Custom UI controls (SkiaViewportMain)
│   ├── ViewModels/                # ReactiveUI ViewModels
│   └── Views/                     # XAML layout definitions
│
└── Directory.Packages.props       # Centralized NuGet version configuration
```

## 🗺️ Roadmap / Future Development

- [ ] Implement advanced skeletal features: Tone Curves (RGB) and HSL sliders.
- [ ] Add background rendering pipeline for ultra-high-resolution images (>24MP).
- [ ] Connect Whites/Blacks and Highlights/Shadows to the Skia filter chain.
- [ ] Add more export options (TIFF, compression quality sliders).

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.
