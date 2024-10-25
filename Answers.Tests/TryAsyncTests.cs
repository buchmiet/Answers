using System;
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
            Answer result = await testClass.TryAsync(method, ct);

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
            Answer result = await testClass.TryAsync(method, ct);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Error occurred", result.Message);
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
            Answer result = await testClass.TryAsync(method, ct);

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
            Answer result = await testClass.TryAsync(method, ct);

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
            Answer result = await testClass.TryAsync(method, ct);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("IsSuccess", result.Message);
        }

        //[Fact]
        //public async Task TryAsync_ExecutionTimesOut_WithTimeoutDialog_UserContinuesWaiting_Succeeds()
        //{
        //    // Arrange
        //    var cts = new CancellationTokenSource();
        //    var ct = cts.Token;

        //    var mockAnswerService = new Mock<IAnswerService>();
        //    mockAnswerService.SetupSequence(x => x.GetTimeout())
        //        .Returns(TimeSpan.FromSeconds(1))
        //        .Returns(TimeSpan.FromSeconds(2)); // New timeout after user chooses to wait
        //    mockAnswerService.Setup(x => x.HasTimeOutAsyncDialog).Returns(true);
        //    mockAnswerService.Setup(x => x.AskYesNoToWaitAsync(It.IsAny<string>(), ct, ct))
        //        .ReturnsAsync(true); // User chooses to continue waiting

        //    var testClass = new TestAnswerableClass(mockAnswerService.Object);

        //    Func<Task<Answer>> method = async () =>
        //    {
        //        await Task.Delay(1500, ct); // Longer than initial timeout but less than total time
        //        return Answer.IsSuccess("IsSuccess after waiting");
        //    };

        //    // Act
        //    var result = await testClass.TryAsync(method, ct);

        //    // Assert
        //    Assert.True(result.IsIsSuccess);
        //    Assert.Equal("IsSuccess after waiting", result.Message);
        //}

        //[Fact]
        //public async Task TryAsync_ExecutionTimesOut_WithTimeoutDialog_UserDoesNotContinue_ReturnsTimeoutError()
        //{
        //    // Arrange
        //    var cts = new CancellationTokenSource();
        //    var ct = cts.Token;

        //    var mockAnswerService = new Mock<IAnswerService>();
        //    mockAnswerService.Setup(x => x.GetTimeout()).Returns(TimeSpan.FromSeconds(1));
        //    mockAnswerService.Setup(x => x.HasTimeOutAsyncDialog).Returns(true);
        //    mockAnswerService.Setup(x => x.AskYesNoToWaitAsync(It.IsAny<string>(), ct, ct))
        //        .ReturnsAsync(false); // User chooses not to continue waiting

        //    var testClass = new TestAnswerableClass(mockAnswerService.Object);

        //    Func<Task<Answer>> method = async () =>
        //    {
        //        await Task.Delay(5000, ct); // Won't complete before timeout
        //        return Answer.IsSuccess("IsSuccess after waiting");
        //    };

        //    // Act
        //    var result = await testClass.TryAsync(method, ct);

        //    // Assert
        //    Assert.False(result.IsIsSuccess);
        //    Assert.Contains("timed out", result.Message, StringComparison.OrdinalIgnoreCase);
        //}

        //[Fact]
        //public async Task TryAsync_ExecutionTimesOut_NoTimeoutDialog_ReturnsTimeoutError()
        //{
        //    // Arrange
        //    var cts = new CancellationTokenSource();
        //    var ct = cts.Token;

        //    var mockAnswerService = new Mock<IAnswerService>();
        //    mockAnswerService.Setup(x => x.GetTimeout()).Returns(TimeSpan.FromSeconds(1));
        //    mockAnswerService.Setup(x => x.HasTimeOutAsyncDialog).Returns(false);

        //    var testClass = new TestAnswerableClass(mockAnswerService.Object);

        //    Func<Task<Answer>> method = async () =>
        //    {
        //        await Task.Delay(5000, ct); // Won't complete before timeout
        //        return Answer.IsSuccess("IsSuccess after waiting");
        //    };

        //    // Act
        //    var result = await testClass.TryAsync(method, ct);

        //    // Assert
        //    Assert.False(result.IsIsSuccess);
        //    Assert.Contains("timed out", result.Message, StringComparison.OrdinalIgnoreCase);
        //}

        //[Fact]
        //public async Task TryAsync_UserCancelsOperation_CancellationRequested_ReturnsCancellation()
        //{
        //    // Arrange
        //    var cts = new CancellationTokenSource(100); // Cancel after 100 ms
        //    var ct = cts.Token;

        //    var mockAnswerService = new Mock<IAnswerService>();
        //    mockAnswerService.Setup(x => x.GetTimeout()).Returns(TimeSpan.Zero);

        //    var testClass = new TestAnswerableClass(mockAnswerService.Object);

        //    Func<Task<Answer>> method = async () =>
        //    {
        //        await Task.Delay(5000, ct); // Long-running task
        //        return Answer.IsSuccess("Should not reach here");
        //    };

        //    // Act
        //    var result = await testClass.TryAsync(method, ct);

        //    // Assert
        //    Assert.False(result.IsIsSuccess);
        //    Assert.Contains("canceled", result.Message, StringComparison.OrdinalIgnoreCase);
        //}

        //public class TryAsyncTests
        //{
        //    private Mock<IAnswerService> _answerServiceMock;
        //    private CancellationToken _cancellationToken;
        //    private TestClassForTryAsync _testClass;

        //    public TryAsyncTests()
        //    {
        //        _answerServiceMock = new Mock<IAnswerService>();
        //        _cancellationToken = CancellationToken.None;
        //        _testClass = new TestClassForTryAsync(_answerServiceMock.Object);
        //    }

        //    [Fact]
        //    public async Task TryAsync_ShouldBeThreadSafe_UnderHeavyLoad()
        //    {
        //        // Given

        //        int taskCount = 100;
        //        var tasks = new Task[taskCount];
        //        var results = new ConcurrentBag<Answers.Answer>();
        //        var cancellationTokenSource = new CancellationTokenSource();

        //        // When
        //        for (int i = 0; i < taskCount; i++)
        //        {
        //            tasks[i] = Task.Run(async () =>
        //            {
        //                var result = await _testClass.MethodReturningAnswer(SomeMethodAsync, cancellationTokenSource.Token);
        //                results.Add(result);
        //            });
        //        }

        //        await Task.WhenAll(tasks);

        //        // Then
        //        // Verify that all tasks completed IsSuccessfully and no task failed due to thread safety issues
        //        Assert.Equal(taskCount, results.Count);
        //        Assert.All(results, answer => Assert.True(answer.IsIsSuccess));
        //    }

        //    private async Task<Answers.Answer> SomeMethodAsync()
        //    {
        //        await Task.Delay(100, _cancellationToken); // Simulate some work
        //        return Answers.Answer.Prepare("IsSuccess");
        //    }


        //    [Fact]
        //    public async Task TryAsync_MethodCompletesIsSuccessfully_ReturnsAnswer()
        //    {
        //        // Arrange
        //        var expectedAnswer = new Answer("TryAsync_MethodCompletesIsSuccessfully_ReturnsAnswer");
        //        Func<Task<Answer>> method = () => Task.FromResult<Answer>(expectedAnswer);

        //        // Act
        //        var result = await _testClass.MethodReturningAnswer(method, _cancellationToken);

        //        // Assert
        //        Assert.Equal(expectedAnswer, result);
        //    }

        //    [Fact]
        //    public async Task TryAsync_MethodFails_UserRetriesUntilIsSuccess()
        //    {
        //        // Arrange
        //        var callCount = 0;
        //        Func<Task<Answer>> method = () =>
        //        {
        //            callCount++;
        //            if (callCount < 2)
        //            {
        //                var answer = Answer.Prepare("Test Error");
        //                answer.Error("Error occurred.");
        //                return Task.FromResult<Answer>(answer);
        //            }
        //            else
        //            {
        //                var answer = new Answer("TryAsync_MethodFails_UserRetriesUntilIsSuccess");
        //                return Task.FromResult<Answer>(answer);
        //            }
        //        };

        //        _answerServiceMock.Setup(x => x.HasDialog).Returns(true);
        //        _answerServiceMock.Setup(x => x.AskYesNoAsync(It.IsAny<string>(), _cancellationToken))
        //            .ReturnsAsync(true);

        //        // Act
        //        var result = await _testClass.MethodReturningAnswer(method, _cancellationToken);

        //        // Assert
        //        Assert.Equal(2, callCount);
        //        Assert.True(result.IsIsSuccess);
        //        _answerServiceMock.Verify(x => x.AskYesNoAsync(It.IsAny<string>(), _cancellationToken), Times.Once);
        //    }

        //    [Fact]
        //    public async Task TryAsync_MethodFails_UserDeclinesToRetry_ReturnsAnswer()
        //    {
        //        // Arrange
        //        var callCount = 0;
        //        Func<Task<Answer>> method = () =>
        //        {
        //            callCount++;
        //            var answer = Answer.Prepare("Test Error");
        //            answer.Error("Error occurred.");
        //            return Task.FromResult<Answer>(answer);
        //        };

        //        _answerServiceMock.Setup(x => x.HasDialog).Returns(true);
        //        _answerServiceMock.Setup(x => x.AskYesNoAsync(It.IsAny<string>(), _cancellationToken))
        //            .ReturnsAsync(false);

        //        // Act
        //        var result = await _testClass.MethodReturningAnswer(method, _cancellationToken);

        //        // Assert
        //        Assert.Equal(1, callCount);
        //        Assert.False(result.IsIsSuccess);
        //        _answerServiceMock.Verify(x => x.AskYesNoAsync(It.IsAny<string>(), _cancellationToken), Times.Once);
        //    }


        //    [Fact]
        //    public async Task TryAsync_MethodThrowsException_ThrowsException()
        //    {
        //        // Arrange
        //        Func<Task<Answer>> method = () => throw new InvalidOperationException("Test exception");

        //        // Act & Assert
        //        await Assert.ThrowsAsync<InvalidOperationException>(() => _testClass.MethodReturningAnswer(method, _cancellationToken));
        //    }

        //    [Fact]
        //    public async Task TryAsync_MethodReturnsAnswerWithDialogConcluded_ReturnsAnswer()
        //    {
        //        // Arrange
        //        var answer = new Answer("TryAsync_MethodReturnsAnswerWithDialogConcluded_ReturnsAnswer").Error("Test error"); 
        //        answer.DialogConcluded= true;

        //        Func<Task<Answer>> method = () => Task.FromResult<Answer>(answer);

        //        _answerServiceMock.Setup(x => x.HasDialog).Returns(true);

        //        // Act
        //        var result = await _testClass.MethodReturningAnswer(method, _cancellationToken);

        //        // Assert
        //        Assert.Equal(answer, result);
        //        _answerServiceMock.Verify(x => x.AskYesNoAsync(It.IsAny<string>(), _cancellationToken), Times.Never);
        //    }

        //    [Fact]
        //    public async Task TryAsync_MethodFails_NoDialog_ReturnsAnswer()
        //    {
        //        // Arrange
        //        var answer = new Answer("TryAsync_MethodFails_NoDialog_ReturnsAnswer");
        //        answer=answer.Error("error");
        //        Func<Task<Answer>> method = () => Task.FromResult<Answer>(answer);

        //        _answerServiceMock.Setup(x => x.HasDialog).Returns(false);

        //        // Act
        //        var result = await _testClass.MethodReturningAnswer(method, _cancellationToken);

        //        // Assert
        //        Assert.Equal(answer, result);
        //        _answerServiceMock.Verify(x => x.AskYesNoAsync(It.IsAny<string>(), _cancellationToken), Times.Never);
        //    }





        //    ///// <summary>
        //    ///// Test Case 3: Task times out, user chooses not to retry.
        //    ///// Expectation: TryAsync returns an error Answer with the timeout message.
        //    ///// </summary>
        //    //[Fact]
        //    //public async Task TryAsync_TaskTimesOut_UserChoosesNotToRetry_ReturnsErrorAnswer()
        //    //{
        //    //    // Arrange
        //    //    var timeout = TimeSpan.FromMilliseconds(100);
        //    //    _answerServiceMock.SetupGet(a => a.Timeout).Returns(timeout);
        //    //    _answerServiceMock.SetupGet(a => a.HasTimeOutDialog).Returns(true);
        //    //    _answerServiceMock.SetupGet(a => a.HasDialog).Returns(true);
        //    //    _answerServiceMock
        //    //        .Setup(a => a.AskYesNoToWaitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //    //        .ReturnsAsync(false); // User chooses not to retry

        //    //    var userDialogStub = new UserDialogStub(false);
        //    //    _answerServiceMock.Object.AddDialog(userDialogStub);

        //    //    // Simulate a long-running task
        //    //    Func<Task<Answer>> longRunningTask = async () =>
        //    //    {
        //    //        var answer = Answer.Prepare("Long-running task message");
        //    //        await Task.Delay(5000, _cancellationToken); // Task duration longer than timeout
        //    //        return answer;
        //    //    };

        //    //    // Act
        //    //    var result = await _testClass.MethodReturningAnswer(longRunningTask, _cancellationToken, timeout);

        //    //    // Assert
        //    //    Assert.False(result.IsIsSuccess);
        //    //    Assert.Contains("Long-running task message", result.Message);
        //    //    Assert.Contains($"{timeout.TotalSeconds} seconds elapsed", result.Message);
        //    //    _answerServiceMock.Verify(a => a.AskYesNoToWaitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        //    //}

        //    ///// <summary>
        //    ///// Test Case 4: Task fails before timeout, user chooses to retry.
        //    ///// Expectation: TryAsync retries the task and eventually succeeds.
        //    ///// </summary>
        //    //[Fact]
        //    //public async Task TryAsync_TaskFailsBeforeTimeout_UserChoosesToRetry_RetriesTask()
        //    //{
        //    //    // Arrange
        //    //    var timeout = TimeSpan.FromSeconds(5);
        //    //    _answerServiceMock.SetupGet(a => a.Timeout).Returns(timeout);
        //    //    _answerServiceMock.SetupGet(a => a.HasTimeOutDialog).Returns(false);
        //    //    _answerServiceMock.SetupGet(a => a.HasDialog).Returns(true);
        //    //    _answerServiceMock
        //    //        .Setup(a => a.AskYesNoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //    //        .ReturnsAsync(true); // User chooses to retry

        //    //    var userDialogStub = new UserDialogStub(true);
        //    //    _answerServiceMock.Object.AddDialog(userDialogStub);

        //    //    int attempt = 0;
        //    //    Func<Task<Answer>> failingTaskWithRetryIsSuccess = async () =>
        //    //    {
        //    //        attempt++;
        //    //        var answer = Answer.Prepare("Failing task message");
        //    //        await Task.Delay(100, _cancellationToken); // Task duration shorter than timeout
        //    //        if (attempt < 2)
        //    //        {
        //    //            answer.ConcludeDialog(); // Simulate failure
        //    //        }
        //    //        else
        //    //        {
        //    //            return answer; // Second attempt succeeds
        //    //        }
        //    //        return answer;
        //    //    };

        //    //    // Act
        //    //    var result = await _testClass.MethodReturningAnswer(failingTaskWithRetryIsSuccess, _cancellationToken, timeout);

        //    //    // Assert
        //    //    Assert.True(result.IsIsSuccess);
        //    //    Assert.Equal("Failing task message", result.Message);
        //    //    Assert.Equal(2, attempt); // Ensures that it retried once
        //    //    _answerServiceMock.Verify(a => a.AskYesNoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        //    //}

        //    ///// <summary>
        //    ///// Test Case 5: Task fails before timeout, user chooses not to retry.
        //    ///// Expectation: TryAsync returns the failure Answer.
        //    ///// </summary>
        //    //[Fact]
        //    //public async Task TryAsync_TaskFailsBeforeTimeout_UserChoosesNotToRetry_ReturnsFailureAnswer()
        //    //{
        //    //    // Arrange
        //    //    var timeout = TimeSpan.FromSeconds(5);
        //    //    _answerServiceMock.SetupGet(a => a.Timeout).Returns(timeout);
        //    //    _answerServiceMock.SetupGet(a => a.HasTimeOutDialog).Returns(false);
        //    //    _answerServiceMock.SetupGet(a => a.HasDialog).Returns(true);
        //    //    _answerServiceMock
        //    //        .Setup(a => a.AskYesNoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //    //        .ReturnsAsync(false); // User chooses not to retry

        //    //    var userDialogStub = new UserDialogStub(false);
        //    //    _answerServiceMock.Object.AddDialog(userDialogStub);

        //    //    Func<Task<Answer>> failingTask = async () =>
        //    //    {
        //    //        var answer = Answer.Prepare("Failing task message");
        //    //        await Task.Delay(100, _cancellationToken); // Task duration shorter than timeout
        //    //        answer.ConcludeDialog(); // Simulate failure
        //    //        return answer;
        //    //    };

        //    //    // Act
        //    //    var result = await _testClass.MethodReturningAnswer(failingTask, _cancellationToken, timeout);

        //    //    // Assert
        //    //    Assert.False(result.IsIsSuccess);
        //    //    Assert.Contains("Failing task message", result.Message);
        //    //    _answerServiceMock.Verify(a => a.AskYesNoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        //    //}

        //    ///// <summary>
        //    ///// Test Case 6: Immediate cancellation.
        //    ///// Expectation: TryAsync respects the cancellation token and throws OperationCanceledException.
        //    ///// </summary>
        //    //[Fact]
        //    //public async Task TryAsync_ImmediateCancellation_ThrowsOperationCanceledException()
        //    //{
        //    //    // Arrange
        //    //    var timeout = TimeSpan.FromSeconds(5);
        //    //    _answerServiceMock.SetupGet(a => a.Timeout).Returns(timeout);
        //    //    _answerServiceMock.SetupGet(a => a.HasTimeOutDialog).Returns(false);
        //    //    _answerServiceMock.SetupGet(a => a.HasDialog).Returns(false);

        //    //    var cts = new CancellationTokenSource();
        //    //    cts.Cancel(); // Immediately cancel

        //    //    Func<Task<Answer>> anyTask = async () =>
        //    //    {
        //    //        var answer = Answer.Prepare("Any task message");
        //    //        await Task.Delay(1000, cts.Token); // Should be canceled immediately
        //    //        return answer;
        //    //    };

        //    //    // Act & Assert
        //    //    await Assert.ThrowsAsync<OperationCanceledException>(() =>
        //    //        _testClass.MethodReturningAnswer(anyTask, cts.Token, timeout));
        //    //}

        //    ///// <summary>
        //    ///// Test Case 7: Exception within the task.
        //    ///// Expectation: TryAsync propagates the exception.
        //    ///// </summary>
        //    //[Fact]
        //    //public async Task TryAsync_TaskThrowsException_PropagatesException()
        //    //{
        //    //    // Arrange
        //    //    var timeout = TimeSpan.FromSeconds(5);
        //    //    _answerServiceMock.SetupGet(a => a.Timeout).Returns(timeout);
        //    //    _answerServiceMock.SetupGet(a => a.HasTimeOutDialog).Returns(false);
        //    //    _answerServiceMock.SetupGet(a => a.HasDialog).Returns(false);

        //    //    Func<Task<Answer>> exceptionThrowingTask = async () =>
        //    //    {
        //    //        var answer = Answer.Prepare("Exception task message");
        //    //        await Task.Delay(100, _cancellationToken);
        //    //        throw new InvalidOperationException("Test exception");
        //    //    };

        //    //    // Act & Assert
        //    //    var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
        //    //        _testClass.MethodReturningAnswer(exceptionThrowingTask, _cancellationToken, timeout));

        //    //    Assert.Equal("Test exception", exception.Message);
        //    //}

        //    ///// <summary>
        //    ///// Test Case 7 (Alternative): Exception within the task handled by TryAsync.
        //    ///// Expectation: TryAsync returns a failure Answer indicating the exception.
        //    ///// Uncomment and modify based on your TryAsync implementation.
        //    ///// </summary>
        //    ///*
        //    //[Fact]
        //    //public async Task TryAsync_TaskThrowsException_ReturnsFailureAnswer()
        //    //{
        //    //    // Arrange
        //    //    var timeout = TimeSpan.FromSeconds(5);
        //    //    _answerServiceMock.SetupGet(a => a.Timeout).Returns(timeout);
        //    //    _answerServiceMock.SetupGet(a => a.HasTimeOutDialog).Returns(false);
        //    //    _answerServiceMock.SetupGet(a => a.HasDialog).Returns(false);

        //    //    Func<Task<Answer>> exceptionThrowingTask = async () =>
        //    //    {
        //    //        var answer = Answer.Prepare("Exception task message");
        //    //        await Task.Delay(100, _cancellationToken);
        //    //        throw new InvalidOperationException("Test exception");
        //    //    };

        //    //    // Act
        //    //    var result = await _testClass.MethodReturningAnswer(exceptionThrowingTask, _cancellationToken, timeout);

        //    //    // Assert
        //    //    Assert.False(result.IsIsSuccess);
        //    //    Assert.Contains("Exception task message", result.Message);
        //    //    Assert.Contains("Test exception", result.Message);
        //    //}
        //    //*/

        //    ///// <summary>
        //    ///// Test Case 8: Task completes just after timeout.
        //    ///// Expectation: TryAsync handles the timeout correctly.
        //    ///// </summary>
        //    //[Fact]
        //    //public async Task TryAsync_TaskCompletesJustAfterTimeout_HandlesTimeout()
        //    //{
        //    //    // Arrange
        //    //    var timeout = TimeSpan.FromMilliseconds(200);
        //    //    _answerServiceMock.SetupGet(a => a.Timeout).Returns(timeout);
        //    //    _answerServiceMock.SetupGet(a => a.HasTimeOutDialog).Returns(true);
        //    //    _answerServiceMock.SetupGet(a => a.HasDialog).Returns(true);
        //    //    _answerServiceMock
        //    //        .Setup(a => a.AskYesNoToWaitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //    //        .ReturnsAsync(false); // User chooses not to retry

        //    //    var userDialogStub = new UserDialogStub(false);
        //    //    _answerServiceMock.Object.AddDialog(userDialogStub);

        //    //    Func<Task<Answer>> borderlineTask = async () =>
        //    //    {
        //    //        var answer = Answer.Prepare("Borderline task message");
        //    //        await Task.Delay(250, _cancellationToken); // Slightly longer than timeout
        //    //        return answer;
        //    //    };

        //    //    // Act
        //    //    var result = await _testClass.MethodReturningAnswer(borderlineTask, _cancellationToken, timeout);

        //    //    // Assert
        //    //    Assert.False(result.IsIsSuccess);
        //    //    Assert.Contains("Borderline task message", result.Message);
        //    //    Assert.Contains($"{timeout.TotalSeconds} seconds elapsed", result.Message);
        //    //    _answerServiceMock.Verify(a => a.AskYesNoToWaitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        //    //}

        //    ///// <summary>
        //    ///// Test Case 9: Concurrent executions of TryAsync.
        //    ///// Expectation: Multiple TryAsync calls do not interfere with each other.
        //    ///// </summary>
        //    //[Fact]
        //    //public async Task TryAsync_ConcurrentExecutions_DoNotInterfere()
        //    //{
        //    //    // Arrange
        //    //    var timeout = TimeSpan.FromMilliseconds(100);
        //    //    _answerServiceMock.SetupGet(a => a.Timeout).Returns(timeout);
        //    //    _answerServiceMock.SetupGet(a => a.HasTimeOutDialog).Returns(true);
        //    //    _answerServiceMock.SetupGet(a => a.HasDialog).Returns(true);
        //    //    _answerServiceMock
        //    //        .Setup(a => a.AskYesNoToWaitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //    //        .ReturnsAsync(false); // User chooses not to retry

        //    //    var userDialogStub = new UserDialogStub(false);
        //    //    _answerServiceMock.Object.AddDialog(userDialogStub);

        //    //    // Create two tasks with different messages
        //    //    Func<Task<Answer>> task1 = async () =>
        //    //    {
        //    //        var answer = Answer.Prepare("Task 1 message");
        //    //        await Task.Delay(5000, _cancellationToken); // Task duration longer than timeout
        //    //        return answer;
        //    //    };

        //    //    Func<Task<Answer>> task2 = async () =>
        //    //    {
        //    //        var answer = Answer.Prepare("Task 2 message");
        //    //        await Task.Delay(5000, _cancellationToken); // Task duration longer than timeout
        //    //        return answer;
        //    //    };

        //    //    // Act
        //    //    var tryAsyncTasks = new[]
        //    //    {
        //    //        _testClass.MethodReturningAnswer(task1, _cancellationToken, timeout),
        //    //        _testClass.MethodReturningAnswer(task2, _cancellationToken, timeout)
        //    //    };

        //    //    var results = await Task.WhenAll(tryAsyncTasks);

        //    //    // Assert
        //    //    Assert.False(results[0].IsIsSuccess);
        //    //    Assert.Contains("Task 1 message", results[0].Message);
        //    //    Assert.Contains($"{timeout.TotalSeconds} seconds elapsed", results[0].Message);

        //    //    Assert.False(results[1].IsIsSuccess);
        //    //    Assert.Contains("Task 2 message", results[1].Message);
        //    //    Assert.Contains($"{timeout.TotalSeconds} seconds elapsed", results[1].Message);

        //    //    // Verify that dialogs were called for each task
        //    //    _answerServiceMock.Verify(a => a.AskYesNoToWaitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        //    //}
        //}

    }
}