using Answers;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Answers.Tests
{
    public class AnswerServiceTests
    {
        private readonly Mock<IUserDialog> _mockUserDialog;
        private readonly Mock<ILogger> _mockLogger;
        private readonly AnswerService _answerService;
        private readonly AnswerService _answerServiceWithoutDialog;


        public AnswerServiceTests()
        {
            _mockUserDialog = new Mock<IUserDialog>();
            _mockLogger = new Mock<ILogger>();
            _answerService = new AnswerService(_mockUserDialog.Object, _mockLogger.Object);
            _answerServiceWithoutDialog = new AnswerService( _mockLogger.Object);
        }


      


        [Fact]
        public async Task OperationFailure_PropagatesCorrectly_WithoutAdditionalPrompts()
        {
            // Arrange
            var mockDialog = new Mock<IUserDialog>();
            // Simulate user choosing not to retry
            mockDialog.Setup(d => d.YesNoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var mockLogger = new Mock<ILogger>();

            var answerService = new AnswerService(mockDialog.Object, mockLogger.Object);

            // Create tier instances
            var tier4 = new Tier4(answerService);
            var tier3 = new Tier3(tier4, answerService);
            var tier2 = new Tier2(tier3, answerService);
            var tier1 = new Tier1(tier2, answerService);

            // Act
            var finalAnswer = await tier1.DoOperationAsync();

            // Assert
            Assert.False(finalAnswer.IsSuccess, "Final Answer should indicate failure.");
            Assert.True(finalAnswer.DialogConcluded, "DialogConcluded should be true to prevent further prompts.");
            Assert.Contains("Tier4 operation failed", finalAnswer.Message);


            // Verify that YesNoAsync was called only once
            mockDialog.Verify(d => d.YesNoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #region Property Tests

        [Fact]
        public void HasDialog_ShouldReturnTrue_WhenDialogIsSet()
        {
            // Arrange
            _answerService.AddYesNoDialog(_mockUserDialog.Object);

            // Act
            var result = _answerService.HasDialog;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasDialog_ShouldReturnFalse_WhenDialogIsNotSet()
        {
            // Arrange
            // Dialog is not set

            // Act
            var result = _answerServiceWithoutDialog.HasDialog;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasTimeOutDialog_ShouldReturnTrue_WhenTimeOutDialogIsSet()
        {
            // Arrange
            _answerService.AddTimeoutDialog(_mockUserDialog.Object);

            // Act
            var result = _answerService.HasTimeOutDialog;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasTimeOutDialog_ShouldReturnFalse_WhenTimeOutDialogIsNotSet()
        {
            // Arrange
            // TimeOutDialog is not set

            // Act
            var result = _answerService.HasTimeOutDialog;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasTimeout_ShouldReturnTrue_WhenTimeoutIsSet()
        {
            // Arrange
            _answerService.SetTimeout(TimeSpan.FromSeconds(30));

            // Act
            var result = _answerService.HasTimeout;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasTimeout_ShouldReturnFalse_WhenTimeoutIsNotSet()
        {
            // Arrange
            // Timeout is not set

            // Act
            var result = _answerService.HasTimeout;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Timeout_ShouldReturnCorrectValue_WhenTimeoutIsSet()
        {
            // Arrange
            var expectedTimeout = TimeSpan.FromSeconds(45);
            _answerService.SetTimeout(expectedTimeout);

            // Act
            var result = _answerService.Timeout;

            // Assert
            Assert.Equal(expectedTimeout, result);
        }

        #endregion

        #region Method Tests

        [Fact]
        public void AddYesNoDialog_ShouldSetDialog()
        {
            // Arrange
            var newDialog = new Mock<IUserDialog>().Object;

            // Act
            _answerService.AddYesNoDialog(newDialog);

            // Assert
            Assert.True(_answerService.HasDialog);
        }

        [Fact]
        public void AddTimeoutDialog_ShouldSetTimeOutDialog()
        {
            // Arrange
            var newDialog = new Mock<IUserDialog>().Object;

            // Act
            _answerService.AddTimeoutDialog(newDialog);

            // Assert
            Assert.True(_answerService.HasTimeOutDialog);
        }

        [Fact]
        public void SetTimeout_ShouldSetTimeoutValue()
        {
            // Arrange
            var expectedTimeout = TimeSpan.FromMinutes(1);

            // Act
            _answerService.SetTimeout(expectedTimeout);

            // Assert
            Assert.Equal(expectedTimeout, _answerService.Timeout);
        }

        [Fact]
        public async Task AskYesNoAsync_ShouldReturnResult_WhenDialogIsSet()
        {
            // Arrange
            var message = "Proceed?";
            var ct = CancellationToken.None;
            var expectedResult = true;

            _mockUserDialog.Setup(d => d.YesNoAsync(message, ct)).ReturnsAsync(expectedResult);
            _answerService.AddYesNoDialog(_mockUserDialog.Object);

            // Act
            var result = await _answerService.AskYesNoAsync(message, ct);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockUserDialog.Verify(d => d.YesNoAsync(message, ct), Times.Once);
        }

        [Fact]
        public async Task AskYesNoAsync_ShouldThrowInvalidOperationException_WhenDialogIsNotSet()
        {
            var message = "Proceed?";
            var ct = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _answerServiceWithoutDialog.AskYesNoAsync(message, ct)
            );
        }

        [Fact]
        public async Task AskYesNoToWaitAsync_ShouldReturnResult_WhenDialogIsSet()
        {
            // Arrange
            var message = "Wait for completion?";
            var ct = CancellationToken.None;
            var expectedResult = false;

            _mockUserDialog.Setup(d => d.ContinueTimedOutYesNoAsync(message, ct)).ReturnsAsync(expectedResult);
            _answerService.AddYesNoDialog(_mockUserDialog.Object);

            // Act
            var result = await _answerService.AskYesNoToWaitAsync(message, ct);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockUserDialog.Verify(d => d.ContinueTimedOutYesNoAsync(message, ct), Times.Once);
        }

        [Fact]
        public async Task AskYesNoToWaitAsync_ShouldThrowInvalidOperationException_WhenDialogIsNotSet()
        {
            // Arrange
            var message = "Wait for completion?";
            var ct = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _answerServiceWithoutDialog.AskYesNoToWaitAsync(message, ct));
        }

        [Fact]
        public void LogInfo_ShouldCallLogger_LogInformation()
        {
            // Arrange
            var message = "Information message";

            // Act
            _answerService.LogInfo(message);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                ),
                Times.Once
            );
        }

        [Fact]
        public void LogError_ShouldCallLogger_LogError()
        {
            // Arrange
            var message = "Error message";

            // Act
            _answerService.LogError(message);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                ),
                Times.Once
            );
        }

        [Fact]
        public void LogWarning_ShouldCallLogger_LogWarning()
        {
            // Arrange
            var message = "Warning message";

            // Act
            _answerService.LogWarning(message);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                ),
                Times.Once
            );
        }

        #endregion

        #region Concurrency Tests

        [Fact]
        public void SetTimeout_ShouldBeThreadSafe()
        {
            // Arrange
            var timeout1 = TimeSpan.FromSeconds(10);
            var timeout2 = TimeSpan.FromSeconds(20);

            // Act
            Parallel.Invoke(
                () => _answerService.SetTimeout(timeout1),
                () => _answerService.SetTimeout(timeout2)
            );

            // Assert
            // Since the operation is thread-safe, the final value should be one of the timeouts
            Assert.True(_answerService.Timeout == timeout1 || _answerService.Timeout == timeout2);
        }

        [Fact]
        public void AddYesNoDialog_ShouldBeThreadSafe()
        {
            // Arrange
            var dialog1 = new Mock<IUserDialog>().Object;
            var dialog2 = new Mock<IUserDialog>().Object;

            // Act
            Parallel.Invoke(
                () => _answerService.AddYesNoDialog(dialog1),
                () => _answerService.AddYesNoDialog(dialog2)
            );

            // Assert
            // Since Interlocked.Exchange is used, the dialog should be one of the two
            Assert.True(_answerService.HasDialog);
        }

        #endregion
    }
}
