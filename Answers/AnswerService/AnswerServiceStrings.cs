using System;
using System.Collections.Generic;
using System.Text;

namespace Answers
{
    public class AnswerServiceStrings
    {
        public string TimeoutMessage { get; set; } = "The operation '{0}' timed out. Do you want to retry?";
        public string CancelMessage { get; set; } = "Operation canceled by user";
        public string TimeoutError { get; set; } = "User wishes not to wait";
        public string TimeoutElapsedMessage { get; set; } = "{0} seconds elapsed";
        public string CallerMessageFormat { get; set; } = "{0} at {1}:{2}";
        public string TimeOutText { get; set; } = "Time out";
        public string CancelledText { get; set; } = "Cancelled";
        public string ErrorMessageFormat { get; set; } = "Error in {0} at {1}:{2}";
        public string WarningMessageFormat { get; set; } = "Timeout in {0} at {1}:{2} - {3}";
        public string UserCancelledMessageFormat { get; set; } = "Operation cancelled by user in {0} at {1}:{2}";
    }


}
