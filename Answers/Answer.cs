using System;
using System.Threading;

namespace Answers
{

    public interface IAnswerValue
    {
        object GetValue();
    }

    public interface IAnswer
    {
        void AddValue<T>(T value);
        T GetValue<T>();
        bool IsSuccess { get; }
        bool DialogConcluded { get; set; }
        string Message { get; }
        bool HasValue { get; }
        void ConcludeDialog();
        Answer Attach(Answer answer);
        Answer Error(string message);
        string ToString();
        Answer WithValue<T>(T value);
        bool Out<T>(out T value);
    }

    public class Answer : IAnswer
    {

        private IAnswerValue _answerValue;

        public void AddValue<T>(T value)
        {
            if (!IsSuccess)
            {
                throw new InvalidOperationException("Answer is in Error state, no values can be added");
            }
            _answerValue = new AnswerAnswerValue<T>(value);
        }


        public T GetValue<T>()
        {
            if (!State.HasValueSet) throw new InvalidOperationException("Value was not set.");
            if (_answerValue is AnswerAnswerValue<T> record)
            {
                return record.GetValue();
            }
            throw new InvalidOperationException("Value is not of the correct type.");
        }

        protected AnswerState State = new();
        protected readonly MessageAggregator Messages = new();


        public bool IsSuccess => State.IsSuccess;
       

        public bool DialogConcluded
        {
            get => State.DialogConcluded;
            set => State.DialogConcluded = value;
        }

        public string Message => Messages.Message;

        public bool HasValue => _answerValue != null;

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
            State.DialogConcluded |= answer.DialogConcluded;
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
            _answerValue = new AnswerAnswerValue<T>(value);
            State.HasValueSet = true;
            return this;
        }

    

        public bool Out<T>(out T value)
        {
            if (State.HasValueSet)
            {
                if (_answerValue is AnswerAnswerValue<T> record)
                {
                    value = record.GetValue();
                    return true;
                }
            }
            throw new InvalidOperationException("Value not set.");
        }
    }
}
