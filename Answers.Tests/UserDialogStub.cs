using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Answers.Tests
{
    public class UserDialogStub : IUserDialog
    {
        private readonly bool _response;

        // Konstruktor pozwala określić, czy odpowiedź ma być true, czy false
        public UserDialogStub(bool response)
        {
            _response = response;
        }

        public Task<bool> YesNoAsync(string errorMessage, CancellationToken ct)
        {
            return Task.FromResult(_response);
        }

        public Task<bool> ContinueTimedOutYesNoAsync(string errorMessage, CancellationToken ct)
        {
            return Task.FromResult(_response);
        }

        public bool YesNo(string errorMessage)
        {
            return _response;
        }

        public bool ContinueTimedOutYesNo(string errorMessage)
        {
            return _response;
        }
    }
}
