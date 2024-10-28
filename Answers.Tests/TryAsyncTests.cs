﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

    }
}