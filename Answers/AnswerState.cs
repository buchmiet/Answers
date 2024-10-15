using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Answers
{
    public class AnswerState
    {
        public bool IsSuccess { get; set; } = true;
        public bool IsTimedOut { get; set; } = false;
        public bool DialogConcluded { get; set; } = false;
        public bool HasValueSet { get; set; } = false;
        public void ConcludeDialog() => DialogConcluded = true;

        public void TimeItOut()
        {
            IsTimedOut = true;
            IsSuccess = false;
        } 
        public static AnswerState TimedOut() => new AnswerState { IsTimedOut = true, IsSuccess = false };
    }
}
