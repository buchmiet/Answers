using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Answers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            // repeat until method returns a successful answer or dialog is concluded
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            stopwatch.Start();
            while (true)
            {
                //AnswerService has timeout set, so we need to wait for the method to complete or timeout to occur
                if (timeoutValue != System.TimeSpan.Zero)
                {
                    Answers.Answer answer;
                    try
                    {
                        answer = await method().WaitAsync(timeoutValue, ct);
                    }
                    catch (System.TimeoutException)
                    {
                        // Wystąpił timeout
                        System.String action = $"{callerName} at {System.IO.Path.GetFileName(callerFilePath)}:{callerLineNumber}";
                        // if timeout dialogs are implemented
                        if (this._answerService.HasTimeOutDialog || _answerService.HasTimeOutAsyncDialog)
                        {
                            System.String timeoutMessage = $"The operation '{action}' timed out. Do you want to retry?";
                            // async dialog has priority, but sync will run if async is not available
                            using System.Threading.CancellationTokenSource dialogCts = new System.Threading.CancellationTokenSource();
                            System.Threading.Tasks.Task<bool> dialogTask = ChooseBetweenAsyncAndNonAsyncDialogTask(timeoutMessage, dialogCts);
                            var response = await ProcessTimeOutDialog(dialogTask, timeoutMessage, dialogCts);
                            // response from Task<bool> was not successful
                            if (!response.IsSuccess)
                            {
                                // carry on waiting
                                continue;
                            }
                            // if user chose not to continue,return timeout answer without any value
                            if (response.GetValue<Answers.Answer>() is Answers.Answer { IsSuccess: false } dialogAnswer)
                            {
                                stopwatch.Stop();
                                return dialogAnswer;
                            }
                        }
                        // Użytkownik wybrał "No" lub brak dostępnych dialogów
                        return TimedOutResponse();
                    }
                    catch (System.OperationCanceledException)
                    {
                        return Answers.Answer.Prepare("Cancelled").Error("Operation canceled by user");
                    }

                    var responseReceivedWithinTimeout = await ProcessAnswerAsync();
                    if (!responseReceivedWithinTimeout.IsSuccess)
                    {
                        // response from Task<Answer> was not successful
                        // try again
                        continue;
                    }
                    // response from Task<Answer> was successful
                    // return the value
                    stopwatch.Stop();
                    return responseReceivedWithinTimeout.GetValue<Answers.Answer>();
                }
                // Brak określonego timeoutu
                var noTimeoutSetResponse = await ProcessAnswerAsync();
                if (!noTimeoutSetResponse.IsSuccess)
                {
                    continue;
                }
                stopwatch.Stop();
                return noTimeoutSetResponse.GetValue<Answers.Answer>();
            }


            Answers.Answer TimedOutResponse() => Answers.Answer.Prepare("Time out").Error($"{stopwatch.Elapsed.TotalSeconds} seconds elapsed");

            System.Threading.Tasks.Task<bool> ChooseBetweenAsyncAndNonAsyncDialogTask(string s, System.Threading.CancellationTokenSource cancellationTokenSource)
            {
                return _answerService.HasTimeOutAsyncDialog ? this._answerService.AskYesNoToWaitAsync(s, cancellationTokenSource.Token, ct) :
                    System.Threading.Tasks.Task.Run(() =>
                        this._answerService.AskYesNoToWait(s, cancellationTokenSource.Token, ct), ct);
            }

            async System.Threading.Tasks.Task<Answers.Answer> ProcessAnswerAsync()
            {
                Answers.Answer returnAnswer = Answers.Answer.Prepare("ProcessAnswerAsync");
                var answer = await methodTask;
                if (answer.IsSuccess || answer.DialogConcluded || !(this._answerService.HasYesNoDialog || _answerService.HasYesNoAsyncDialog))
                {
                    return returnAnswer.WithValue(answer);
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
                    returnAnswer.Error("Yes pressed"); // Użytkownik wybrał "Yes", ponawiamy operację
                }

                answer.ConcludeDialog();
                return returnAnswer.WithValue(answer); // Użytkownik wybrał "No", kończymy
            }

            async System.Threading.Tasks.Task<Answers.Answer> ProcessTimeOutDialog(
                System.Threading.Tasks.Task<bool> dialogTask,
                System.String timeoutMessage, System.Threading.CancellationTokenSource dialogCts)
            {
                Answers.Answer response = Answers.Answer.Prepare("ProcessAnswerAsync");
                System.Threading.Tasks.Task dialogOutcomeTask;
                try
                {
                    dialogOutcomeTask = await System.Threading.Tasks.Task.WhenAny(methodTask, dialogTask);
                }
                catch (System.OperationCanceledException ex)
                {
                    return Answers.Answer.Prepare("Cancelled").Error(ex.Message);
                }

                if (dialogOutcomeTask == methodTask)
                {
                    var answer = await methodTask;
                    await dialogCts.CancelAsync();
                    return response.WithValue(answer);
                }
                if (await dialogTask)
                {
                    return response.Error("User wishes to continue");
                }
                return response.WithValue(Answers.Answer.Prepare("Timeout").Error("User wishes not to wait").ConcludeDialog());
            }
        }


