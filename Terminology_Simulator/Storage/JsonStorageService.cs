using System.Net.NetworkInformation;
using System.Text.Json;
using Terminology_Simulator.Models;

namespace Terminology_Simulator.Storage;

/// <summary>
/// Stores application data in a JSON file using System.Text.Json.
/// </summary>
public class JsonStorageService : IStorageService
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonStorageService(string filePath)
    {
        _filePath = filePath;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    public AppState LoadState()
    {
        if (!File.Exists(_filePath))
        {
            return new AppState();
        }

        string json = File.ReadAllText(_filePath);
        AppState? state = JsonSerializer.Deserialize<AppState>(json, _jsonOptions);

        return EnsureInitialized(state);
    }

    public void SaveState(AppState state)
    {
        EnsureDirectoryExists(_filePath);

        string json = JsonSerializer.Serialize(state, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }

    public void ExportToTxt(string filepath, string content)
    {
        EnsureDirectoryExists(filepath);
        File.WriteAllText(filepath, content);
    }

    private static AppState EnsureInitialized(AppState? state)
    {
        if (state is null)
        {
            return new AppState();
        }

        state.TermSets ??= new List<TermSet>();
        state.History ??= new List<SessionResult>();
        state.Statistics ??= new ErrorStatistics();
        state.Statistics.ErrorCounts ??= new Dictionary<Guid, int>();

        return state;
    }

    private static void EnsureDirectoryExists(string filepath)
    {
        string? directory = Path.GetDirectoryName(filepath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}