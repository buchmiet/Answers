using System;
using System.Threading;
using System.Threading.Tasks;
using Answers.Dialogs;
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
        AnswerServiceStrings Strings { get; }
    }

    public partial class AnswerService : IAnswerService
    {

        public TimeSpan GetTimeout()
        {
            lock (_syncRoot)
            {
                var returnValue =_state.TimeOut;
                _state.TimeOut= TimeSpan.Zero;
                return returnValue;
            }
        }

        
        public void AddDialog(IUserDialog dialog1)
        {
            if (dialog1 == null)
            {
                throw new ArgumentNullException(nameof(dialog1));
            }
            Interlocked.Exchange(ref _dialog, dialog1);
            lock (_syncRoot)
            {
                _state = new AnswerServiceState(_dialog, _logger);
            }
        }


        public AnswerService()
        {
            _state = new AnswerServiceState(null,null);
        }

        public AnswerService(ILogger logger) 
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _state = new AnswerServiceState(null,logger);
        }

        public AnswerService(IUserDialog dialog, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            _state = new AnswerServiceState(_dialog,_logger);
        }
        public AnswerService(IUserDialog dialog)
        {
            _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            _state = new AnswerServiceState(_dialog, null);
        }

        public void SetTimeout(TimeSpan timeout)
        {
            lock (_syncRoot)
            {
                _state.TimeOut = timeout;
            }
        }
    }

}
