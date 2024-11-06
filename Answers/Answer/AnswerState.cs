namespace Answers;

public class AnswerState
{
    public bool IsSuccess { get; set; } = true;
    public bool DialogConcluded { get; set; } = false;
    public bool HasValueSet { get; set; } = false;
    public void ConcludeDialog() => DialogConcluded = true;
}