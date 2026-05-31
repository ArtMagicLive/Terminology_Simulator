namespace Terminology_Simulator.Models;

/// <summary>
/// Stores user error counts by term identifier.
/// </summary>
public class ErrorStatistics
{
    public Dictionary<Guid, int> ErrorCounts { get; set; } = new();

    public ErrorStatistics()
    {
    }

    public List<KeyValuePair<Guid, int>> GetMostDifficultTerms(int limit)
    {
        return ErrorCounts
            .OrderByDescending(errorCount => errorCount.Value)
            .Take(limit)
            .ToList();
    }
}