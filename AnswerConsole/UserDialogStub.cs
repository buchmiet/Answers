using Answers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnswerConsole
{
    public class UserDialogStub : IUserDialog
    {
        private readonly List<bool> _responses;
        private int _currentResponseIndex = 0;

        // Konstruktor przyjmujący listę odpowiedzi
        public UserDialogStub(IEnumerable<bool> responses)
        {
            _responses = responses.ToList();
            if (_responses.Count == 0)
            {
                throw new ArgumentException("List of responses cannot be empty");
            }
        }

        // Właściwości sprawdzające dostępność wersji async i sync
        public bool IsAsyncAvailable => true;
        public bool IsSyncAvailable => true;

        // Metoda pomocnicza do uzyskania kolejnej odpowiedzi
        private bool GetNextResponse()
        {
            var response = _responses[_currentResponseIndex];
            _currentResponseIndex = (_currentResponseIndex + 1) % _responses.Count; // Powtarzanie cyklicznie
            return response;
        }

        public Task<bool> YesNoAsync(string errorMessage, CancellationToken ct)
        {
            var response = GetNextResponse();
            Console.WriteLine($"{errorMessage}: Do you want to retry - True or false?{response}");
            return Task.FromResult(response);
        }

        public Task<bool> ContinueTimedOutYesNoAsync(string errorMessage, CancellationToken localCancellationToken, CancellationToken ct)
        {
            var response = GetNextResponse();
            Console.WriteLine($"{errorMessage}: continue - True or false?{response}");
            return Task.FromResult(response);
        }


        public bool YesNo(string errorMessage)
        {
            return GetNextResponse();
        }

        public bool ContinueTimedOutYesNo(string errorMessage)
        {
            return GetNextResponse();
        }
    }

}
