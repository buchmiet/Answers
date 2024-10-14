using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Answers.Tests
{
    public class TestClassForTryAsync
    {
        private readonly IAnswerService _answerService;

        public TestClassForTryAsync(IAnswerService answerService)
        {
            _answerService = answerService;
        }

        public async Task<IAnswer> TryAsync(
            Func<Task<IAnswer>> method,
            CancellationToken ct,
            TimeSpan? timeout = null)
        {
            _answerService.LogInfo($"TryAsync started, method: {method.Method.Name}, timeout: {(timeout.HasValue ? timeout.ToString() : "none")}");
            while (true)
            {
                Task<IAnswer> methodTask = method();
                Task timeoutTask = null;
                IAnswer answer;

                if (timeout.HasValue)
                {
                    _answerService.LogInfo($"Timeout set to {timeout.Value}");
                    timeoutTask = Task.Delay(timeout.Value, ct);
                }

                if (timeoutTask != null)
                {
                    Task completedTask = await Task.WhenAny(methodTask, timeoutTask);

                    if (completedTask == methodTask)
                    {
                        _answerService.LogInfo($"{method.Method.Name} finished before timeout");
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

                    _answerService.LogWarning($"Timeout occurred during {method.Method.Name}");
                    if (!_answerService.HasTimeOutDialog || !await _answerService.AskYesNoToWaitAsync(
                            "The operation timed out. Do you want to retry?", ct))
                    {
                        _answerService.LogWarning($"User declined to wait, returning timed-out answer");
                        return Answer.TimedOut();
                    }

                    _answerService.LogInfo("User chose to wait and retry after timeout");
                    continue;
                }

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
    }
}
