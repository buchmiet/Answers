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

            timeoutValue = this._answerService.HasTimeout ? _answerService.GetTimeout() : TimeSpan.Zero; // Pobiera i resetuje timeout
            System.Threading.Tasks.Task<Answers.Answer> methodTask = method();

            Answers.Answer answer;
            while (true)
            {
                System.Threading.Tasks.Task timeoutTask = null;

                if (timeoutValue != System.TimeSpan.Zero)
                {
                    timeoutTask = System.Threading.Tasks.Task.Delay(timeoutValue, ct);
                }

                if (timeoutTask != null)
                {
                    System.Threading.Tasks.Task completedTask = await System.Threading.Tasks.Task.WhenAny(methodTask, timeoutTask);

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
                    System.Boolean timeoutResponse = false;

                    if (this._answerService.HasTimeOutDialog)
                    {
                        System.String timeoutMessage = $"The operation '{action}' timed out. Do you want to retry?";

                        if (this._answerService.HasTimeOutAsyncDialog)
                        {
                            timeoutResponse = await this._answerService.AskYesNoToWaitAsync(timeoutMessage, ct);
                        }
                        else
                        {
                            timeoutResponse = this._answerService.AskYesNoToWait(timeoutMessage);
                        }

                    }

                    if (timeoutResponse)
                    {
                        // User chose to wait longer; create a new timeoutTask
                        timeoutTask = Task.Delay(timeoutValue, ct);
                        // Continue to wait without restarting methodTask
                        continue; // Ponawiamy operację
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

