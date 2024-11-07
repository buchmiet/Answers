using System;

namespace Answers;

public partial class Answer
{
    
    public Answer ConcludeDialog()
    {
        State.ConcludeDialog();
        return this;
    }

    public Answer(string action) => Messages.AddAction(action);
    public static Answer Prepare(string action) => new(action);

    public Answer Attach(Answer answer)
    {
        Messages.AddActions(answer.Messages.Actions);
        State.IsSuccess &= answer.IsSuccess;
        State.DialogConcluded |= answer.DialogConcluded;
        if (HasValue && !answer.IsSuccess)
        {
            throw new InvalidOperationException(
                string.Format(InvalidMergeWithErrorState, AnswerValue.GetValue(), AnswerValue.GetType().FullName));
        }

        if (HasValue && answer.HasValue)
        {
            throw new InvalidOperationException(
                string.Format(InvalidMergeWithAnotherValue, AnswerValue.GetValue(), AnswerValue.GetType().FullName, answer.AnswerValue.GetValue(), answer.AnswerValue.GetType(), answer.Message));
        }

        if (!answer.HasValue) return this;
        AnswerValue = answer.AnswerValue;
        State.HasValueSet = true;
        return this;
    }

    public Answer Error(string message)
    {
        if (HasValue)
        {
            throw new InvalidOperationException(ErrorAlreadyHasValue);
        }
        if (!IsSuccess)
        {
            throw new InvalidOperationException(ErrorCanOnlyBeSetOnce);
        }
        State.IsSuccess = false;
        Messages.AddAction(message);
        return this;
    }

    public override string ToString() => Message;

    public Answer WithValue<T>(T value)
    {
        AnswerValue = new AnswerValue<T>(value);
        State.HasValueSet = true;
        return this;
    }

    public void AddValue<T>(T value)
    {
        if (!IsSuccess)
        {
            throw new InvalidOperationException(ErrorStateNoValues);
        }
        State.HasValueSet = true;
        AnswerValue = new AnswerValue<T>(value);
    }

    public T GetValue<T>()
    {
        if (!State.HasValueSet) throw new InvalidOperationException(ValueNotSet);
        if (AnswerValue is AnswerValue<T> record)
        {
            return record.GetValue();
        }
        throw new InvalidOperationException(ValueIncorrectType);
    }

    public bool Out<T>(out T value)
    {
        if (!State.HasValueSet || AnswerValue is not AnswerValue<T> record)
            throw new InvalidOperationException(ValueNotSet);
        value = record.GetValue();
        return true;
    }
}

