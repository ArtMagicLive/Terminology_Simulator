namespace Terminology_Simulator.Models;

/// <summary>
/// Represents the result of a completed training session.
/// </summary>
public class SessionResult
{
    public DateTime DateTime { get; set; }

    public string ThemeName { get; set; } = string.Empty;

    public int TotalQuestions { get; set; }

    public int CorrectAnswers { get; set; }

    public double SuccessRate { get; set; }

    public SessionResult()
    {
    }
}