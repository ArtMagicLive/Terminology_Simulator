using System.Net.NetworkInformation;
using Terminology_Simulator.Models;

namespace Terminology_Simulator.Storage;

/// <summary>
/// Defines operations for loading, saving, and exporting application data.
/// </summary>
public interface IStorageService
{
    AppState LoadState();

    void SaveState(AppState state);

    void ExportToTxt(string filepath, string content);
}