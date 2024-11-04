using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Answers
{
    /// <summary>
    /// Stub implementujący interfejs <see cref="IUserDialog"/> na potrzeby testów jednostkowych.
    /// Pozwala symulować odpowiedzi użytkownika oraz wprowadza opóźnienia, które imitują rzeczywiste
    /// zachowanie aplikacji.
    /// </summary>
    public class UserDialogStub : IUserDialog, IDisposable
    {
        private readonly List<bool> _responses;
        private readonly List<TimeSpan> _delays;
        private int _currentResponseIndex = 0;
        private int _currentDelayIndex = 0;
        private bool _disposed = false;

        /// <summary>
        /// Inicjalizuje nową instancję klasy <see cref="UserDialogStub"/> z podaną listą odpowiedzi
        /// oraz listą czasów opóźnień.
        /// </summary>
        /// <param name="responses">Lista odpowiedzi (true lub false), które będą zwracane przez metody dialogowe.</param>
        /// <param name="delays">Lista czasów opóźnień między zwracaniem odpowiedzi przez metody dialogowe.</param>
        /// <exception cref="ArgumentException">Rzucane, gdy lista <paramref name="responses"/> lub <paramref name="delays"/> jest pusta.</exception>
        public UserDialogStub(IEnumerable<bool> responses, IEnumerable<TimeSpan> delays)
        {
            _responses = responses.ToList();
            if (_responses.Count == 0)
            {
                throw new ArgumentException("List of responses cannot be empty", nameof(responses));
            }
            _delays = delays.ToList();
            if (_delays.Count == 0)
            {
                throw new ArgumentException("List of delays cannot be empty", nameof(delays));
            }
        }

        /// <summary>
        /// Wskazuje, że metoda asynchroniczna <see cref="YesNoAsync"/> jest dostępna.
        /// </summary>
        public bool HasAsyncYesNo => true;

        /// <summary>
        /// Wskazuje, że metoda asynchroniczna <see cref="ContinueTimedOutYesNoAsync"/> z obsługą timeoutu jest dostępna.
        /// </summary>
        public bool HasAsyncTimeoutDialog => true;

        /// <summary>
        /// Wskazuje, że metoda synchroniczna <see cref="YesNo"/> jest dostępna.
        /// </summary>
        public bool HasYesNo => true;

        /// <summary>
        /// Wskazuje, że metoda synchroniczna <see cref="ContinueTimedOutYesNo"/> z obsługą timeoutu jest dostępna.
        /// </summary>
        public bool HasTimeoutDialog => true;

        /// <summary>
        /// Symuluje dialog synchroniczny z użytkownikiem, zwracając wartość odpowiedzi z listy <paramref name="responses"/>.
        /// Odpowiedzi są zwracane cyklicznie.
        /// </summary>
        /// <param name="errorMessage">Wiadomość do wyświetlenia w dialogu (symulowana).</param>
        /// <returns>Wartość odpowiedzi (true lub false) zgodnie z kolejnym elementem w liście <paramref name="responses"/>.</returns>
        /// <exception cref="ObjectDisposedException">Rzucane, gdy obiekt został wcześniej zniszczony.</exception>
        public bool YesNo(string errorMessage)
        {
            return GetNextResponse();
        }

        /// <summary>
        /// Symuluje dialog asynchroniczny z użytkownikiem, zwracając wartość odpowiedzi z listy <paramref name="responses"/>.
        /// Odpowiedzi są zwracane cyklicznie.
        /// </summary>
        /// <param name="errorMessage">Wiadomość do wyświetlenia w dialogu (symulowana).</param>
        /// <param name="ct">Token anulujący operację w przypadku anulowania zadania.</param>
        /// <returns>Asynchroniczna odpowiedź (true lub false) zgodnie z kolejnym elementem w liście <paramref name="responses"/>.</returns>
        /// <exception cref="ObjectDisposedException">Rzucane, gdy obiekt został wcześniej zniszczony.</exception>
        public Task<bool> YesNoAsync(string errorMessage, CancellationToken ct)
        {
            var response = GetNextResponse();
            return Task.FromResult(response);
        }

        /// <summary>
        /// Symuluje dialog synchroniczny z użytkownikiem z opóźnieniem, zwracając wartość odpowiedzi po upływie czasu z listy <paramref name="delays"/>.
        /// Czas oczekiwania i odpowiedzi są cykliczne. Jeśli którykolwiek z tokenów anulowania zostanie aktywowany,
        /// operacja zostaje przerwana, a obiekt jest usuwany.
        /// </summary>
        /// <param name="errorMessage">Wiadomość do wyświetlenia w dialogu (symulowana).</param>
        /// <param name="localCancellationToken">Lokalny token anulujący.</param>
        /// <param name="ct">Główny token anulujący operację.</param>
        /// <returns>Wartość odpowiedzi (true lub false) po upływie opóźnienia z listy <paramref name="delays"/>.</returns>
        /// <exception cref="OperationCanceledException">Rzucane, gdy nastąpi anulowanie operacji przez dowolny token.</exception>
        /// <exception cref="ObjectDisposedException">Rzucane, gdy obiekt został wcześniej zniszczony.</exception>
        public bool ContinueTimedOutYesNo(string errorMessage,  CancellationToken ct)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UserDialogStub));

            var delay = GetNextDelay();
            var totalMilliseconds = delay.TotalMilliseconds;
            var interval = 100; // milliseconds
            var waited = 0;

            while (waited < totalMilliseconds)
            {
                if ( ct.IsCancellationRequested)
                {
                    Dispose();
                    throw new OperationCanceledException("Operation was canceled.");
                }
                Thread.Sleep((int)Math.Min(interval, totalMilliseconds - waited));
                waited += interval;
            }
            return GetNextResponse();
        }

        /// <summary>
        /// Symuluje asynchroniczny dialog z użytkownikiem z opóźnieniem, zwracając wartość odpowiedzi po upływie czasu z listy <paramref name="delays"/>.
        /// Odpowiedzi są zwracane cyklicznie. Jeśli którykolwiek z tokenów anulowania zostanie aktywowany, operacja
        /// zostaje przerwana, a obiekt jest usuwany.
        /// </summary>
        /// <param name="errorMessage">Wiadomość do wyświetlenia w dialogu (symulowana).</param>
        /// <param name="localCancellationToken">Lokalny token anulujący.</param>
        /// <param name="ct">Główny token anulujący operację.</param>
        /// <returns>Odpowiedź (true lub false) po opóźnieniu z listy <paramref name="delays"/>.</returns>
        /// <exception cref="OperationCanceledException">Rzucane, gdy nastąpi anulowanie operacji przez dowolny token.</exception>
        /// <exception cref="ObjectDisposedException">Rzucane, gdy obiekt został wcześniej zniszczony.</exception>
        public async Task<bool> ContinueTimedOutYesNoAsync(string errorMessage, CancellationToken ct)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UserDialogStub));

            var delay = GetNextDelay();

                try
                {
                    await Task.Delay(delay, ct);
                }
                catch (TaskCanceledException)
                {
                    Dispose();
                    throw new OperationCanceledException("Operation was canceled.", ct);
                }
            return GetNextResponse();
        }

        /// <summary>
        /// Zwolnienie zasobów związanych z tą instancją <see cref="UserDialogStub"/>.
        /// Po wywołaniu tej metody wszystkie próby użycia obiektu zakończą się wyjątkiem <see cref="ObjectDisposedException"/>.
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
        }

        /// <summary>
        /// Pobiera kolejną odpowiedź z listy odpowiedzi, cyklicznie przechodząc przez listę.
        /// </summary>
        /// <returns>Wartość odpowiedzi (true lub false).</returns>
        /// <exception cref="ObjectDisposedException">Rzucane, gdy obiekt został wcześniej zniszczony.</exception>
        private bool GetNextResponse()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UserDialogStub));

            var response = _responses[_currentResponseIndex];
            _currentResponseIndex = (_currentResponseIndex + 1) % _responses.Count;
            return response;
        }

        /// <summary>
        /// Pobiera kolejny czas opóźnienia z listy opóźnień, cyklicznie przechodząc przez listę.
        /// </summary>
        /// <returns>Czas opóźnienia.</returns>
        /// <exception cref="ObjectDisposedException">Rzucane, gdy obiekt został wcześniej zniszczony.</exception>
        private TimeSpan GetNextDelay()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UserDialogStub));

            var delay = _delays[_currentDelayIndex];
            _currentDelayIndex = (_currentDelayIndex + 1) % _delays.Count;
            return delay;
        }
    }

}
