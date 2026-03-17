$ErrorActionPreference = "Stop"

Write-Host "Creating Standalone Executable for PhotoEditor..." -ForegroundColor Cyan

$sourceProject = "PhotoEditor.Desktop/PhotoEditor.Desktop.csproj"
$outputDir = "./publish"

# Clean previous publish folder if it exists
if (Test-Path $outputDir) {
    Remove-Item -Recurse -Force $outputDir
}

# Run the publish command
#  -c Release        : Optimized build
#  -r win-x64        : Target Windows 64-bit architecture
#  --self-contained  : Bundle the .NET runtime with the application
#  PublishSingleFile : Combine application files into a single .exe
#  IncludeNativeLibrariesForSelfExtract : Required for native C++ DLLs like SkiaSharp
dotnet publish $sourceProject -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $outputDir

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "=============================================" -ForegroundColor Green
    Write-Host "Build Successful!" -ForegroundColor Green
    Write-Host "The standalone executable is located in: $(Resolve-Path $outputDir)\PhotoEditor.Desktop.exe" -ForegroundColor Green
    Write-Host "=============================================" -ForegroundColor Green
} else {
    Write-Host "Build Failed. Please check the error messages above." -ForegroundColor Red
}
