using System.Net.NetworkInformation;
using Terminology_Simulator.Services;
using Terminology_Simulator.Models;

namespace Terminology_Simulator.Services;

/// <summary>
/// Creates configured training sessions from available term sets.
/// </summary>
public class TrainingService
{
    private const int MaxQuestionCount = 20;
    private readonly TermManager _termManager;

    public TrainingService(TermManager termManager)
    {
        _termManager = termManager;
    }

    public TrainingSession StartSession(List<Guid> setIds, int questionCount, bool mixed)
    {
        List<TermSet> selectedSets = SelectSets(setIds, mixed);
        int questionsToTake = Math.Clamp(questionCount, 0, MaxQuestionCount);

        List<Term> questions = selectedSets
            .SelectMany(termSet => termSet.Terms)
            .OrderBy(_ => Random.Shared.Next())
            .Take(questionsToTake)
            .ToList();

        return new TrainingSession(questions);
    }

    public void SaveSessionResult(SessionResult result, Dictionary<Guid, bool> sessionErrors)
    {
        AppState state = _termManager.State;
        state.History.Add(result);

        foreach (KeyValuePair<Guid, bool> sessionError in sessionErrors)
        {
            if (!sessionError.Value)
            {
                continue;
            }

            if (!state.Statistics.ErrorCounts.ContainsKey(sessionError.Key))
            {
                state.Statistics.ErrorCounts[sessionError.Key] = 0;
            }

            state.Statistics.ErrorCounts[sessionError.Key]++;
        }

        _termManager.SaveChanges();
    }

    private List<TermSet> SelectSets(List<Guid> setIds, bool mixed)
    {
        if (mixed && setIds.Count == 0)
        {
            return _termManager.TermSets.ToList();
        }

        if (!mixed && setIds.Count > 0)
        {
            TermSet? singleSet = _termManager.TermSets
                .FirstOrDefault(termSet => termSet.Id == setIds[0]);

            return singleSet is null ? new List<TermSet>() : new List<TermSet> { singleSet };
        }

        HashSet<Guid> selectedIds = setIds.ToHashSet();

        return _termManager.TermSets
            .Where(termSet => selectedIds.Contains(termSet.Id))
            .ToList();
    }
}