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
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IUserDialog> _dialogMock;
        private readonly AnswerService _service;

        public AnswerServiceTests()
        {
            _loggerMock = new Mock<ILogger>();
            _dialogMock = new Mock<IUserDialog>();
            _service = new AnswerService(_dialogMock.Object, _loggerMock.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullDialog_DoesNotThrow()
        {
            // Arrange & Act
            var exception = Record.Exception(() => new AnswerService(null, _loggerMock.Object));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Arrange & Act
            var exception = Assert.Throws<ArgumentNullException>(() => new AnswerService(_dialogMock.Object, null));

            // Assert
            Assert.Equal("logger", exception.ParamName);
        }

        #endregion

        #region Property Tests



        [Fact]
        public void HasYesNoDialog_WhenIsSyncAvailableIsTrue_ReturnsTrue()
        {
            // Arrange
            _dialogMock.SetupGet(d => d.HasYesNo).Returns(true);

            // Act
            var result = _service.HasYesNoDialog;

            // Assert
            Assert.True(result);
        }


        [Fact]
        public void HasTimeOutDialog_WhenIsAsyncAvailableIsTrue_ReturnsTrue()
        {
            // Arrange
            _dialogMock.SetupGet(d => d.HasTimeoutDialog).Returns(true);

            // Act
            var result = _service.HasTimeOutDialog;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasTimeOutAsyncDialog_WhenIsAsyncAvailableIsTrue_ReturnsTrue()
        {
            // Arrange
            _dialogMock.SetupGet(d => d.HasAsyncTimeoutDialog).Returns(true);

            // Act
            var result = _service.HasTimeOutAsyncDialog;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasTimeout_WhenTimeoutIsSet_ReturnsTrue()
        {
            // Arrange
            _service.SetTimeout(TimeSpan.FromSeconds(30));

            // Act
            var result = _service.HasTimeout;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasTimeout_WhenTimeoutIsZero_ReturnsFalse()
        {
            // Arrange & Act
            var result = _service.HasTimeout;

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Method Tests

        [Fact]
        public void AddDialog_WithHasYesNoTrue_SetsHasYesNoDialogToTrue()
        {
            // Arrange
            var newDialogMock = new Mock<IUserDialog>();
            newDialogMock.SetupGet(d => d.HasYesNo).Returns(true);

            // Act
            _service.AddDialog(newDialogMock.Object);

            // Assert
            Assert.True(_service.HasYesNoDialog);
        }

        [Fact]
        public void AddDialog_WithHasYesNoFalse_SetsHasYesNoDialogToFalse()
        {
            // Arrange
            var newDialogMock = new Mock<IUserDialog>();
            newDialogMock.SetupGet(d => d.HasYesNo).Returns(false);

            // Act
            _service.AddDialog(newDialogMock.Object);

            // Assert
            Assert.False(_service.HasYesNoDialog);
        }

        [Fact]
        public void AddDialog_WithHasAsyncYesNoTrue_SetsHasYesNoAsyncDialogToTrue()
        {
            // Arrange
            var newDialogMock = new Mock<IUserDialog>();
            newDialogMock.SetupGet(d => d.HasAsyncYesNo).Returns(true);

            // Act
            _service.AddDialog(newDialogMock.Object);

            // Assert
            Assert.True(_service.HasYesNoAsyncDialog);
        }

        [Fact]
        public void AddDialog_WithHasAsyncYesNoFalse_SetsHasYesNoAsyncDialogToFalse()
        {
            // Arrange
            var newDialogMock = new Mock<IUserDialog>();
            newDialogMock.SetupGet(d => d.HasAsyncYesNo).Returns(false);

            // Act
            _service.AddDialog(newDialogMock.Object);

            // Assert
            Assert.False(_service.HasYesNoAsyncDialog);
        }

        [Fact]
        public void AddDialog_WithHasTimeoutDialogTrue_SetsHasTimeOutDialogToTrue()
        {
            // Arrange
            var newDialogMock = new Mock<IUserDialog>();
            newDialogMock.SetupGet(d => d.HasTimeoutDialog).Returns(true);

            // Act
            _service.AddDialog(newDialogMock.Object);

            // Assert
            Assert.True(_service.HasTimeOutDialog);
        }

        [Fact]
        public void AddDialog_WithHasTimeoutDialogFalse_SetsHasTimeOutDialogToFalse()
        {
            // Arrange
            var newDialogMock = new Mock<IUserDialog>();
            newDialogMock.SetupGet(d => d.HasTimeoutDialog).Returns(false);

            // Act
            _service.AddDialog(newDialogMock.Object);

            // Assert
            Assert.False(_service.HasTimeOutDialog);
        }

        [Fact]
        public void AddDialog_WithHasAsyncTimeoutDialogTrue_SetsHasTimeOutAsyncDialogToTrue()
        {
            // Arrange
            var newDialogMock = new Mock<IUserDialog>();
            newDialogMock.SetupGet(d => d.HasAsyncTimeoutDialog).Returns(true);

            // Act
            _service.AddDialog(newDialogMock.Object);

            // Assert
            Assert.True(_service.HasTimeOutAsyncDialog);
        }

        [Fact]
        public void AddDialog_WithHasAsyncTimeoutDialogFalse_SetsHasTimeOutAsyncDialogToFalse()
        {
            // Arrange
            var newDialogMock = new Mock<IUserDialog>();
            newDialogMock.SetupGet(d => d.HasAsyncTimeoutDialog).Returns(false);

            // Act
            _service.AddDialog(newDialogMock.Object);

            // Assert
            Assert.False(_service.HasTimeOutAsyncDialog);
        }

        [Fact]
        public void AddDialog_WithAllFlagsSet_SetsAllHasDialogPropertiesToTrue()
        {
            // Arrange
            var newDialogMock = new Mock<IUserDialog>();
            newDialogMock.SetupGet(d => d.HasYesNo).Returns(true);
            newDialogMock.SetupGet(d => d.HasAsyncYesNo).Returns(true);
            newDialogMock.SetupGet(d => d.HasTimeoutDialog).Returns(true);
            newDialogMock.SetupGet(d => d.HasAsyncTimeoutDialog).Returns(true);

            // Act
            _service.AddDialog(newDialogMock.Object);

            // Assert
            Assert.True(_service.HasYesNoDialog);
            Assert.True(_service.HasYesNoAsyncDialog);
            Assert.True(_service.HasTimeOutDialog);
            Assert.True(_service.HasTimeOutAsyncDialog);
        }

        [Fact]
        public void AddDialog_WithNoFlagsSet_SetsAllHasDialogPropertiesToFalse()
        {
            // Arrange
            var newDialogMock = new Mock<IUserDialog>();
            newDialogMock.SetupGet(d => d.HasYesNo).Returns(false);
            newDialogMock.SetupGet(d => d.HasAsyncYesNo).Returns(false);
            newDialogMock.SetupGet(d => d.HasTimeoutDialog).Returns(false);
            newDialogMock.SetupGet(d => d.HasAsyncTimeoutDialog).Returns(false);

            // Act
            _service.AddDialog(newDialogMock.Object);

            // Assert
            Assert.False(_service.HasYesNoDialog);
            Assert.False(_service.HasYesNoAsyncDialog);
            Assert.False(_service.HasTimeOutDialog);
            Assert.False(_service.HasTimeOutAsyncDialog);
        }

        #endregion

        #region Property Tests

        [Fact]
        public void HasYesNoDialog_WhenDialogIsNull_ReturnsFalse()
        {
            // Arrange
            var service = new AnswerService(null, _loggerMock.Object);

            // Act
            var result = service.HasYesNoDialog;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasYesNoDialog_WhenDialogHasYesNoIsTrue_ReturnsTrue()
        {
            // Arrange
            _dialogMock.SetupGet(d => d.HasYesNo).Returns(true);

            // Act
            var result = _service.HasYesNoDialog;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasYesNoDialog_WhenDialogHasYesNoIsFalse_ReturnsFalse()
        {
            // Arrange
            _dialogMock.SetupGet(d => d.HasYesNo).Returns(false);

            // Act
            var result = _service.HasYesNoDialog;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasYesNoAsyncDialog_WhenDialogHasAsyncYesNoIsTrue_ReturnsTrue()
        {
            // Arrange
            _dialogMock.SetupGet(d => d.HasAsyncYesNo).Returns(true);

            // Act
            var result = _service.HasYesNoAsyncDialog;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasYesNoAsyncDialog_WhenDialogHasAsyncYesNoIsFalse_ReturnsFalse()
        {
            // Arrange
            _dialogMock.SetupGet(d => d.HasAsyncYesNo).Returns(false);

            // Act
            var result = _service.HasYesNoAsyncDialog;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasTimeOutDialog_WhenDialogHasTimeoutDialogIsTrue_ReturnsTrue()
        {
            // Arrange
            _dialogMock.SetupGet(d => d.HasTimeoutDialog).Returns(true);

            // Act
            var result = _service.HasTimeOutDialog;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasTimeOutDialog_WhenDialogHasTimeoutDialogIsFalse_ReturnsFalse()
        {
            // Arrange
            _dialogMock.SetupGet(d => d.HasTimeoutDialog).Returns(false);

            // Act
            var result = _service.HasTimeOutDialog;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasTimeOutAsyncDialog_WhenDialogHasAsyncTimeoutDialogIsTrue_ReturnsTrue()
        {
            // Arrange
            _dialogMock.SetupGet(d => d.HasAsyncTimeoutDialog).Returns(true);

            // Act
            var result = _service.HasTimeOutAsyncDialog;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasTimeOutAsyncDialog_WhenDialogHasAsyncTimeoutDialogIsFalse_ReturnsFalse()
        {
            // Arrange
            _dialogMock.SetupGet(d => d.HasAsyncTimeoutDialog).Returns(false);

            // Act
            var result = _service.HasTimeOutAsyncDialog;

            // Assert
            Assert.False(result);
        }

        #endregion

        #region SetTimeout and GetTimeout Tests

        [Fact]
        public void SetTimeout_StoresTimeoutValue()
        {
            // Arrange
            var timeout = TimeSpan.FromSeconds(15);

            // Act
            _service.SetTimeout(timeout);

            // Assert
            Assert.True(_service.HasTimeout);
        }

        [Fact]
        public void GetTimeout_ReturnsAndResetsTimeout()
        {
            // Arrange
            var timeout = TimeSpan.FromSeconds(15);
            _service.SetTimeout(timeout);

            // Act
            var returnedTimeout = _service.GetTimeout();

            // Assert
            Assert.Equal(timeout, returnedTimeout);
            Assert.False(_service.HasTimeout);
        }

        #endregion

        #region Logging Method Tests

        [Fact]
        public void LogInfo_CallsLoggerInformation()
        {
            // Arrange
            var message = "Information message";

            // Act
            _service.LogInfo(message);

            // Assert
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                    It.IsAny<Exception?>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
                ),
                Times.Once
            );
        }


        [Fact]
        public void LogWarning_CallsLoggerWarning()
        {
            // Arrange
            var message = "Warning message";

            // Act
            _service.LogWarning(message);

            // Assert
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                    It.IsAny<Exception?>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
                ),
                Times.Once
            );
        }


        [Fact]
        public void LogError_CallsLoggerError()
        {
            // Arrange
            var message = "Error message";

            // Act
            _service.LogError(message);

            // Assert
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                    It.IsAny<Exception?>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
                ),
                Times.Once
            );
        }


        #endregion

        #region Synchronous Method Tests

        [Fact]
        public void AskYesNo_WhenDialogIsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var service = new AnswerService(null, _loggerMock.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => service.AskYesNo("Test message"));
        }

        [Fact]
        public void AskYesNo_CallsDialogYesNo()
        {
            // Arrange
            var message = "Test message";
            _dialogMock.Setup(d => d.YesNo(message)).Returns(true);

            // Act
            var result = _service.AskYesNo(message);

            // Assert
            Assert.True(result);
            _dialogMock.Verify(d => d.YesNo(message), Times.Once);
        }

        [Fact]
        public void AskYesNoToWait_CallsDialogContinueTimedOutYesNo()
        {
            // Arrange
            var message = "Test message";
            var localCancellationToken = new CancellationTokenSource().Token;
            var ct = new CancellationTokenSource().Token;
            _dialogMock.Setup(d => d.ContinueTimedOutYesNo(message, localCancellationToken, ct)).Returns(true);

            // Act
            var result = _service.AskYesNoToWait(message, localCancellationToken, ct);

            // Assert
            Assert.True(result);
            _dialogMock.Verify(d => d.ContinueTimedOutYesNo(message, localCancellationToken, ct), Times.Once);
        }

        #endregion

        #region Asynchronous Method Tests

        [Fact]
        public async Task AskYesNoAsync_WhenDialogIsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var service = new AnswerService(null, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AskYesNoAsync("Test message", CancellationToken.None));
        }

        [Fact]
        public async Task AskYesNoAsync_CallsDialogYesNoAsync()
        {
            // Arrange
            var message = "Test message";
            _dialogMock.Setup(d => d.YesNoAsync(message, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _service.AskYesNoAsync(message, CancellationToken.None);

            // Assert
            Assert.True(result);
            _dialogMock.Verify(d => d.YesNoAsync(message, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AskYesNoToWaitAsync_CallsDialogContinueTimedOutYesNoAsync()
        {
            // Arrange
            var message = "Test message";
            var localCancellationToken = new CancellationTokenSource().Token;
            var ct = new CancellationTokenSource().Token;
            _dialogMock.Setup(d => d.ContinueTimedOutYesNoAsync(message, localCancellationToken, ct)).ReturnsAsync(true);

            // Act
            var result = await _service.AskYesNoToWaitAsync(message, localCancellationToken, ct);

            // Assert
            Assert.True(result);
            _dialogMock.Verify(d => d.ContinueTimedOutYesNoAsync(message, localCancellationToken, ct), Times.Once);
        }

        [Fact]
        public async Task AskYesNoAsync_CancellationRequested_ThrowsTaskCanceledException()
        {
            // Arrange
            var message = "Test message";
            var cts = new CancellationTokenSource();
            cts.Cancel();

            _dialogMock.Setup(d => d.YesNoAsync(message, It.IsAny<CancellationToken>()))
                .Returns<string, CancellationToken>((msg, token) => Task.FromCanceled<bool>(token));

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => _service.AskYesNoAsync(message, cts.Token));
        }

        #endregion


    }
}
