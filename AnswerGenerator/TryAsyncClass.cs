﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Answers;

namespace AnswerGenerator
{
    public class TryAsyncClass
    {
        private async Task<Answers.Answer> TryAsync(
     Func<System.Threading.Tasks.Task<Answers.Answer>> method,
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
                    timeoutTask = System.Threading.Tasks.Task.Delay(timeout.Value, ct);
                }

                if (timeoutTask != null)
                {
                    System.Threading.Tasks.Task completedTask = await System.Threading.Tasks.Task.WhenAny(methodTask, timeoutTask);

                    if (completedTask == methodTask)
                    {
                        answer = await methodTask;

                        if (answer.IsSuccess || answer.DialogConcluded || !_answerService.HasDialog)
                        {
                            return answer;
                        }

                        if (await _answerService.AskYesNoAsync(answer.Message, ct))
                        {
                            continue;
                        }

                        answer.ConcludeDialog();
                        return answer;
                    }

                    // Timeout occurred
                    var fullOperationName = GetFullOperationName(System.Diagnostics.Activity.Current);
                    var message = fullOperationName ?? "Unknown task";
                    if (!_answerService.HasTimeOutDialog || !await _answerService.AskYesNoToWaitAsync(
                            $"The operation {message} timed out. Do you want to retry?", ct))
                    {
                        answer = Answers.Answer.Prepare(message);
                        return answer.Error($"{timeout.Value.TotalSeconds} seconds elapsed");
                    }

                    continue;
                }

                // No timeout specified
                answer = await methodTask;

                if (answer.IsSuccess || answer.DialogConcluded || !_answerService.HasDialog)
                {
                    return answer;
                }

                if (await _answerService.AskYesNoAsync(answer.Message, ct))
                {
                    continue;
                }

                answer.ConcludeDialog();
                return answer;
            }
        }

        private string GetFullOperationName(System.Diagnostics.Activity activity)
        {
            if (activity == null) return null;

            var operationNames = new Stack<string>();
            var currentActivity = activity;

            while (currentActivity != null)
            {
                operationNames.Push(currentActivity.OperationName);
                currentActivity = currentActivity.Parent;
            }

            return string.Join(" -> ", operationNames);
        }

    }

