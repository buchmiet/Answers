using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Answers;

namespace AnswerGenerator
{
    public class TryAsyncClass
    {
        private async System.Threading.Tasks.Task<Answers.Answer> TryAsync(
      System.Func<System.Threading.Tasks.Task<Answers.Answer>> method,
      System.Threading.CancellationToken ct,
      [System.Runtime.CompilerServices.CallerMemberName] System.String callerName = "",
      [System.Runtime.CompilerServices.CallerFilePath] System.String callerFilePath = "",
      [System.Runtime.CompilerServices.CallerLineNumber] System.Int32 callerLineNumber = 0)
        {
            System.TimeSpan timeoutValue;

            timeoutValue = this._answerService.HasTimeout ? _answerService.GetTimeout() : System.TimeSpan.Zero; // Pobiera i resetuje timeout
            System.Threading.Tasks.Task<Answers.Answer> methodTask = method();

            Answers.Answer answer;
            while (true)
            {

                //  if (timeoutTask != null)
                if (timeoutValue != System.TimeSpan.Zero)
                {
                    System.Threading.Tasks.Task completedTask = await System.Threading.Tasks.Task.WhenAny(methodTask, System.Threading.Tasks.Task.Delay(timeoutValue, ct));

                    if (completedTask == methodTask)
                    {
                        answer = await methodTask;

                        if (answer.IsSuccess || answer.DialogConcluded || !(this._answerService.HasYesNoDialog || _answerService.HasYesNoAsyncDialog))
                        {
                            return answer;
                        }

                        bool yesNoResponse;

                        if (this._answerService.HasYesNoAsyncDialog)
                        {
                            yesNoResponse = await this._answerService.AskYesNoAsync(answer.Message, ct);
                        }
                        else
                        {
                            yesNoResponse = this._answerService.AskYesNo(answer.Message);
                        }


                        if (yesNoResponse)
                        {
                            continue; // Użytkownik wybrał "Yes", ponawiamy operację
                        }

                        answer.ConcludeDialog();
                        return answer; // Użytkownik wybrał "No", kończymy
                    }

                    // Wystąpił timeout
                    System.String action = $"{callerName} at {System.IO.Path.GetFileName(callerFilePath)}:{callerLineNumber}";


                    if (this._answerService.HasTimeOutDialog || _answerService.HasTimeOutAsyncDialog)
                    {
                        System.String timeoutMessage = $"The operation '{action}' timed out. Do you want to retry?";
                        // async dialog has priority
                        if (this._answerService.HasTimeOutAsyncDialog)
                        {
                            using System.Threading.CancellationTokenSource dialogCts = new System.Threading.CancellationTokenSource();
                            System.Threading.Tasks.Task dialogTask = this._answerService.AskYesNoToWaitAsync(timeoutMessage, dialogCts.Token, ct);
                            System.Threading.Tasks.Task dialogOutcomeTask = await System.Threading.Tasks.Task.WhenAny(methodTask, dialogTask);
                            if (dialogOutcomeTask == methodTask)
                            {
                                await dialogCts.CancelAsync();
                                answer = await methodTask;
                                return answer;
                            }
                            continue;
                        }
                        else
                        {
                            using System.Threading.CancellationTokenSource dialogCts = new System.Threading.CancellationTokenSource();

                            // Uruchomienie synchronicznego dialogu w osobnym wątku
                            System.Threading.Tasks.Task<bool> dialogTask = System.Threading.Tasks.Task.Run(() =>
                                this._answerService.AskYesNoToWait(timeoutMessage, dialogCts.Token, ct), dialogCts.Token);

                            System.Threading.Tasks.Task dialogOutcomeTask = await System.Threading.Tasks.Task.WhenAny(methodTask, dialogTask);

                            if (dialogOutcomeTask == methodTask)
                            {
                                // Anulowanie dialogu, jeśli operacja została zakończona
                                await dialogCts.CancelAsync();
                                answer = await methodTask;
                                return answer;
                            }

                            // Kontynuowanie pętli lub operacji w przypadku, gdy dialog oczekuje na użytkownika
                            continue;
                        }
                    }



                    // Użytkownik wybrał "No" lub brak dostępnych dialogów
                    answer = Answers.Answer.Prepare("Time out");
                    return answer.Error($"{timeoutValue.TotalSeconds} seconds elapsed");
                }

                // Brak określonego timeoutu
                answer = await methodTask;

                if (answer.IsSuccess || answer.DialogConcluded || !(this._answerService.HasYesNoDialog || _answerService.HasYesNoAsyncDialog))
                {
                    return answer;
                }

                System.Boolean userResponse;

                if (this._answerService.HasYesNoAsyncDialog)
                {
                    userResponse = await this._answerService.AskYesNoAsync(answer.Message, ct);
                }
                else
                {
                    userResponse = this._answerService.AskYesNo(answer.Message);
                }

                if (userResponse)
                {
                    methodTask = method();
                    continue; // Użytkownik wybrał "Yes", ponawiamy operację
                }

                answer.ConcludeDialog();
                return answer; // Użytkownik wybrał "No", kończymy
            }
        }

        //public void LogDetailedInfo(
        //    [System.Runtime.CompilerServices.CallerMemberName] string callerName = "",
        //    [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
        //    [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0,
        //    [System.Runtime.CompilerServices.CallerArgumentExpression("callerName")] string callerExpression = "")
        //{
        //    Console.WriteLine("Metoda została wywołana przez:");
        //    Console.WriteLine($"- Nazwa metody: {callerName}");
        //    Console.WriteLine($"- Ścieżka pliku: {callerFilePath}");
        //    Console.WriteLine($"- Numer linii: {callerLineNumber}");
        //    Console.WriteLine($"- Wyrażenie argumentu: {callerExpression}");
        //}
    }

