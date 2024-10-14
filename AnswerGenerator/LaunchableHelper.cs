using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Trier4;

namespace AnswerGenerator
{
    public class LaunchableHelper
    {
        private async Task<Trier4.Answer> TryAsync(
      Func<Task<Trier4.Answer>> method,
      CancellationToken ct,
      TimeSpan? timeout = null)
        {
            _answerService.LogInfo($"TryAsync started, method: {method.Method.Name}, timeout: {(timeout.HasValue ? timeout.ToString() : "none")}");
            while (true)
            {
                Task<Trier4.Answer> methodTask = method();
                Task timeoutTask = null;
                Trier4.Answer answer;

                if (timeout.HasValue)
                {
                    // Create a delay task that completes after the specified timeout
                    _answerService.LogInfo($"Timeout set to {timeout.Value}");
                    timeoutTask = Task.Delay(timeout.Value, ct);
                }

                if (timeoutTask != null)
                {
                    // Wait for either the method to complete or the timeout to occur
                    Task completedTask = await Task.WhenAny(methodTask, timeoutTask);

                    if (completedTask == methodTask)
                    {
                        _answerService.LogInfo($"{method.Method.Name} finished before timeout");
                        // The method completed before the timeout
                        try
                        {
                            answer = await methodTask;
                        }
                        catch (Exception ex)
                        {
                            _answerService.LogError($"Exception in method {method.Method.Name}: {ex.Message}");
                            throw;
                        }

                        if (answer.IsSuccess || answer.DialogConcluded || !_answerService.HasDialog)
                        {
                            _answerService.LogInfo($"Method {method.Method.Name} succeeded or dialog concluded");
                            return answer;
                        }

                        // Method failed; prompt the user to retry
                        _answerService.LogWarning($"Method {method.Method.Name} failed: {answer.Message}");
                        if (await _answerService.AskYesNoAsync(answer.Message, ct))
                        {
                            _answerService.LogInfo("User chose to retry");
                            continue;
                        }

                        _answerService.LogInfo("User declined to retry, concluding dialog");
                        answer.ConcludeDialog();
                        return answer;
                    }

                    // The timeout occurred before the method completed
                    _answerService.LogWarning($"Timeout occurred during {method.Method.Name}");
                    if (!_answerService.HasTimeOutDialog || !await _answerService.AskYesNoToWaitAsync(
                            "The operation timed out. Do you want to retry?", ct))
                    {
                        // Cannot prompt the user or user chose not to retry; return timed-out answer
                        _answerService.LogWarning($"User declined to wait, returning timed-out answer");
                        return Trier4.Answer.TimedOut();
                    }

                    _answerService.LogInfo("User chose to wait and retry after timeout");
                    // User chose to retry; loop again
                    continue;
                }

                // No timeout specified; await the method normally
                try
                {
                    answer = await methodTask; // Let exceptions propagate if any
                }
                catch (Exception ex)
                {
                    _answerService.LogError($"Exception in method {method.Method.Name}: {ex.Message}");
                    throw;
                }

                if (answer.IsSuccess || answer.DialogConcluded || !_answerService.HasDialog)
                {
                    _answerService.LogInfo($"Method {method.Method.Name} succeeded or dialog concluded");
                    return answer;
                }

                // Method failed; prompt the user to retry
                _answerService.LogWarning($"Method {method.Method.Name} failed: {answer.Message}");
                if (await _answerService.AskYesNoAsync(answer.Message, ct))
                {
                    _answerService.LogInfo("User chose to retry");
                    continue;
                }

                _answerService.LogInfo("User declined to retry, concluding dialog");
                answer.ConcludeDialog();
                return answer;
            }
        }

