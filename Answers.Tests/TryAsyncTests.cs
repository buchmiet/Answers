using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            mockAnswerService.Setup(x => x.AskYesNoToWaitAsync(It.IsAny<string>(), ct, ct))
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
            mockAnswerService.Setup(x => x.AskYesNoToWaitAsync(It.IsAny<string>(), ct, ct))
                .ReturnsAsync(false); // User chooses not to continue waiting

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
            Assert.Contains("Timeout", result.Message, StringComparison.OrdinalIgnoreCase);
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
            Assert.Contains("Timeout", result.Message, StringComparison.OrdinalIgnoreCase);
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
            Assert.Contains("canceled", result.Message, StringComparison.OrdinalIgnoreCase);
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

            // Counters to keep track of prompts
            int promptCount = 0;

            // Simulate user responses
            mockAnswerService.Setup(x => x.HasTimeOutAsyncDialog).Returns(true);
            mockAnswerService
                .Setup(x => x.AskYesNoToWaitAsync(
                    Moq.It.IsAny<string>(),
                    Moq.It.IsAny<System.Threading.CancellationToken>(),
                    Moq.It.IsAny<System.Threading.CancellationToken>()))
                .Returns<string, System.Threading.CancellationToken, System.Threading.CancellationToken>(async (message, dialogCt, globalCt) =>
                {
                    promptCount++;
                    if (promptCount <= 2)
                    {
                        // First two responses take 200ms
                        await System.Threading.Tasks.Task.Delay(200, dialogCt);
                        return true; // User chooses to continue
                    }

                    // Third response takes 1 second
                    await System.Threading.Tasks.Task.Delay(1000, dialogCt);
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
            Assert.InRange(totalStopwatch.Elapsed.TotalSeconds, 7, 8);

            // The method's stopwatch should have recorded approximately 7 seconds
            Assert.InRange(methodStopwatch.Elapsed.TotalSeconds, 7, 7.5);

            // The prompt count should be 3
            Assert.Equal(3, promptCount);
        }

    }
}