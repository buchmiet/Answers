using System;
using System.Collections.Generic;
using System.Text;

namespace Answers.AnswerService
{
    public class AnswerServiceState(
        bool hasYesNoDialog,
        bool hasYesNoAsyncDialog,
        bool hasTimeOutDialog,
        bool hasTimeOutAsyncDialog,
        bool hasLogger)
    {
        public TimeSpan TimeOut { get; set; } = TimeSpan.Zero;
        public bool HasTimeOut => TimeOut != TimeSpan.Zero;

        public bool HasYesNoDialog { get; private set; } = hasYesNoDialog;
        public bool HasYesNoAsyncDialog { get; private set; } = hasYesNoAsyncDialog;
        public bool HasTimeOutDialog { get; private set; } = hasTimeOutDialog;
        public bool HasTimeOutAsyncDialog { get; private set; } = hasTimeOutAsyncDialog;
        public bool HasLogger { get; private set; } = hasLogger;
    }

}
