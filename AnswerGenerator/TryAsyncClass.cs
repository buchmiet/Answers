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
            var timeoutValue = _answerService.HasTimeout ? _answerService.GetTimeout() : System.TimeSpan.Zero; // Pobiera i resetuje timeout
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
                        answer = await methodTask.WaitAsync(timeoutValue, ct);
                    }
                    catch (System.TimeoutException)
                    {
                        // Wystąpił timeout
                        System.String action = string.Format(
                            _answerService.Strings.CallerMessageFormat,
                            callerName,
                            System.IO.Path.GetFileName(callerFilePath),
                            callerLineNumber
                        ); ;
                        // if timeout dialogs are implemented
                        if (_answerService.HasTimeOutDialog || _answerService.HasTimeOutAsyncDialog)
                        {
                            System.String timeoutMessage = string.Format(_answerService.Strings.TimeoutMessage, action);
                            // async dialog has priority, but sync will run if async is not available
                            using var dialogCts = new System.Threading.CancellationTokenSource();
                            using var linkedCts = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct, dialogCts.Token);
                            System.Threading.Tasks.Task<bool> dialogTask = ChooseBetweenAsyncAndNonAsyncDialogTask(timeoutMessage, linkedCts);
                            var response = await ProcessTimeOutDialog(dialogTask, dialogCts);

                            switch (response.Response)
                            {
                                case Answers.AnswerService.DialogResponse.Continue:
                                    // carry on waiting
                                    continue;
                                case Answers.AnswerService.DialogResponse.Cancel:
                                    stopwatch.Stop();
                                    return response.Answer;
                                case Answers.AnswerService.DialogResponse.DoNotWait:
                                    stopwatch.Stop();
                                    return response.Answer;
                                case Answers.AnswerService.DialogResponse.Answered:
                                    if (response.Answer.IsSuccess)
                                    {
                                        stopwatch.Stop();
                                        return response.Answer;
                                    }
                                    methodTask = method();
                                    continue;
                            }

                        }
                        // Użytkownik wybrał "No" lub brak dostępnych dialogów
                        return TimedOutResponse();
                    }
                    catch (System.OperationCanceledException)
                    {
                        return Answers.Answer.Prepare(_answerService.Strings.CancelledText).Error(_answerService.Strings.CancelMessage);
                    }

                    var responseReceivedWithinTimeout = await ProcessAnswerAsync(answer);
                    switch (responseReceivedWithinTimeout.Response)
                    {
                        case Answers.AnswerService.DialogResponse.Answered:
                            stopwatch.Stop();
                            return responseReceivedWithinTimeout.Answer;
                        case Answers.AnswerService.DialogResponse.DoNotRepeat:
                            stopwatch.Stop();
                            return responseReceivedWithinTimeout.Answer;
                        case Answers.AnswerService.DialogResponse.Continue:
                            continue;
                    }
                }
                // Brak określonego timeoutu

                var noTimeoutSetResponse = await ProcessAnswerAsync(await methodTask);
                switch (noTimeoutSetResponse.Response)
                {
                    case Answers.AnswerService.DialogResponse.Answered:
                        stopwatch.Stop();
                        return noTimeoutSetResponse.Answer;
                    case Answers.AnswerService.DialogResponse.DoNotRepeat:
                        stopwatch.Stop();
                        return noTimeoutSetResponse.Answer;
                    case Answers.AnswerService.DialogResponse.Continue:
                        continue;
                }


            }


            Answers.Answer TimedOutResponse() => Answers.Answer.Prepare(_answerService.Strings.TimeOutText).Error(string.Format(_answerService.Strings.TimeoutElapsedMessage, stopwatch.Elapsed.TotalSeconds));

            System.Threading.Tasks.Task<bool> ChooseBetweenAsyncAndNonAsyncDialogTask(string s, System.Threading.CancellationTokenSource linkedCts) =>
             _answerService.HasTimeOutAsyncDialog ? _answerService.AskYesNoToWaitAsync(s, linkedCts.Token) :
                    System.Threading.Tasks.Task.Run(() =>
                        _answerService.AskYesNoToWait(s, linkedCts.Token), ct);


            async System.Threading.Tasks.Task<(Answers.AnswerService.DialogResponse Response, Answers.Answer Answer)> ProcessAnswerAsync(Answers.Answer localAnswer)
            {
                if (localAnswer.IsSuccess || localAnswer.DialogConcluded || !(_answerService.HasYesNoDialog || _answerService.HasYesNoAsyncDialog))
                {
                    return (Answers.AnswerService.DialogResponse.Answered, localAnswer);
                }
                System.Boolean userResponse;
                if (_answerService.HasYesNoAsyncDialog)
                {
                    userResponse = await _answerService.AskYesNoAsync(localAnswer.Message, ct);
                }
                else
                {
                    userResponse = _answerService.AskYesNo(localAnswer.Message);
                }

                if (userResponse)
                {
                    methodTask = method();
                    return (Answers.AnswerService.DialogResponse.Continue, null);
                }

                localAnswer.ConcludeDialog();
                return (Answers.AnswerService.DialogResponse.DoNotRepeat, localAnswer); // Użytkownik wybrał "No", kończymy
            }


            async System.Threading.Tasks.Task<(Answers.AnswerService.DialogResponse Response, Answers.Answer Answer)> ProcessTimeOutDialog(
                System.Threading.Tasks.Task<bool> dialogTask,
                System.Threading.CancellationTokenSource dialogCts)
            {
                try
                {
                    var dialogOutcomeTask = await System.Threading.Tasks.Task.WhenAny(methodTask, dialogTask).WaitAsync(ct);

                    if (dialogOutcomeTask == methodTask)
                    {
                        var localAnswer = await methodTask;
                        await dialogCts.CancelAsync();
                        return (Answers.AnswerService.DialogResponse.Answered, localAnswer);
                    }

                    // Sprawdzamy czy dialog został zakończony przez użytkownika
                    if (await dialogTask)
                    {
                        return (Answers.AnswerService.DialogResponse.Continue, null);
                    }

                    return (Answers.AnswerService.DialogResponse.DoNotWait,
                        Answers.Answer.Prepare(_answerService.Strings.CancelledText).Error(_answerService.Strings.TimeoutError).ConcludeDialog());
                }
                catch (System.OperationCanceledException)
                {
                    return (Answers.AnswerService.DialogResponse.Cancel,
                        Answers.Answer.Prepare(_answerService.Strings.CancelMessage).Error(_answerService.Strings.CancelMessage).ConcludeDialog());
                }
            }
        }
    }




