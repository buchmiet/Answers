using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Answers
{

   

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
        protected AnswerState State = new();
        protected readonly MessageAggregator Messages = new();
        protected IAnswerValue AnswerValue;
        public bool IsSuccess => State.IsSuccess;
        public string Message => Messages.Message;
        public bool HasValue => AnswerValue is not null;

        public bool DialogConcluded
        {
            get => State.DialogConcluded;
            set => State.DialogConcluded = value;
        }

        
        public void ConcludeDialog() => State.ConcludeDialog();
        public Answer(string action)=> Messages.AddAction(action);
        public static Answer Prepare(string action)=>new(action);
        

        public Answer Attach(Answer answer)
        {
            Messages.AddActions(answer.Messages.Actions);
            State.IsSuccess &= answer.IsSuccess;
            State.DialogConcluded |= answer.DialogConcluded;
            if (HasValue && !answer.IsSuccess)
            {
                throw new InvalidOperationException(
                    $"This object already has value ({AnswerValue.GetValue()}) of type {AnswerValue.GetType().FullName}, terefore it can not be merged with another object in an error state");
            }

            if (HasValue && answer.HasValue)
            {
                throw new InvalidOperationException($"There is already value ({AnswerValue.GetValue()}) of type {AnswerValue.GetType().FullName} assigned to this Answer object. You can not merge value {answer.AnswerValue.GetValue()} of Type {answer.AnswerValue.GetType()} from {answer.Message} with it.");
            }
            if (answer.HasValue)
            {
                AnswerValue = answer.AnswerValue;
                State.HasValueSet = true;
            }
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
            AnswerValue = new AnswerValue<T>(value);
            State.HasValueSet = true;
            return this;
        }
        public void AddValue<T>(T value)
        {
            if (!IsSuccess)
            {
                throw new InvalidOperationException("Answer is in Error state, no values can be added");
            }
            AnswerValue = new AnswerValue<T>(value);
        }

        public T GetValue<T>()
        {
            if (!State.HasValueSet) throw new InvalidOperationException("Value was not set.");
            if (AnswerValue is AnswerValue<T> record)
            {
                return record.GetValue();
            }
            throw new InvalidOperationException("Value is not of the correct type.");
        }


        public bool Out<T>(out T value)
        {
            if (State.HasValueSet && AnswerValue is AnswerValue<T> record)
            {
                value = record.GetValue();
                return true;
            }

            throw new InvalidOperationException("Value not set.");
        }

   
    }
}
