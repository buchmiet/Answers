using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Answers
{
    public interface IAnswerService
    {
        bool HasYesNoDialog { get; }
        bool HasYesNoAsyncDialog { get; }
        bool HasTimeOutDialog { get; }
        bool HasTimeOutAsyncDialog { get; }
        bool HasTimeout { get; }
        TimeSpan GetTimeout();
        void AddDialog(IUserDialog dialog1);
        Task<bool> AskYesNoAsync(string message, CancellationToken ct);
        Task<bool> AskYesNoToWaitAsync(string message, CancellationToken localCancellationToken, CancellationToken ct);
        bool AskYesNo(string message);
        bool AskYesNoToWait(string message);
        void SetTimeout(TimeSpan timeout);
        void LogWarning(string message);
        void LogError(string message);
        void LogInfo(string message);
    }

    public class AnswerService(IUserDialog dialog, ILogger logger) : IAnswerService
    {
        private readonly object _syncRoot = new();
        private TimeSpan Timeout { get; set; }
        public void LogWarning(string message)
        {
            logger.LogWarning(message);
        }

        public void LogError(string message)
        {
            logger.LogError(message);
        }

        public void LogInfo(string message)
        {
            logger.LogInformation(message);
        }
        public TimeSpan GetTimeout()
        {
            lock (_syncRoot)
            {
                var returnValue = Timeout;
                Timeout= TimeSpan.Zero;
                return returnValue;
            }
        }

        public bool HasYesNoDialog => dialog is { IsSyncAvailable: true };
        public bool HasYesNoAsyncDialog => dialog is { IsSyncAvailable: true };
        public bool HasTimeOutDialog => dialog is { IsAsyncAvailable: true };
        public bool HasTimeOutAsyncDialog => dialog is { IsAsyncAvailable: true };

        public bool HasTimeout => Timeout != TimeSpan.Zero;
       

        public void AddDialog(IUserDialog dialog1)
        {
            Interlocked.Exchange(ref dialog, dialog1);
        }

        //public void AddTimeoutDialog(IUserDialog dialog)
        //{
        //    Interlocked.Exchange(ref _timeOutDialog, dialog);
        //}

        // Metody asynchroniczne
        public Task<bool> AskYesNoAsync(string message, CancellationToken ct)
        {
            if (dialog is not null)
            {
                return dialog.YesNoAsync(message, ct);
            }
            throw new InvalidOperationException("Dialog is not set.");
        }

        public Task<bool> AskYesNoToWaitAsync(string message, CancellationToken localCancellationToken, CancellationToken ct)
        {
            if (dialog is not null)
            {
                return dialog.ContinueTimedOutYesNoAsync(message, localCancellationToken, ct);
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
