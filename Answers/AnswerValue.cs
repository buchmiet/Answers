namespace Answers;

public record AnswerAnswerValue<T> : IAnswerValue
{
    private readonly T _value;

    public AnswerAnswerValue(T value)
    {
        _value = value;
    }

    public T GetValue() => _value;

    object IAnswerValue.GetValue() => _value; 
}