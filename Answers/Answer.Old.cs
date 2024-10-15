//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Answers
//{
//    public class Answer : IAnswer
//    {
//        protected AnswerState State = new();
//        protected readonly MessageAggregator Messages = new();
       

//        public bool IsSuccess => State.IsSuccess;
//        public bool IsTimedOut => State.IsTimedOut;

//        public bool DialogConcluded
//        {
//            get => State.DialogConcluded;
//            set => State.DialogConcluded = value;
//        }

//        public string Message => Messages.Message;

//        public void ConcludeDialog() => State.ConcludeDialog();

//        public static Answer Prepare(string action)
//        {
//            var answer = new Answer();
//            answer.Messages.AddAction(action);
//            return answer;
//        }

//        public Answer Attach(Answer answer)
//        {
//            Messages.AddActions(answer.Messages.Actions);
//            State.IsSuccess &= answer.IsSuccess;
//            return this;
//        }

//        public static Answer TimedOut() => new Answer { State = AnswerState.TimedOut() };

//        public Answer Error(string message)
//        {
//            State.IsSuccess = false;
//            Messages.AddAction(message);
//            return this;
//        }

//        public override string ToString() => Message;
//    }
//}
