using System;
using System.Threading.Tasks;
using System.Threading;

namespace Answers.AnswerService
{
    public partial class AnswerService
    {
        public Task<bool> AskYesNoAsync(string message, CancellationToken ct)
        {
            if (_dialog is not null)
            {
                return _dialog.YesNoAsync(message, ct);
            }
            throw new InvalidOperationException(DialogNotSetText);
        }

        public Task<bool> AskYesNoToWaitAsync(string message, CancellationToken ct)
        {
            if (_dialog is not null)
            {
                return _dialog.ContinueTimedOutYesNoAsync(message, ct);
            }
            throw new InvalidOperationException(DialogNotSetText);
        }

        public bool AskYesNo(string message)
        {
            var dialog1 = _dialog;
            if (dialog1 is not null)
            {
                return dialog1.YesNo(message);
            }

            throw new InvalidOperationException(DialogNotSetText);
        }

        public bool AskYesNoToWait(string message, CancellationToken ct)
        {
            var dialog1 = _dialog;
            if (dialog1 is not null)
            {
                return dialog1.ContinueTimedOutYesNo(message, ct);
            }
            throw new InvalidOperationException(DialogNotSetText);
        }
    }
}
