using System.Net.NetworkInformation;
using System.Text;
using Terminology_Simulator.Models;
using Terminology_Simulator.Storage;

namespace Terminology_Simulator.Services;

/// <summary>
/// Manages term sets, terms, and answer variants in the current application state.
/// </summary>
public class TermManager
{
    private readonly IStorageService _storage;
    private readonly AppState _state;

    public TermManager(IStorageService storage)
    {
        _storage = storage;
        _state = _storage.LoadState();
    }

    public IReadOnlyList<TermSet> TermSets => _state.TermSets;

    public AppState State => _state;

    public TermSet CreateSet(string name)
    {
        TermSet termSet = new()
        {
            Name = NormalizeRequiredText(name, nameof(name))
        };

        _state.TermSets.Add(termSet);
        SaveChanges();

        return termSet;
    }

    public bool DeleteSet(Guid setId)
    {
        TermSet? termSet = FindSet(setId);

        if (termSet is null)
        {
            return false;
        }

        RemoveStatisticsForTerms(termSet.Terms.Select(term => term.Id));
        _state.TermSets.Remove(termSet);
        SaveChanges();

        return true;
    }

    public bool RenameSet(Guid setId, string newName)
    {
        TermSet? termSet = FindSet(setId);

        if (termSet is null)
        {
            return false;
        }

        termSet.Name = NormalizeRequiredText(newName, nameof(newName));
        SaveChanges();

        return true;
    }

    public List<KeyValuePair<Term, TermSet>> SearchTerms(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<KeyValuePair<Term, TermSet>>();
        }

        string normalizedQuery = query.Trim();
        List<KeyValuePair<Term, TermSet>> results = new();

        foreach (TermSet termSet in _state.TermSets)
        {
            IEnumerable<Term> matchingTerms = termSet.Terms
                .Where(term => term.Word.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase));

            foreach (Term term in matchingTerms)
            {
                results.Add(new KeyValuePair<Term, TermSet>(term, termSet));
            }
        }

        return results;
    }

    public void ExportSet(Guid setId, string path)
    {
        TermSet? termSet = FindSet(setId);

        if (termSet is null)
        {
            throw new ArgumentException("Term set was not found.", nameof(setId));
        }

        StringBuilder content = new();
        content.AppendLine($"Набор терминов: {termSet.Name}");
        content.AppendLine($"Id: {termSet.Id}");
        content.AppendLine($"Количество терминов: {termSet.Terms.Count}");
        content.AppendLine();

        foreach (Term term in termSet.Terms.OrderBy(term => term.Word))
        {
            AppendTermCard(content, term);
            content.AppendLine();
        }

        _storage.ExportToTxt(path, content.ToString());
    }

    public void ExportTerm(Term term, string path)
    {
        StringBuilder content = new();
        AppendTermCard(content, term);

        _storage.ExportToTxt(path, content.ToString());
    }

    public Term? AddTerm(Guid setId, string word, IEnumerable<string> definitions)
    {
        TermSet? termSet = FindSet(setId);

        if (termSet is null)
        {
            return null;
        }

        List<string> definitionList = CreateDefinitionList(definitions);

        Term term = new()
        {
            Word = NormalizeRequiredText(word, nameof(word)),
            Definitions = definitionList
        };

        termSet.Terms.Add(term);
        SaveChanges();

        return term;
    }

    public bool EditTerm(Guid setId, Guid termId, string newWord, IEnumerable<string> newDefinitions)
    {
        Term? term = FindTerm(setId, termId);

        if (term is null)
        {
            return false;
        }

        term.Word = NormalizeRequiredText(newWord, nameof(newWord)); 
        term.Definitions = CreateDefinitionList(newDefinitions);
        SaveChanges();

        return true;
    }

    public bool DeleteTerm(Guid setId, Guid termId)
    {
        TermSet? termSet = FindSet(setId);

        if (termSet is null)
        {
            return false;
        }

        Term? term = termSet.Terms.FirstOrDefault(currentTerm => currentTerm.Id == termId);

        if (term is null)
        {
            return false;
        }

        RemoveStatisticsForTerms(new[] { term.Id });
        termSet.Terms.Remove(term);
        SaveChanges();

        return true;
    }

    public bool AddDefinition(Guid setId, Guid termId, string definition)
    {
        Term? term = FindTerm(setId, termId);

        if (term is null)
        {
            return false;
        }

        string normalizedDefinition = NormalizeRequiredText(definition, nameof(definition));
        if (term.Definitions.Contains(normalizedDefinition, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        term.Definitions.Add(normalizedDefinition);
        SaveChanges();

        return true;
    }

    public bool RemoveDefinition(Guid setId, Guid termId, string definition)
    {
        Term? term = FindTerm(setId, termId);

        if (term is null)
        {
            return false;
        }

        string normalizedDefinition = NormalizeRequiredText(definition, nameof(definition));
        string? existingDefinition = term.Definitions
            .FirstOrDefault(currentDefinition =>
                string.Equals(currentDefinition, normalizedDefinition, StringComparison.OrdinalIgnoreCase));
        if (existingDefinition is null)
        {
            return false;
        }

        if (term.Definitions.Count <= 1)
        {
            return false;
        }

        term.Definitions.Remove(existingDefinition);
        SaveChanges();

        return true;
    }

    private static void AppendTermCard(StringBuilder content, Term term)
    {
        content.AppendLine($"Термин: {term.Word}");
        content.AppendLine($"Id: {term.Id}");
        content.AppendLine("Варианты ответа:");

        for (int i = 0; i < term.Definitions.Count; i++)
        {
            content.AppendLine($"{i + 1}. {term.Definitions[i]}");
        }
    }

    private static List<string> CreateDefinitionList(IEnumerable<string> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        List<string> definitionList = definitions
            .Select(definition => NormalizeRequiredText(definition, nameof(definitions)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (definitionList.Count == 0)
        {
            throw new ArgumentException("A term must have at least one definition.", nameof(definitions));
        }

        return definitionList;
    }
    private static string NormalizeRequiredText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value.Trim();
    }

    private void RemoveStatisticsForTerms(IEnumerable<Guid> termIds)
    {
        foreach (Guid termId in termIds.ToList())
        {
            _state.Statistics.ErrorCounts.Remove(termId);
        }
    }

    private TermSet? FindSet(Guid setId)
    {
        return _state.TermSets.FirstOrDefault(termSet => termSet.Id == setId);
    }

    private Term? FindTerm(Guid setId, Guid termId)
    {
        return FindSet(setId)?.Terms.FirstOrDefault(term => term.Id == termId);
    }

    public void SaveChanges()
    {
        _storage.SaveState(_state);
    }
}