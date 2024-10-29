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
            System.TimeSpan timeoutValue = _answerService.HasTimeout ? _answerService.GetTimeout() : System.TimeSpan.Zero;
            System.Threading.Tasks.Task<Answers.Answer> methodTask = method();
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            while (true)
            {
                try
                {
                    Answers.Answer methodResult = timeoutValue != System.TimeSpan.Zero ?
                        await WaitWithTimeoutAsync(methodTask, timeoutValue, ct) :
                        await methodTask;

                    Answers.Answer processedAnswer = await ProcessAnswerAsync(methodResult, ct);
                    if (processedAnswer.IsSuccess)
                    {
                        stopwatch.Stop();
                        return processedAnswer.GetValue<Answers.Answer>();
                    }

                    if (processedAnswer.DialogConcluded)
                    {
                        stopwatch.Stop();
                        return processedAnswer;
                    }

                    // If methodTask is completed (unsuccessfully), we need to restart it.
                    if (methodTask.IsCompleted)
                    {
                        methodTask = method();
                    }


                }
                catch (System.TimeoutException)
                {
                    (System.Boolean ShouldRetry, Answers.Answer Answer) timeoutResponse = await HandleTimeoutAsync(
                        methodTask, ct, callerName, callerFilePath, callerLineNumber, stopwatch);

                    if (timeoutResponse.ShouldRetry)
                    {
                        // Continue waiting for the existing methodTask.
                        continue;
                    }

                    stopwatch.Stop();
                    return timeoutResponse.Answer;
                }
                catch (System.OperationCanceledException)
                {
                    stopwatch.Stop();
                    return Answers.Answer.Prepare("Cancelled").Error("Operation canceled by user");
                }
            }


        }

        private async System.Threading.Tasks.Task<Answers.Answer> WaitWithTimeoutAsync(
            System.Threading.Tasks.Task<Answers.Answer> task,
            System.TimeSpan timeout,
            System.Threading.CancellationToken ct)
        {
            using System.Threading.CancellationTokenSource cts = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout);

            try
            {
                return await task.WaitAsync(cts.Token);
            }
            catch (System.OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new System.TimeoutException();
            }
        }


        private async System.Threading.Tasks.Task<Answers.Answer> ProcessAnswerAsync(
            Answers.Answer methodResult,
            System.Threading.CancellationToken ct)
        {
            if (methodResult.IsSuccess || methodResult.DialogConcluded || !(_answerService.HasYesNoDialog || _answerService.HasYesNoAsyncDialog))
            {
                return Answers.Answer.Prepare("Success").WithValue(methodResult);
            }

            System.Boolean userWantsToRetry = _answerService.HasYesNoAsyncDialog ?
                await _answerService.AskYesNoAsync(methodResult.Message, ct) :
                _answerService.AskYesNo(methodResult.Message, ct);

            if (userWantsToRetry)
            {
                // User chose to retry; we'll continue waiting for methodTask or restart it if it's completed.
                return Answers.Answer.Prepare("Retry").Error("User chose to retry");
            }

            methodResult.ConcludeDialog();
            return Answers.Answer.Prepare("DialogConcluded").WithValue(methodResult);
        }

        private async System.Threading.Tasks.Task<(System.Boolean ShouldRetry, Answers.Answer Answer)> HandleTimeoutAsync(
        System.Threading.Tasks.Task<Answers.Answer> methodTask,
        System.Threading.CancellationToken ct,
        System.String callerName,
        System.String callerFilePath,
        System.Int32 callerLineNumber,
        System.Diagnostics.Stopwatch stopwatch)
        {
            System.String action = $"{callerName} at {System.IO.Path.GetFileName(callerFilePath)}:{callerLineNumber}";
            System.String timeoutMessage = $"The operation '{action}' timed out. Do you want to wait?";

            using var dialogCts = new System.Threading.CancellationTokenSource();

            System.Threading.Tasks.Task<bool> dialogTask;

            if (_answerService.HasTimeOutAsyncDialog)
            {
                dialogTask = _answerService.AskYesNoToWaitAsync(timeoutMessage, dialogCts.Token, ct);
            }
            else if (_answerService.HasTimeOutDialog)
            {
                dialogTask = System.Threading.Tasks.Task.Run(() =>
                    _answerService.AskYesNoToWait(timeoutMessage, dialogCts.Token, ct), ct);
            }
            else
            {
                // No dialog available; return a timeout answer.
                return (false, Answers.Answer.Prepare("Timeout")
                    .Error($"{stopwatch.Elapsed.TotalSeconds} seconds elapsed")
                    .ConcludeDialog());
            }

            // Wait for either the methodTask to complete or the dialog response
            if (await System.Threading.Tasks.Task.WhenAny(methodTask, dialogTask) == methodTask)
            {
                // Method completed before user responded; cancel the dialog
                await dialogCts.CancelAsync();

                // Get the method result
                Answers.Answer methodResult = await methodTask;

                return (false, methodResult);
            }

            // Get the user's response
            System.Boolean userWantsToWait = await dialogTask;

            if (userWantsToWait)
            {
                // User wants to wait; continue waiting for methodTask
                return (true, null);
            }

            // User does not want to wait; return a timeout answer
            return (false, Answers.Answer.Prepare("Timeout")
                .Error("User chose not to wait")
                .ConcludeDialog());
        }

    }


