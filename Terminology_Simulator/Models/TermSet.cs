namespace Terminology_Simulator.Models;

/// <summary>
/// Represents a thematic collection of terms.
/// </summary>
public class TermSet
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public List<Term> Terms { get; set; } = new();

    public TermSet()
    {
    }
}