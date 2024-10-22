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
   System.Threading.CancellationToken ct)
        {
            System.TimeSpan timeoutValue = System.TimeSpan.Zero;
            if (_answerService.HasTimeout)
            {
                timeoutValue = _answerService.GetTimeout();
            }
            while (true)
            {

                System.Threading.Tasks.Task<Answers.Answer> methodTask = method();
                System.Threading.Tasks.Task timeoutTask = null;
                Answers.Answer answer;


                if (timeoutValue != TimeSpan.Zero)
                {
                    timeoutTask = System.Threading.Tasks.Task.Delay(timeoutValue, ct);
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
                    if (!_answerService.HasTimeOutDialog || !await _answerService.AskYesNoToWaitAsync(
                            $"The operation timed out. Do you want to retry?", ct))
                    {
                        answer = Answers.Answer.Prepare("Time out");
                        return answer.Error($"{timeoutValue.TotalSeconds} seconds elapsed");
                    }
                    _answerService.SetTimeout(timeoutValue);
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


    }

