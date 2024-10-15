using System;

namespace Answers
{
    public class Answer 
    {

        protected AnswerState State = new();
        protected readonly MessageAggregator Messages = new();


        public bool IsSuccess => State.IsSuccess;
        public bool IsTimedOut => State.IsTimedOut;

        public bool DialogConcluded
        {
            get => State.DialogConcluded;
            set => State.DialogConcluded = value;
        }

        public string Message => Messages.Message;

        public bool HasValue => _value!=null;

        public void ConcludeDialog() => State.ConcludeDialog();

        public static Answer Prepare(string action)
        {
            var answer = new Answer();
            answer.Messages.AddAction(action);
            return answer;
        }

        public Answer Attach(Answer answer)
        {
            Messages.AddActions(answer.Messages.Actions);
            State.IsSuccess &= answer.IsSuccess;
            return this;
        }

        public static Answer TimedOut() => new() { State = AnswerState.TimedOut() };

        public Answer Error(string message)
        {
            State.IsSuccess = false;
            Messages.AddAction(message);
            return this;
        }

        public override string ToString() => Message;
        private object _value;

        public Answer WithValue(object value)
        {
            _value = value;
            State.HasValueSet = true;
            return this;
        }

        public T GetValue<T>()
        {
            if (State.HasValueSet)
            {
                if (_value is T value)
                {
                    return value;
                }
                throw new InvalidOperationException($"Expected a value of type {typeof(T)}.");
            }
            throw new InvalidOperationException($"Expected a value of type {typeof(T)}.");
        }

        public bool Out<T>(out T value)
        {
            if (State.HasValueSet)
            {
                value = (T)_value;
                return IsSuccess;
            }
            throw new InvalidOperationException($"Value not set.");
        }
    }
}
