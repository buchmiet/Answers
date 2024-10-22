using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Answers
{
    public interface IAnswerService
    {
        bool HasDialog { get; }
        bool HasTimeOutDialog { get; }
        bool HasTimeout { get; }
        void AddYesNoDialog(IUserDialog dialog);
        Task<bool> AskYesNoAsync(string message, CancellationToken ct);
        Task<bool> AskYesNoToWaitAsync(string message, CancellationToken ct);
        void SetTimeout(TimeSpan timeout);
        
        bool AskYesNo(string message);
        bool AskYesNoToWait(string message);
        TimeSpan GetTimeout();
    }


    public class AnswerService(IUserDialog dialog, ILogger logger) : IAnswerService
    {
        private IUserDialog _timeOutDialog;
        public ILogger Logger { get; } = logger;
        private readonly object _syncRoot = new();
        private TimeSpan Timeout { get; set; }
        public TimeSpan GetTimeout()
        {
            lock (_syncRoot)
            {
                var returnValue = Timeout;
                Timeout= TimeSpan.Zero;
                return returnValue;
            }
        }

        public bool HasDialog => dialog != null;
        public bool HasTimeout => Timeout != TimeSpan.Zero;
        public bool HasTimeOutDialog => _timeOutDialog != null;

        public void AddYesNoDialog(IUserDialog dialog1)
        {
            Interlocked.Exchange(ref dialog, dialog1);
        }

        public void AddTimeoutDialog(IUserDialog dialog)
        {
            Interlocked.Exchange(ref _timeOutDialog, dialog);
        }

        // Metody asynchroniczne
        public Task<bool> AskYesNoAsync(string message, CancellationToken ct)
        {
            if (dialog is not null)
            {
                return dialog.YesNoAsync(message, ct);
            }
            throw new InvalidOperationException("Dialog is not set.");
        }

        public Task<bool> AskYesNoToWaitAsync(string message, CancellationToken ct)
        {
            if (dialog is not null)
            {
                return dialog.ContinueTimedOutYesNoAsync(message, ct);
            }
            throw new InvalidOperationException("Dialog is not set.");
        }

        // Metody synchroniczne
        public bool AskYesNo(string message)
        {
            var dialog1 = dialog;
            if (dialog1 is not null)
            {
                return dialog1.YesNo(message);
            }

            throw new InvalidOperationException("Dialog is not set.");
        }

        public bool AskYesNoToWait(string message)
        {
            var dialog1 = dialog;
            if (dialog1 is not null)
            {
                return dialog1.ContinueTimedOutYesNo(message);
            }

            throw new InvalidOperationException("Dialog is not set.");
        }

        public AnswerService(ILogger logger) : this(null, logger)
        {
        }


        public void SetTimeout(TimeSpan timeout)
        {
            lock (_syncRoot)
            {
                Timeout = timeout;
            }
        }
    }

}
