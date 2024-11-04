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
        Task<bool> AskYesNoToWaitAsync(string message,  CancellationToken ct);
        bool AskYesNo(string message);
        bool AskYesNoToWait(string message, CancellationToken ct);
        void SetTimeout(TimeSpan timeout);
        void LogWarning(string message);
        void LogError(string message);
        void LogInfo(string message);
    }

    public class AnswerService : IAnswerService
    {
        public enum DialogResponse
        {
            Continue,
            Cancel,
            DoNotWait,
            Answered,
            DoNotRepeat
        }
        private readonly object _syncRoot = new();
        private IUserDialog _dialog;
        private readonly ILogger _logger;
        private TimeSpan Timeout { get; set; }
        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }

        public void LogError(string message)
        {
            _logger.LogError(message);
        }

        public void LogInfo(string message)
        {
            _logger.LogInformation(message);
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

        public bool HasYesNoDialog => _dialog is { HasYesNo: true };
        public bool HasYesNoAsyncDialog => _dialog is { HasAsyncYesNo: true };
        public bool HasTimeOutDialog => _dialog is { HasTimeoutDialog: true };
        public bool HasTimeOutAsyncDialog => _dialog is { HasAsyncTimeoutDialog: true };
        private bool HasLogger => _logger is not null;

        public bool HasTimeout => Timeout != TimeSpan.Zero;
       

        public void AddDialog(IUserDialog dialog1)
        {
            Interlocked.Exchange(ref _dialog, dialog1);
        }

        // Metody asynchroniczne
        public Task<bool> AskYesNoAsync(string message, CancellationToken ct)
        {
            if (_dialog is not null)
            {
                return _dialog.YesNoAsync(message, ct);
            }
            throw new InvalidOperationException("Dialog is not set.");
        }

        public Task<bool> AskYesNoToWaitAsync(string message, CancellationToken ct)
        {
            if (_dialog is not null)
            {
                return _dialog.ContinueTimedOutYesNoAsync(message,  ct);
            }
            throw new InvalidOperationException("Dialog is not set.");
        }

        // Metody synchroniczne
        public bool AskYesNo(string message)
        {
            var dialog1 = _dialog;
            if (dialog1 is not null)
            {
                return dialog1.YesNo(message);
            }

            throw new InvalidOperationException("Dialog is not set.");
        }

        public bool AskYesNoToWait(string message,  CancellationToken ct)
        {
            var dialog1 = _dialog;
            if (dialog1 is not null)
            {
                return dialog1.ContinueTimedOutYesNo(message, ct);
            }

            throw new InvalidOperationException("Dialog is not set.");
        }

        public AnswerService()
        {
        }

        public AnswerService(ILogger logger) 
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public AnswerService(IUserDialog dialog, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
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
