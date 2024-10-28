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
        public async System.Threading.Tasks.Task<Answers.Answer> TryAsync(
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
                    System.Threading.Tasks.Task completedTask;
                    try
                    {
                        completedTask = await System.Threading.Tasks.Task.WhenAny(methodTask,
                            System.Threading.Tasks.Task.Delay(timeoutValue, ct));
                    }
                    catch (OperationCanceledException ex)
                    {
                        return Answers.Answer.Prepare("Cancelled").Error(ex.Message);
                    }

                    if (completedTask == methodTask)
                    {
                        var response = await ProcessAnswerAsync();
                        if (!response.IsSuccess)
                        {
                            // response from Task<Answer> was not successful
                            // try again
                            continue;
                        }
                        // response from Task<Answer> was successful
                        // return the value
                        return response.GetValue<Answers.Answer>();
                    }

                    // Wystąpił timeout
                    System.String action = $"{callerName} at {System.IO.Path.GetFileName(callerFilePath)}:{callerLineNumber}";

                    // timeout dialogs are implemented
                    if (this._answerService.HasTimeOutDialog || _answerService.HasTimeOutAsyncDialog)
                    {
                        System.String timeoutMessage = $"The operation '{action}' timed out. Do you want to retry?";
                        // async dialog has priority, but sync will run if async is not available
                        using System.Threading.CancellationTokenSource dialogCts = new System.Threading.CancellationTokenSource();
                        System.Threading.Tasks.Task<bool> dialogTask =
                            _answerService.HasTimeOutAsyncDialog ? this._answerService.AskYesNoToWaitAsync(timeoutMessage, dialogCts.Token, ct) :
                                System.Threading.Tasks.Task.Run(() =>
                                    this._answerService.AskYesNoToWait(timeoutMessage, dialogCts.Token, ct), ct);

                        var response = await ProcessTimeOutDialog(dialogTask, timeoutMessage, dialogCts);
                        if (!response.IsSuccess)
                        {
                            // response from Task<bool> was not successful
                            continue;
                        }

                        if (response.GetValue<Answers.Answer>() is Answers.Answer { IsSuccess: false } dialogAnswer)
                        {
                            return dialogAnswer;
                        }

                    }



                    // Użytkownik wybrał "No" lub brak dostępnych dialogów
                    answer = Answers.Answer.Prepare("Time out");
                    return answer.Error($"{timeoutValue.TotalSeconds} seconds elapsed");
                }

                // Brak określonego timeoutu


                var response2 = await ProcessAnswerAsync();
                if (!response2.IsSuccess)
                {
                    continue;
                }
                return response2.GetValue<Answers.Answer>();

            }

            async System.Threading.Tasks.Task<Answers.Answer> ProcessAnswerAsync()
            {
                Answers.Answer returnAnswer = Answers.Answer.Prepare("ProcessAnswerAsync");
                answer = await methodTask;

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

                System.Threading.Tasks.Task<bool> dialogTask, System.String timeoutMessage, System.Threading.CancellationTokenSource dialogCts)
            {
                Answers.Answer response = Answers.Answer.Prepare("ProcessAnswerAsync");
                System.Threading.Tasks.Task dialogOutcomeTask;
                try
                {
                    dialogOutcomeTask = await System.Threading.Tasks.Task.WhenAny(methodTask, dialogTask);
                }
                catch (OperationCanceledException ex)
                {
                    return Answers.Answer.Prepare("Cancelled").Error(ex.Message);
                }

                if (dialogOutcomeTask == methodTask)
                {
                    answer = await methodTask;
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


