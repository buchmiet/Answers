namespace Answers.Answer;

public interface IAnswerValue
{
    object GetValue();
}

public record AnswerValue<T> : IAnswerValue
{
    private readonly T _value;

    public AnswerValue(T value)=> _value = value;
    
    public T GetValue() => _value;

    object IAnswerValue.GetValue() => _value;
}