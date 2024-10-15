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
        public async Task<Answers.Answer> TryAsync(
      Func<Task<Answers.Answer>> method,
      CancellationToken ct,
      TimeSpan? timeout = null)
        {
            while (true)
            {
                Task<Answers.Answer> methodTask = method();
                Task timeoutTask = null;
                Answers.Answer answer;

                if (timeout.HasValue)
                {
                    // Create a delay task that completes after the specified timeout
                    timeoutTask = Task.Delay(timeout.Value, ct);
                }

                if (timeoutTask != null)
                {
                    // Wait for either the method to complete or the timeout to occur
                    Task completedTask = await Task.WhenAny(methodTask, timeoutTask);

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
                    if (!_answerService.HasTimeOutDialog || !await _answerService.AskYesNoToWaitAsync(
                            "The operation timed out. Do you want to retry?", ct))
                    {
                        // Cannot prompt the user or user chose not to retry; return timed-out answer
                        return Answers.Answer.TimedOut();
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

