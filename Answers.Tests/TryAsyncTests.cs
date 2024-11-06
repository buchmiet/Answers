using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Answers;
using Moq;

namespace Answers.Tests
{
    public partial class TestAnswerableClass : IAnswerable
    {
        public async Task<Answer> DoSomething(
            System.Func<System.Threading.Tasks.Task<Answers.Answer>> method,
            System.Threading.CancellationToken ct)
        {
            return await TryAsync(method, ct);
        }

    }

    public class TryAsyncTests
    {
        [Fact]
        public async Task TryAsync_IsSuccessfulExecution_NoTimeout_ReturnsIsSuccess()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var mockAnswerService = new Mock<IAnswerService>();
            mockAnswerService.Setup(x => x.GetTimeout()).Returns(TimeSpan.Zero);
            mockAnswerService.Setup(x => x.HasYesNoDialog).Returns(false);

            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            Func<Task<Answer>> method = async () =>
            {
                await Task.Delay(100, ct);
                return Answer.Prepare("IsSuccess");
            };

            // Act
            Answer result = await testClass.DoSomething(method, ct);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("IsSuccess", result.Message);
        }

        [Fact]
        public async Task TryAsync_FailedExecution_NoTimeout_NoDialog_ReturnsFailure()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var mockAnswerService = new Mock<IAnswerService>();
            mockAnswerService.Setup(x => x.GetTimeout()).Returns(TimeSpan.Zero);
            mockAnswerService.Setup(x => x.HasYesNoDialog).Returns(false);
            mockAnswerService.Setup(x => x.Strings).Returns(new AnswerServiceStrings());
            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            Func<Task<Answer>> method = async () =>
            {
                await Task.Delay(100, ct);
                return Answer.Prepare("Test answer").Error("Error occurred");
            };

            // Act
            Answer result = await testClass.DoSomething(method, ct);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Error occurred", result.Message);
        }

        [Fact]
        public async Task TryAsync_NoTimeoutAsync_MethodRunsSyncDialog()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var mockAnswerService = new Mock<IAnswerService>();
            mockAnswerService.Setup(x => x.GetTimeout()).Returns(TimeSpan.FromSeconds(2));
            mockAnswerService.Setup(x => x.HasYesNoDialog).Returns(true);
            mockAnswerService.Setup(x => x.HasTimeOutAsyncDialog).Returns(false);

            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            Func<Task<Answer>> method = async () =>
            {
                await Task.Delay(100, ct);
                return Answer.Prepare("Test answer");
            };

            // Act
            Answer result = await testClass.DoSomething(method, ct);

            // Assert
            Assert.True(result.IsSuccess);

        }


        [Fact]
        public async Task TryAsync_FailedExecution_NoTimeout_WithYesNoDialog_UserRetries_Succeeds()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var mockAnswerService = new Mock<IAnswerService>();
            mockAnswerService.Setup(x => x.GetTimeout()).Returns(TimeSpan.Zero);
            mockAnswerService.Setup(x => x.HasYesNoAsyncDialog).Returns(true);
            mockAnswerService.Setup(x => x.AskYesNoAsync(It.IsAny<string>(), ct))
                .ReturnsAsync(true); // User chooses to retry
            mockAnswerService.Setup(x => x.Strings).Returns(new AnswerServiceStrings());
            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            int attempt = 0;
            Func<Task<Answer>> method = async () =>
            {
                await Task.Delay(100, ct);
                attempt++;
                if (attempt < 2)
                {
                    return Answer.Prepare("Test answer").Error("Error occurred");
                }

                return Answer.Prepare("IsSuccess after retry");
            };

            // Act
            Answer result = await testClass.DoSomething(method, ct);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Contains("IsSuccess after retry", result.Message);
            Assert.Equal(2, attempt);
        }

        [Fact]
        public async Task TryAsync_FailedExecution_NoTimeout_WithYesNoDialog_UserDoesNotRetry_ReturnsFailure()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var mockAnswerService = new Mock<IAnswerService>();
            mockAnswerService.Setup(x => x.GetTimeout()).Returns(TimeSpan.Zero);
            mockAnswerService.Setup(x => x.HasYesNoAsyncDialog).Returns(true);
            mockAnswerService.Setup(x => x.AskYesNoAsync(It.IsAny<string>(), ct))
                .ReturnsAsync(false); // User chooses not to retry
            mockAnswerService.Setup(x => x.Strings).Returns(new AnswerServiceStrings());
            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            int attempt = 0;
            Func<Task<Answer>> method = async () =>
            {
                await Task.Delay(100, ct);
                attempt++;
                return Answer.Prepare("Test").Error("Error occurred");
            };

            // Act
            Answer result = await testClass.DoSomething(method, ct);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Error occurred", result.Message);
            Assert.Equal(1, attempt);
        }

        [Fact]
        public async Task TryAsync_IsSuccessfulExecution_WithTimeout_CompletesBeforeTimeout_ReturnsIsSuccess()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var mockAnswerService = new Mock<IAnswerService>();
            mockAnswerService.Setup(x => x.GetTimeout()).Returns(TimeSpan.FromSeconds(1));
            mockAnswerService.Setup(x => x.HasYesNoDialog).Returns(false);

            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            Func<Task<Answer>> method = async () =>
            {
                await Task.Delay(500, ct); // Completes before timeout
                return Answer.Prepare("IsSuccess");
            };

            // Act
            Answer result = await testClass.DoSomething(method, ct);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("IsSuccess", result.Message);
        }

        [Fact]
        public async Task TryAsync_ExecutionTimesOut_WithTimeoutDialog_UserContinuesWaiting_Succeeds()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var mockAnswerService = new Mock<IAnswerService>();
            mockAnswerService.SetupSequence(x => x.GetTimeout())
                .Returns(TimeSpan.FromSeconds(1))
                .Returns(TimeSpan.FromSeconds(2)); // New timeout after user chooses to wait
            mockAnswerService.Setup(x => x.HasTimeOutAsyncDialog).Returns(true);
            mockAnswerService.Setup(x => x.AskYesNoToWaitAsync(It.IsAny<string>(), ct))
                .ReturnsAsync(true); // User chooses to continue waiting

            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            Func<Task<Answer>> method = async () =>
            {
                await Task.Delay(1500, ct); // Longer than initial timeout but less than total time
                return Answer.Prepare("IsSuccess after waiting");
            };

            // Act
            Answer result = await testClass.DoSomething(method, ct);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("IsSuccess after waiting", result.Message);
        }

        [Fact]
        public async Task TryAsync_ExecutionTimesOut_WithTimeoutDialog_UserDoesNotContinue_ReturnsTimeoutError()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var mockAnswerService = new Mock<IAnswerService>();
            mockAnswerService.Setup(x => x.GetTimeout()).Returns(TimeSpan.FromSeconds(1));
            mockAnswerService.Setup(x => x.HasTimeout).Returns(true);
            mockAnswerService.Setup(x => x.HasTimeOutAsyncDialog).Returns(true);
            mockAnswerService.Setup(x => x.AskYesNoToWaitAsync(It.IsAny<string>(), ct))
                .ReturnsAsync(false); // User chooses not to continue waiting
            mockAnswerService.Setup(x => x.Strings).Returns(new AnswerServiceStrings());
            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            Func<Task<Answer>> method = async () =>
            {
                await Task.Delay(5000, ct); // Won't complete before timeout
                return Answer.Prepare("IsSuccess after waiting");
            };

            // Act
            Answer result = await testClass.DoSomething(method, ct);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(new AnswerServiceStrings().TimeoutError, result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task TryAsync_ExecutionTimesOut_NoTimeoutDialog_ReturnsTimeoutError()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var mockAnswerService = new Mock<IAnswerService>();
            mockAnswerService.Setup(x => x.GetTimeout()).Returns(TimeSpan.FromSeconds(1));
            mockAnswerService.Setup(x => x.HasTimeout).Returns(true);
            mockAnswerService.Setup(x => x.HasTimeOutAsyncDialog).Returns(false);
            mockAnswerService.Setup(x => x.Strings).Returns(new AnswerServiceStrings());
            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            Func<Task<Answer>> method = async () =>
            {
                await Task.Delay(5000, ct); // Won't complete before timeout
                return Answer.Prepare("IsSuccess after waiting");
            };

            // Act
            Answer result = await testClass.DoSomething(method, ct);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Time out", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task TryAsync_UserCancelsOperation_CancellationRequested_ReturnsCancellation()
        {
            // Arrange
            var cts = new CancellationTokenSource(155); // Cancel after 155 ms
            var ct = cts.Token;

            var mockAnswerService = new Mock<IAnswerService>();
            mockAnswerService.Setup(x => x.GetTimeout()).Returns(TimeSpan.FromSeconds(1));
            mockAnswerService.Setup(x => x.HasTimeout).Returns(true);

            // Konfiguracja lokalizacji dla stringów
            var localizedStrings = new AnswerServiceStrings
            {
                CancelMessage = "Operation canceled by user" // Ustawiamy oczekiwany komunikat
            };
            mockAnswerService.Setup(x => x.Strings).Returns(localizedStrings);

            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            Func<Task<Answer>> method = async () =>
            {
                await Task.Delay(5000, ct); // Long-running task
                return Answer.Prepare("Should not reach here");
            };

            // Act
            Answer result = await testClass.DoSomething(method, ct);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(localizedStrings.CancelMessage, result.Message, StringComparison.OrdinalIgnoreCase);
        }



        [Fact]
        public async System.Threading.Tasks.Task TryAsync_LongRunningMethod_WithTimeout_UserContinues_MethodCompletesDuringThirdPrompt()
        {
            // Arrange
            var timeout = System.TimeSpan.FromSeconds(2);
            using var cts = new System.Threading.CancellationTokenSource();
            var ct = cts.Token;

            var mockAnswerService = new Moq.Mock<IAnswerService>();

            // Set up the timeout
            mockAnswerService.Setup(x => x.GetTimeout()).Returns(timeout);
            mockAnswerService.Setup(x => x.HasTimeout).Returns(true);
            mockAnswerService.Setup(x => x.Strings).Returns(new AnswerServiceStrings());
            // Counters to keep track of prompts
            int promptCount = 0;

            // Simulate user responses
            mockAnswerService.Setup(x => x.HasTimeOutAsyncDialog).Returns(true);
            mockAnswerService
                .Setup(x => x.AskYesNoToWaitAsync(
                    Moq.It.IsAny<string>(),
                    Moq.It.IsAny<System.Threading.CancellationToken>()))
                .Returns<string, System.Threading.CancellationToken>(async (message, ct) =>
                {
                    promptCount++;
                    if (promptCount <= 2)
                    {
                        // First two responses take 200ms
                        await System.Threading.Tasks.Task.Delay(200, ct);
                        return true; // User chooses to continue
                    }

                    // Third response takes 1 second
                    await System.Threading.Tasks.Task.Delay(1000, ct);
                    return true; // User chooses to continue
                });

            // Simulate the long-running method
            var methodDuration = System.TimeSpan.FromSeconds(7);
            var methodStopwatch = new System.Diagnostics.Stopwatch();

            System.Func<System.Threading.Tasks.Task<Answer>> method = async () =>
            {
                methodStopwatch.Start();
                await System.Threading.Tasks.Task.Delay(methodDuration, ct); // Simulate long-running operation
                methodStopwatch.Stop();
                return Answer.Prepare("Success");
            };

            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            // Act
            var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();
            Answer result = await testClass.DoSomething(method, ct);
            totalStopwatch.Stop();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Success", result.Message);

            // The method should have completed during the third prompt
            // Total time should be slightly over 7 seconds due to the delays
            Assert.InRange(totalStopwatch.Elapsed.TotalSeconds, 6.5, 8);

            // The method's stopwatch should have recorded approximately 7 seconds
            Assert.InRange(methodStopwatch.Elapsed.TotalSeconds, 7, 7.5);

            // The prompt count should be 3
            Assert.Equal(3, promptCount);
        }

        [Fact]
        public async System.Threading.Tasks.Task TryAsync_MethodFailsTwiceBeforeSuccess_UserRetriesEachTime_ReturnsSuccess()
        {
            // Arrange
            using var cts = new System.Threading.CancellationTokenSource();
            var ct = cts.Token;

            var mockAnswerService = new Moq.Mock<IAnswerService>();

            // No timeout for this test
            mockAnswerService.Setup(x => x.HasTimeout).Returns(false);

            // Simulate the Yes/No dialog
            mockAnswerService.Setup(x => x.HasYesNoAsyncDialog).Returns(true);
            mockAnswerService
                .Setup(x => x.AskYesNoAsync(
                    Moq.It.IsAny<string>(),
                    Moq.It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(true); // User chooses to retry
            mockAnswerService.Setup(x => x.Strings).Returns(new AnswerServiceStrings());
            int attemptCount = 0;

            // Simulate the method failing twice before succeeding
            System.Func<System.Threading.Tasks.Task<Answer>> method = async () =>
            {
                attemptCount++;
                await System.Threading.Tasks.Task.Delay(100, ct); // Simulate some work

                if (attemptCount < 3)
                {
                    return Answer.Prepare("Failure").Error($"Method failed on attempt {attemptCount}");
                }
                else
                {
                    return Answer.Prepare("Success");
                }
            };

            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            // Act
            Answer result = await testClass.DoSomething(method, ct);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Success", result.Message);
            Assert.Equal(3, attemptCount); // Method was called three times
        }

        [Fact]
        public async System.Threading.Tasks.Task TryAsync_GlobalCancellationTokenCanceled_ReturnsCanceledResponse()
        {
            // Arrange
            var timeout = System.TimeSpan.FromSeconds(5);
            using var cts = new System.Threading.CancellationTokenSource();
            var ct = cts.Token;

            var mockAnswerService = new Moq.Mock<IAnswerService>();

            // Set up the timeout
            mockAnswerService.Setup(x => x.GetTimeout()).Returns(timeout);
            mockAnswerService.Setup(x => x.HasTimeout).Returns(true);
            mockAnswerService.Setup(x => x.Strings).Returns(new AnswerServiceStrings());
            // Simulate the method that takes a long time
            System.Func<System.Threading.Tasks.Task<Answer>> method = async () =>
            {
                await System.Threading.Tasks.Task.Delay(10000, ct); // Simulate long-running operation
                return Answer.Prepare("Success");
            };

            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            // Act
            var task = testClass.DoSomething(method, ct);

            // Cancel the global cancellation token after 1 second
            await System.Threading.Tasks.Task.Delay(1000);
            cts.Cancel();

            Answer result = await task;

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("canceled", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async System.Threading.Tasks.Task TryAsync_MethodFailsTwice_UserRetries_MethodSucceeds()
        {
            // Arrange
            using var cts = new System.Threading.CancellationTokenSource();
            var ct = cts.Token;

            var mockAnswerService = new Moq.Mock<IAnswerService>();

            // No timeout
            mockAnswerService.Setup(x => x.HasTimeout).Returns(false);

            // Simulate Yes/No dialog
            mockAnswerService.Setup(x => x.HasYesNoAsyncDialog).Returns(true);
            mockAnswerService
                .Setup(x => x.AskYesNoAsync(
                    Moq.It.IsAny<string>(),
                    Moq.It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(true); // User chooses to retry
            mockAnswerService.Setup(x => x.Strings).Returns(new AnswerServiceStrings());
            int attemptCount = 0;

            System.Func<System.Threading.Tasks.Task<Answer>> method = async () =>
            {
                attemptCount++;
                await System.Threading.Tasks.Task.Delay(100, ct);

                if (attemptCount < 3)
                {
                    return Answer.Prepare("Failure").Error($"Attempt {attemptCount} failed");
                }
                else
                {
                    return Answer.Prepare("Success");
                }
            };

            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            // Act
            Answer result = await testClass.DoSomething(method, ct);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Success", result.Message);
            Assert.Equal(3, attemptCount); // Method was attempted three times
        }

        [Fact]
        public async System.Threading.Tasks.Task TryAsync_MethodFailsWithTimeouts_UserWaits_MethodEventuallySucceeds()
        {
            // Arrange
            var timeout = System.TimeSpan.FromSeconds(1);
            using var cts = new System.Threading.CancellationTokenSource();
            var ct = cts.Token;

            var mockAnswerService = new Moq.Mock<IAnswerService>();

            // Setup timeout
            mockAnswerService.Setup(x => x.HasTimeout).Returns(true);
            mockAnswerService.Setup(x => x.GetTimeout()).Returns(timeout);
            mockAnswerService.Setup(x => x.Strings).Returns(new AnswerServiceStrings());
            // Simulate timeout dialog
            mockAnswerService.Setup(x => x.HasTimeOutAsyncDialog).Returns(true);
            mockAnswerService
                .Setup(x => x.AskYesNoToWaitAsync(
                    Moq.It.IsAny<string>(),
                    Moq.It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(true); // User chooses to wait

            int attemptCount = 0;

            System.Func<System.Threading.Tasks.Task<Answer>> method = async () =>
            {
                attemptCount++;
                if (attemptCount < 3)
                {
                    await System.Threading.Tasks.Task.Delay(3000, ct); // Exceeds timeout
                    return Answer.Prepare("Success"); // Won't reach here before timeout
                }
                else
                {
                    await System.Threading.Tasks.Task.Delay(500, ct); // Completes within timeout
                    return Answer.Prepare("Success");
                }
            };

            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            // Act
            Answer result = await testClass.DoSomething(method, ct);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Success", result.Message);
        }

        [Fact]
        public async System.Threading.Tasks.Task TryAsync_GlobalCancellationDuringUserPrompt_ReturnsCanceledResponse()
        {
            // Arrange
            var timeout = System.TimeSpan.FromSeconds(2);
            using var cts = new System.Threading.CancellationTokenSource();
            var ct = cts.Token;

            var mockAnswerService = new Moq.Mock<IAnswerService>();

            // Setup timeout
            mockAnswerService.Setup(x => x.HasTimeout).Returns(true);
            mockAnswerService.Setup(x => x.GetTimeout()).Returns(timeout);
            mockAnswerService.Setup(x => x.Strings).Returns(new AnswerServiceStrings());
            // Simulate method that will time out
            System.Func<System.Threading.Tasks.Task<Answer>> method = async () =>
            {
                await System.Threading.Tasks.Task.Delay(5000, ct); // Simulate long-running operation
                return Answer.Prepare("Success");
            };

            // Simulate timeout dialog
            mockAnswerService.Setup(x => x.HasTimeOutAsyncDialog).Returns(true);
            mockAnswerService
                .Setup(x => x.AskYesNoToWaitAsync(
                    Moq.It.IsAny<string>(),
                    Moq.It.IsAny<System.Threading.CancellationToken>()
                    ))
                .Returns<string, System.Threading.CancellationToken>(async (message, ct) =>
                {
                    // Cancel the global cancellation token after 1 second
                    await System.Threading.Tasks.Task.Delay(1000);
                    cts.Cancel();

                    // Simulate user taking time to respond
                    await System.Threading.Tasks.Task.Delay(2000, ct);
                    return true;
                });

            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            // Act
            Answer result = await testClass.DoSomething(method, ct);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("canceled", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task TryAsync_MethodFails_LogsError()
        {
            // Arrange
            var mockAnswerService = new Mock<IAnswerService>();
            mockAnswerService.Setup(x => x.Strings).Returns(new AnswerServiceStrings());
            var logMessages = new List<string>();
            mockAnswerService.Setup(x => x.LogError(It.IsAny<string>()))
                .Callback<string>(message => logMessages.Add(message));
            Func<Task<Answer>> method = () =>
            {
                return Task.FromResult(Answer.Prepare("Method failed").Error("An error occurred."));
            };

            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            // Act
            var result = await testClass.DoSomething(method, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("An error occurred.", result.Message);
            Assert.Single(logMessages);
            Assert.Contains("Error in", logMessages[0]);
     
        }


        [Fact]
        public async Task TryAsync_MethodTimesOut_LogsWarning()
        {
            // Arrange
            var timeout = TimeSpan.FromSeconds(1);
            using var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var mockAnswerService = new Mock<IAnswerService>();
            mockAnswerService.Setup(x => x.GetTimeout()).Returns(timeout);
            mockAnswerService.Setup(x => x.HasTimeout).Returns(true);
            mockAnswerService.Setup(x => x.Strings).Returns(new AnswerServiceStrings());
            mockAnswerService.Setup(x => x.HasTimeOutAsyncDialog).Returns(false); // No timeout dialogs
            var logMessages = new List<string>();
            mockAnswerService.Setup(x => x.LogWarning(It.IsAny<string>()))
                .Callback<string>(message => logMessages.Add(message));
            mockAnswerService.Setup(x => x.Strings).Returns(new AnswerServiceStrings());
            Func<Task<Answer>> method = async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
                return Answer.Prepare("Success");
            };

            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            // Act
            var result = await testClass.DoSomething(method, ct);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(new AnswerServiceStrings().TimeOutText, result.Message);
            Assert.Single(logMessages);
            Assert.Contains("Timeout in", logMessages[0]);
        }
        [Fact]
        public async Task TryAsync_UserCancelsOperation_LogsError()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var mockAnswerService = new Mock<IAnswerService>();
            mockAnswerService.Setup(x => x.Strings).Returns(new AnswerServiceStrings());
            mockAnswerService.Setup(x => x.HasYesNoAsyncDialog).Returns(true);
            mockAnswerService.Setup(x => x.AskYesNoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // User chooses not to continue

            var logMessages = new List<string>();
            mockAnswerService.Setup(x => x.LogError(It.IsAny<string>()))
                .Callback<string>(message => logMessages.Add(message));

            Func<Task<Answer>> method = () =>
            {
                return Task.FromResult(Answer.Prepare("Method encountered an issue").Error("An error occurred."));
            };

            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            // Act
            var result = await testClass.DoSomething(method, ct);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("An error occurred.", result.Message);
            Assert.Equal(2, logMessages.Count);
            Assert.Contains("Error in", logMessages[0]);
            
            Assert.Contains("Operation cancelled by user in", logMessages[1]);
        }

        [Fact]
        public async Task TryAsync_MethodTimesOut_UserDeclinesToContinue_LogsWarningAndError()
        {
            // Arrange
            var timeout = TimeSpan.FromSeconds(1);
            using var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var mockAnswerService = new Mock<IAnswerService>();
            mockAnswerService.Setup(x => x.GetTimeout()).Returns(timeout);
            mockAnswerService.Setup(x => x.HasTimeout).Returns(true);
            mockAnswerService.Setup(x => x.Strings).Returns(new AnswerServiceStrings());
            mockAnswerService.Setup(x => x.HasTimeOutAsyncDialog).Returns(true);
            mockAnswerService.Setup(x => x.AskYesNoToWaitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // User chooses not to wait

            var logMessages = new List<string>();
            mockAnswerService.Setup(x => x.LogWarning(It.IsAny<string>()))
                .Callback<string>(message => logMessages.Add("Warning: " + message));
            mockAnswerService.Setup(x => x.LogError(It.IsAny<string>()))
                .Callback<string>(message => logMessages.Add("Error: " + message));
            mockAnswerService.Setup(x => x.Strings).Returns(new AnswerServiceStrings());
            Func<Task<Answer>> method = async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
                return Answer.Prepare("Success");
            };

            var testClass = new TestAnswerableClass(mockAnswerService.Object);

            // Act
            var result = await testClass.DoSomething(method, ct);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(new AnswerServiceStrings().CancelledText, result.Message);
            Assert.Equal(2, logMessages.Count);
            Assert.Contains("Warning: Timeout in", logMessages[0]);
            Assert.Contains("Error: Operation cancelled by user in", logMessages[1]);
        }

    }
}