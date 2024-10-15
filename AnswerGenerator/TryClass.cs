namespace AnswerGenerator
{
    using System;

    public class TryClass
    {
        private readonly Answers.AnswerService _answerService;

        public TryClass(Answers.AnswerService answerService)
        {
            _answerService = answerService;
        }

        public Answers.Answer Try(Func<Answers.Answer> method, TimeSpan? timeout = null)
        {
            while (true)
            {
                Answers.Answer answer = null;

                if (timeout.HasValue)
                {
                    // Uruchom metodę w osobnym wątku
                    var thread = new System.Threading.Thread(() =>
                    {
                            answer = method();
                  
                    });

                    thread.Start();

                    // Czekaj na zakończenie metody lub upływ timeout
                    bool completedInTime = thread.Join(timeout.Value);

                    if (completedInTime)
                    {
                        // Metoda zakończyła się przed timeoutem
                        if (answer.IsSuccess || answer.DialogConcluded || !_answerService.HasDialog)
                        {
                            return answer;
                        }

                        // Metoda nie powiodła się; zapytaj użytkownika o ponowienie
                        bool retry = _answerService.AskYesNo(answer.Message);
                        if (retry)
                        {
                            continue;
                        }

                        answer.ConcludeDialog();
                        return answer;
                    }

                    // Timeout wystąpił przed zakończeniem metody
                    if (!_answerService.HasTimeOutDialog || !_answerService.AskYesNoToWait("The operation timed out. Do you want to retry?"))
                    {
                        // Nie można zapytać użytkownika lub użytkownik wybrał nie ponawiać
                        return Answers.Answer.TimedOut();
                    }

                    // Użytkownik wybrał ponowienie; kontynuuj pętlę
                    continue;
                }

                // Brak timeout; uruchom metodę synchronously

                    answer = method();


                if (answer.IsSuccess || answer.DialogConcluded || !_answerService.HasDialog)
                {
                    return answer;
                }

                // Metoda nie powiodła się; zapytaj użytkownika o ponowienie
                if (_answerService.AskYesNo(answer.Message))
                {
                    continue;
                }

                answer.ConcludeDialog();
                return answer;
            }
        }
    }

}
