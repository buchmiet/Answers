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
        TimeSpan Timeout { get; }
        bool HasTimeout { get; }

        void AddYesNoDialog(IUserDialog dialog);
        Task<bool> AskYesNoAsync(string message, CancellationToken ct);
        Task<bool> AskYesNoToWaitAsync(string message, CancellationToken ct);
        void SetTimeout(TimeSpan timeout);
        void LogInfo(string message);
        void LogError(string message);
        void LogWarning(string message);
    }

    public class AnswerService : IAnswerService
    {
        private IUserDialog _dialog;
        private IUserDialog _timeOutDialog;
        private readonly ILogger _logger;
        private readonly object _syncRoot = new object();
        public TimeSpan Timeout { get; private set; }

        public bool HasDialog => _dialog != null;
        public bool HasTimeout => Timeout != TimeSpan.Zero;
        public bool HasTimeOutDialog => _timeOutDialog != null;

        public void LogInfo(string message) => _logger.LogInformation(message);
        public void LogError(string message) => _logger.LogError(message);
        public void LogWarning(string message) => _logger.LogWarning(message);

        public void AddYesNoDialog(IUserDialog dialog)
        {
            Interlocked.Exchange(ref _dialog, dialog);
        }

        public void AddTimeoutDialog(IUserDialog dialog)
        {
            Interlocked.Exchange(ref _timeOutDialog, dialog);
        }

        public Task<bool> AskYesNoAsync(string message, CancellationToken ct)
        {
            var dialog = _dialog;
            if (dialog is not null)
            {
                return dialog.YesNoAsync(message, ct);
            }

            throw new InvalidOperationException("Dialog is not set.");
        }

        public Task<bool> AskYesNoToWaitAsync(string message, CancellationToken ct)
        {
            var dialog = _dialog;
            if (dialog is not null)
            {
                return dialog.ContinueTimedOutYesNoAsync(message, ct);
            }

            throw new InvalidOperationException("Dialog is not set.");
        }

        public AnswerService(IUserDialog dialog, ILogger logger)
        {
            _dialog = dialog;
            _logger = logger;
        }

        public AnswerService()
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
