﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Answers
{
    public interface IAnswer
    {
        bool IsSuccess { get; }
        bool IsTimedOut { get; }
        bool DialogConcluded { get; }
        string Message { get; }
        void ConcludeDialog();
    }
}