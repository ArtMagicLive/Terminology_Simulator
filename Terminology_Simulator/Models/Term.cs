namespace Terminology_Simulator.Models;

/// <summary>
/// Represents a term and its answer variants.
/// </summary>
public class Term
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Word { get; set; } = string.Empty;

    public List<string> Definitions { get; set; } = new();

    public Term()
    {
    }
}