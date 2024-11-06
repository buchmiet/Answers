using System;
using System.Collections.Generic;
using System.Text;
using Answers.Dialogs;
using Microsoft.Extensions.Logging;

namespace Answers
{
    public class AnswerServiceState(IUserDialog dialog,ILogger logger)
    {
        public TimeSpan TimeOut { get; set; } = TimeSpan.Zero;
        public bool HasTimeOut => TimeOut != TimeSpan.Zero;
        public bool HasYesNoDialog = dialog?.HasYesNo ?? false;
        public bool HasYesNoAsyncDialog =dialog?.HasAsyncYesNo??false;
        public bool HasTimeOutDialog = dialog?.HasTimeoutDialog ?? false;
        public bool HasTimeOutAsyncDialog =dialog?.HasAsyncTimeoutDialog ?? false;
        public bool HasLogger =logger is not null;
    }

}
