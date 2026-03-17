using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using PhotoEditor.Core.Models;

namespace PhotoEditor.Core.Services;

public class PresetService
{
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public async Task SavePresetAsync(Preset preset, string path)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, preset, _options);
    }

    public async Task<Preset?> LoadPresetAsync(string path)
    {
        if (!File.Exists(path)) return null;
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<Preset>(stream, _options);
    }
}
