using Answers.Dialogs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Answers.AnswerService
{
    public partial class AnswerService
    {
        private const string DialogNotSetText = "Dialog is not set.";
        public AnswerServiceStrings Strings { get; } = new();
        private readonly object _syncRoot = new();
        private AnswerServiceState _state;
        private IUserDialog _dialog;
        private readonly ILogger _logger;
        public bool HasTimeout => _state.HasTimeOut;
        public bool HasYesNoDialog => _state.HasYesNoDialog;
        public bool HasYesNoAsyncDialog => _state.HasYesNoAsyncDialog;
        public bool HasTimeOutDialog => _state.HasTimeOutDialog;
        public bool HasTimeOutAsyncDialog => _state.HasTimeOutAsyncDialog;
        public bool HasLogger => _state.HasLogger;
    }
}
