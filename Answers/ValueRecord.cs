namespace Answers;

public record ValueRecord<T> : IValueRecord
{
    private readonly T _value;

    public ValueRecord(T value)
    {
        _value = value;
    }

    public T GetValue() => _value;

    object IValueRecord.GetValue() => _value; 
}