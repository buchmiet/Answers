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

        public bool DialogConcluded { get; set; } = false;
        public bool HasValueSet { get; set; } = false;
        public void ConcludeDialog() => DialogConcluded = true;
    }
}
