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
      System.TimeSpan? timeout = null)
        {
            while (true)
            {
                System.Threading.Tasks.Task<Answers.Answer> methodTask = method();
                System.Threading.Tasks.Task timeoutTask = null;
                Answers.Answer answer;

                if (timeout.HasValue)
                {
                    // Create a delay task that completes after the specified timeout
                    timeoutTask = System.Threading.Tasks.Task.Delay(timeout.Value, ct);
                }

                if (timeoutTask != null)
                {
                    // Wait for either the method to complete or the timeout to occur
                    System.Threading.Tasks.Task completedTask = await System.Threading.Tasks.Task.WhenAny(methodTask, timeoutTask);

                    if (completedTask == methodTask)
                    {
                        // The method completed before the timeout
                        answer = await methodTask;
                        if (answer.IsSuccess || answer.DialogConcluded || !_answerService.HasDialog)
                        {
                            return answer;
                        }
                        // Method failed; prompt the user to retry
                        if (await _answerService.AskYesNoAsync(answer.Message, ct))
                        {
                            continue;
                        }
                        answer.ConcludeDialog();
                        return answer;
                    }
                    // The timeout occurred before the method completed
                    var message = Answers.Answer.Current?.Message ?? "Unknown task";
                    if (!_answerService.HasTimeOutDialog || !await _answerService.AskYesNoToWaitAsync(
                            "The operation {Message} timed out. Do you want to retry?", ct))
                    {
                        // Cannot prompt the user or user chose not to retry; return timed-out answer
                        answer = Answers.Answer.Prepare(message);
                        return answer.Error($"{timeout.Value.TotalSeconds} seconds elapsed");
                    }
                    // User chose to retry; loop again
                    continue;
                }
                // No timeout specified; await the method normally
                answer = await methodTask; // Let exceptions propagate if any

                if (answer.IsSuccess || answer.DialogConcluded || !_answerService.HasDialog)
                {
                    return answer;
                }

                // Method failed; prompt the user to retry
                if (await _answerService.AskYesNoAsync(answer.Message, ct))
                {
                    continue;
                }
                answer.ConcludeDialog();
                return answer;
            }
        }

    }

