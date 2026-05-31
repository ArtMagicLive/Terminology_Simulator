using Terminology_Simulator.Models;

namespace Terminology_Simulator.Services;

/// <summary>
/// Represents one in-memory training session without persisting results.
/// </summary>
public class TrainingSession
{
    private readonly List<Term> _questions;
    private readonly List<Guid> _errorTermIds = new();
    private int _currentIndex;

    public int TotalQuestions => _questions.Count;

    public int CorrectCount { get; private set; }

    public bool IsFinished => _currentIndex >= _questions.Count;

    public IReadOnlyList<Guid> ErrorTermIds => _errorTermIds;

    public TrainingSession(List<Term> terms)
    {
        _questions = terms.OrderBy(_ => Random.Shared.Next()).ToList();
        _currentIndex = 0;
        CorrectCount = 0;
    }

    public Term GetCurrentQuestion()
    {
        if (IsFinished)
        {
            throw new InvalidOperationException("Training session is already finished.");
        }

        return _questions[_currentIndex];
    }

    public bool SubmitAnswer(string answer)
    {
        Term currentQuestion = GetCurrentQuestion();
        bool isCorrect = currentQuestion.Definitions.Any(definition =>
            string.Equals(definition, answer, StringComparison.OrdinalIgnoreCase));

        if (isCorrect)
        {
            CorrectCount++;
        }
        else
        {
            _errorTermIds.Add(currentQuestion.Id);
        }

        _currentIndex++;

        return isCorrect;
    }
}