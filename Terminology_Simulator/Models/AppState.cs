namespace Terminology_Simulator.Models;

/// <summary>
/// Represents the complete application state for persistence.
/// </summary>
public class AppState
{
    public List<TermSet> TermSets { get; set; } = new();

    public List<SessionResult> History { get; set; } = new();

    public ErrorStatistics Statistics { get; set; } = new();

    public AppState()
    {
    }
}