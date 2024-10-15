using System;

namespace Answers
{

    public interface IValueRecord
    {
        object GetValue();
    }

    public class Answer 
    {

        private IValueRecord _valueRecord;

        public void AddValue<T>(T value)
        {
            if (!IsSuccess)
            {
                throw new InvalidOperationException("Answer is in Error state, no values can be added");
            }
            _valueRecord = new ValueRecord<T>(value);
        }


        public T GetValue<T>()
        {
            if (!State.HasValueSet) throw new InvalidOperationException("Value was not set.");
            if (_valueRecord is ValueRecord<T> record)
            {
                return record.GetValue();
            }
            throw new InvalidOperationException("Value is not of the correct type.");
        }

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

        public bool HasValue => _valueRecord != null;

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

        public Answer TimeOut()
        {
            if (State.IsTimedOut)
            {
                throw new InvalidOperationException("Answer already timed out.");
            }
            if (!IsSuccess)
            {
                throw new InvalidOperationException("Answer is in error state, it can not be timed out.");
            }
            State.TimeItOut();
            return this;
        }

        public Answer Error(string message)
        {
            if (HasValue)
            {
                throw new InvalidOperationException("Answer already has value, therefore it can not be in error state ");
            }
            if (!IsSuccess)
            {
                throw new InvalidOperationException("Error can only be set once.");
            }
            State.IsSuccess = false;
            Messages.AddAction(message);
            return this;
        }

        public override string ToString() => Message;
   

        public Answer WithValue<T>(T value)
        {
            _valueRecord = new ValueRecord<T>(value);
            State.HasValueSet = true;
            return this;
        }

    

        public bool Out<T>(out T value)
        {
            if (State.HasValueSet)
            {
                if (_valueRecord is ValueRecord<T> record)
                {
                    value = record.GetValue();
                    return true;
                }
            }
            throw new InvalidOperationException("Value not set.");
        }
    }
}
