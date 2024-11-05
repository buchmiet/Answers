using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Answers.Dialogs;
using Answers.AnswerService;

namespace Answers.Tests
{
  

        public class AnswerServiceTests
        {
            [Fact]
            public void Constructor_WithNullLogger_ThrowsArgumentNullException()
            {
                // Arrange & Act & Assert
                Assert.Throws<ArgumentNullException>(() => new AnswerService.AnswerService(null));
            }

            [Fact]
            public void HasYesNoDialog_WhenIsSyncAvailableIsTrue_ReturnsTrue()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasYesNo).Returns(true);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(dialogMock.Object, loggerMock.Object);

                // Act
                var result = answerService.HasYesNoDialog;

                // Assert
                Assert.True(result);
            }

            [Fact]
            public void HasTimeOutDialog_WhenIsAsyncAvailableIsTrue_ReturnsTrue()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasTimeoutDialog).Returns(true);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(dialogMock.Object, loggerMock.Object);

                // Act
                var result = answerService.HasTimeOutDialog;

                // Assert
                Assert.True(result);
            }

            [Fact]
            public void HasTimeOutAsyncDialog_WhenIsAsyncAvailableIsTrue_ReturnsTrue()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasAsyncTimeoutDialog).Returns(true);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(dialogMock.Object, loggerMock.Object);

                // Act
                var result = answerService.HasTimeOutAsyncDialog;

                // Assert
                Assert.True(result);
            }

            [Fact]
            public void HasTimeout_WhenTimeoutIsSet_ReturnsTrue()
            {
                // Arrange
                var answerService = new AnswerService.AnswerService();
                var timeout = TimeSpan.FromSeconds(30);
                answerService.SetTimeout(timeout);

                // Act
                var result = answerService.HasTimeout;

                // Assert
                Assert.True(result);
            }

            [Fact]
            public void HasTimeout_WhenTimeoutIsZero_ReturnsFalse()
            {
                // Arrange
                var answerService = new AnswerService.AnswerService();

                // Act
                var result = answerService.HasTimeout;

                // Assert
                Assert.False(result);
            }

            [Fact]
            public void AddDialog_WithHasYesNoTrue_SetsHasYesNoDialogToTrue()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasYesNo).Returns(true);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(loggerMock.Object);

                // Act
                answerService.AddDialog(dialogMock.Object);

                // Assert
                Assert.True(answerService.HasYesNoDialog);
            }

            [Fact]
            public void AddDialog_WithHasYesNoFalse_SetsHasYesNoDialogToFalse()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasYesNo).Returns(false);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(loggerMock.Object);

                // Act
                answerService.AddDialog(dialogMock.Object);

                // Assert
                Assert.False(answerService.HasYesNoDialog);
            }

            [Fact]
            public void AddDialog_WithHasAsyncYesNoTrue_SetsHasYesNoAsyncDialogToTrue()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasAsyncYesNo).Returns(true);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(loggerMock.Object);

                // Act
                answerService.AddDialog(dialogMock.Object);

                // Assert
                Assert.True(answerService.HasYesNoAsyncDialog);
            }

            [Fact]
            public void AddDialog_WithHasAsyncYesNoFalse_SetsHasYesNoAsyncDialogToFalse()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasAsyncYesNo).Returns(false);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(loggerMock.Object);

                // Act
                answerService.AddDialog(dialogMock.Object);

                // Assert
                Assert.False(answerService.HasYesNoAsyncDialog);
            }

            [Fact]
            public void AddDialog_WithHasTimeoutDialogTrue_SetsHasTimeOutDialogToTrue()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasTimeoutDialog).Returns(true);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(loggerMock.Object);

                // Act
                answerService.AddDialog(dialogMock.Object);

                // Assert
                Assert.True(answerService.HasTimeOutDialog);
            }

            [Fact]
            public void AddDialog_WithHasTimeoutDialogFalse_SetsHasTimeOutDialogToFalse()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasTimeoutDialog).Returns(false);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(loggerMock.Object);

                // Act
                answerService.AddDialog(dialogMock.Object);

                // Assert
                Assert.False(answerService.HasTimeOutDialog);
            }

            [Fact]
            public void AddDialog_WithHasAsyncTimeoutDialogTrue_SetsHasTimeOutAsyncDialogToTrue()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasAsyncTimeoutDialog).Returns(true);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(loggerMock.Object);

                // Act
                answerService.AddDialog(dialogMock.Object);

                // Assert
                Assert.True(answerService.HasTimeOutAsyncDialog);
            }

            [Fact]
            public void AddDialog_WithHasAsyncTimeoutDialogFalse_SetsHasTimeOutAsyncDialogToFalse()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasAsyncTimeoutDialog).Returns(false);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(loggerMock.Object);

                // Act
                answerService.AddDialog(dialogMock.Object);

                // Assert
                Assert.False(answerService.HasTimeOutAsyncDialog);
            }

            [Fact]
            public void AddDialog_WithAllFlagsSet_SetsAllHasDialogPropertiesToTrue()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.SetupAllProperties();
                dialogMock.Setup(d => d.HasYesNo).Returns(true);
                dialogMock.Setup(d => d.HasAsyncYesNo).Returns(true);
                dialogMock.Setup(d => d.HasTimeoutDialog).Returns(true);
                dialogMock.Setup(d => d.HasAsyncTimeoutDialog).Returns(true);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(loggerMock.Object);

                // Act
                answerService.AddDialog(dialogMock.Object);

                // Assert
                Assert.True(answerService.HasYesNoDialog);
                Assert.True(answerService.HasYesNoAsyncDialog);
                Assert.True(answerService.HasTimeOutDialog);
                Assert.True(answerService.HasTimeOutAsyncDialog);
            }

            [Fact]
            public void AddDialog_WithNoFlagsSet_SetsAllHasDialogPropertiesToFalse()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.SetupAllProperties();
                dialogMock.Setup(d => d.HasYesNo).Returns(false);
                dialogMock.Setup(d => d.HasAsyncYesNo).Returns(false);
                dialogMock.Setup(d => d.HasTimeoutDialog).Returns(false);
                dialogMock.Setup(d => d.HasAsyncTimeoutDialog).Returns(false);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(loggerMock.Object);

                // Act
                answerService.AddDialog(dialogMock.Object);

                // Assert
                Assert.False(answerService.HasYesNoDialog);
                Assert.False(answerService.HasYesNoAsyncDialog);
                Assert.False(answerService.HasTimeOutDialog);
                Assert.False(answerService.HasTimeOutAsyncDialog);
            }

            [Fact]
            public void HasYesNoDialog_WhenDialogHasYesNoIsTrue_ReturnsTrue()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasYesNo).Returns(true);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(dialogMock.Object, loggerMock.Object);

                // Act
                var result = answerService.HasYesNoDialog;

                // Assert
                Assert.True(result);
            }

            [Fact]
            public void HasYesNoDialog_WhenDialogHasYesNoIsFalse_ReturnsFalse()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasYesNo).Returns(false);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(dialogMock.Object, loggerMock.Object);

                // Act
                var result = answerService.HasYesNoDialog;

                // Assert
                Assert.False(result);
            }

            [Fact]
            public void HasYesNoAsyncDialog_WhenDialogHasAsyncYesNoIsTrue_ReturnsTrue()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasAsyncYesNo).Returns(true);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(dialogMock.Object, loggerMock.Object);

                // Act
                var result = answerService.HasYesNoAsyncDialog;

                // Assert
                Assert.True(result);
            }

            [Fact]
            public void HasYesNoAsyncDialog_WhenDialogHasAsyncYesNoIsFalse_ReturnsFalse()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasAsyncYesNo).Returns(false);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(dialogMock.Object, loggerMock.Object);

                // Act
                var result = answerService.HasYesNoAsyncDialog;

                // Assert
                Assert.False(result);
            }

            [Fact]
            public void HasTimeOutDialog_WhenDialogHasTimeoutDialogIsTrue_ReturnsTrue()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasTimeoutDialog).Returns(true);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(dialogMock.Object, loggerMock.Object);

                // Act
                var result = answerService.HasTimeOutDialog;

                // Assert
                Assert.True(result);
            }

            [Fact]
            public void HasTimeOutDialog_WhenDialogHasTimeoutDialogIsFalse_ReturnsFalse()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasTimeoutDialog).Returns(false);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(dialogMock.Object, loggerMock.Object);

                // Act
                var result = answerService.HasTimeOutDialog;

                // Assert
                Assert.False(result);
            }

            [Fact]
            public void HasTimeOutAsyncDialog_WhenDialogHasAsyncTimeoutDialogIsTrue_ReturnsTrue()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasAsyncTimeoutDialog).Returns(true);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(dialogMock.Object, loggerMock.Object);

                // Act
                var result = answerService.HasTimeOutAsyncDialog;

                // Assert
                Assert.True(result);
            }

            [Fact]
            public void HasTimeOutAsyncDialog_WhenDialogHasAsyncTimeoutDialogIsFalse_ReturnsFalse()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasAsyncTimeoutDialog).Returns(false);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(dialogMock.Object, loggerMock.Object);

                // Act
                var result = answerService.HasTimeOutAsyncDialog;

                // Assert
                Assert.False(result);
            }

            [Fact]
            public void SetTimeout_StoresTimeoutValue()
            {
                // Arrange
                var answerService = new AnswerService.AnswerService();
                var timeout = TimeSpan.FromSeconds(30);

                // Act
                answerService.SetTimeout(timeout);

                // Assert
                Assert.Equal(timeout, answerService.GetTimeout());
            }

            [Fact]
            public void GetTimeout_ReturnsAndResetsTimeout()
            {
                // Arrange
                var answerService = new AnswerService.AnswerService();
                var timeout = TimeSpan.FromSeconds(30);
                answerService.SetTimeout(timeout);

                // Act
                var returnedTimeout = answerService.GetTimeout();
                var hasTimeoutAfterGet = answerService.HasTimeout;

                // Assert
                Assert.Equal(timeout, returnedTimeout);
                Assert.False(hasTimeoutAfterGet);
            }

            [Fact]
            public void LogInfo_CallsLoggerInformation()
            {
            var loggerMock = new Mock<ILogger<AnswerService.AnswerService>>();
            var answerService = new AnswerService.AnswerService(loggerMock.Object);
            var message = "Test info message";

            // Act
            answerService.LogInfo(message);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void LogWarning_CallsLoggerWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AnswerService.AnswerService>>();
            var answerService = new AnswerService.AnswerService(loggerMock.Object);
            var message = "Test warning message";

            // Act
            answerService.LogWarning(message);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void LogError_CallsLoggerError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AnswerService.AnswerService>>();
            var answerService = new AnswerService.AnswerService(loggerMock.Object);
            var message = "Test error message";

            // Act
            answerService.LogError(message);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }


        [Fact]
            public void AddDialogNull_ThrowsArgumentNullException()
            {
                // Arrange
                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(loggerMock.Object);

                // Act & Assert
                Assert.Throws<ArgumentNullException>(() => answerService.AddDialog(null));
            }

            [Fact]
            public void AskYesNo_CallsDialogYesNo()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(dialogMock.Object, loggerMock.Object);
                var message = "Test message";

                // Act
                answerService.AskYesNo(message);

                // Assert
                dialogMock.Verify(d => d.YesNo(message), Times.Once);
            }

            [Fact]
            public void AskYesNoToWait_CallsDialogContinueTimedOutYesNo()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(dialogMock.Object, loggerMock.Object);
                var message = "Test message";
                var cancellationToken = new CancellationToken();

                // Act
                answerService.AskYesNoToWait(message, cancellationToken);

                // Assert
                dialogMock.Verify(d => d.ContinueTimedOutYesNo(message, cancellationToken), Times.Once);
            }

            [Fact]
            public async Task AskYesNoAsync_CallsDialogYesNoAsync()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.YesNoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(dialogMock.Object, loggerMock.Object);
                var message = "Test async message";
                var cancellationToken = new CancellationToken();

                // Act
                await answerService.AskYesNoAsync(message, cancellationToken);

                // Assert
                dialogMock.Verify(d => d.YesNoAsync(message, cancellationToken), Times.Once);
            }

            [Fact]
            public async Task AskYesNoToWaitAsync_CallsDialogContinueTimedOutYesNoAsync()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.ContinueTimedOutYesNoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(dialogMock.Object, loggerMock.Object);
                var message = "Test async message";
                var cancellationToken = new CancellationToken();

                // Act
                await answerService.AskYesNoToWaitAsync(message, cancellationToken);

                // Assert
                dialogMock.Verify(d => d.ContinueTimedOutYesNoAsync(message, cancellationToken), Times.Once);
            }

            [Fact]
            public async Task AskYesNoAsync_CancellationRequested_ThrowsTaskCanceledException()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                var cts = new CancellationTokenSource();
                cts.Cancel();

                dialogMock.Setup(d => d.YesNoAsync(It.IsAny<string>(), cts.Token))
                          .ThrowsAsync(new TaskCanceledException());

                var loggerMock = new Mock<ILogger>();
                var answerService = new AnswerService.AnswerService(dialogMock.Object, loggerMock.Object);

                // Act & Assert
                await Assert.ThrowsAsync<TaskCanceledException>(() =>
                    answerService.AskYesNoAsync("Test message", cts.Token));
            }
        }
    }


    //public class AnswerServiceTests : IAnswerServiceTests
    //{
    //    private readonly Mock<ILogger> _loggerMock;
    //    private readonly Mock<IUserDialog> _dialogMock;
    //    private readonly AnswerService.AnswerService _service;

    //    public AnswerServiceTests()
    //    {
    //        _loggerMock = new Mock<ILogger>();
    //        _dialogMock = new Mock<IUserDialog>();

    //        // Configure the mock before instantiation
    //        _dialogMock.SetupGet(d => d.HasTimeoutDialog).Returns(true);

    //        _service = new AnswerService.AnswerService(_dialogMock.Object, _loggerMock.Object);
    //    }

    //    //#region Constructor Tests


    //    [Fact]
    //    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    //    {
    //        // Arrange & Act
    //        var exception = Assert.Throws<ArgumentNullException>(() => new AnswerService.AnswerService(_dialogMock.Object, null));

    //        // Assert
    //        Assert.Equal("logger", exception.ParamName);
    //    }


    //    #region Property Tests



    //    [Fact]
    //    public void HasYesNoDialog_WhenIsSyncAvailableIsTrue_ReturnsTrue()
    //    {
    //        // Arrange
    //        _dialogMock.SetupGet(d => d.HasYesNo).Returns(true);

    //        // Act
    //        var result = _service.HasYesNoDialog;

    //        // Assert
    //        Assert.True(result);
    //    }


    //    [Fact]
    //    public void HasTimeOutDialog_WhenIsAsyncAvailableIsTrue_ReturnsTrue()
    //    {
    //        // Arrange
    //        _dialogMock.SetupGet(d => d.HasTimeoutDialog).Returns(true);

    //        // Act
    //        var result = _service.HasTimeOutDialog;

    //        // Assert
    //        Assert.True(result);
    //    }

    //    [Fact]
    //    public void HasTimeOutAsyncDialog_WhenIsAsyncAvailableIsTrue_ReturnsTrue()
    //    {
    //        // Arrange
    //        _dialogMock.SetupGet(d => d.HasAsyncTimeoutDialog).Returns(true);

    //        // Act
    //        var result = _service.HasTimeOutAsyncDialog;

    //        // Assert
    //        Assert.True(result);
    //    }

    //    [Fact]
    //    public void HasTimeout_WhenTimeoutIsSet_ReturnsTrue()
    //    {
    //        // Arrange
    //        _service.SetTimeout(TimeSpan.FromSeconds(30));

    //        // Act
    //        var result = _service.HasTimeout;

    //        // Assert
    //        Assert.True(result);
    //    }

    //    [Fact]
    //    public void HasTimeout_WhenTimeoutIsZero_ReturnsFalse()
    //    {
    //        // Arrange & Act
    //        var result = _service.HasTimeout;

    //        // Assert
    //        Assert.False(result);
    //    }

    //    #endregion

    //    #region Method Tests

    //    [Fact]
    //    public void AddDialog_WithHasYesNoTrue_SetsHasYesNoDialogToTrue()
    //    {
    //        // Arrange
    //        var newDialogMock = new Mock<IUserDialog>();
    //        newDialogMock.SetupGet(d => d.HasYesNo).Returns(true);

    //        // Act
    //        _service.AddDialog(newDialogMock.Object);

    //        // Assert
    //        Assert.True(_service.HasYesNoDialog);
    //    }

    //    [Fact]
    //    public void AddDialog_WithHasYesNoFalse_SetsHasYesNoDialogToFalse()
    //    {
    //        // Arrange
    //        var newDialogMock = new Mock<IUserDialog>();
    //        newDialogMock.SetupGet(d => d.HasYesNo).Returns(false);

    //        // Act
    //        _service.AddDialog(newDialogMock.Object);

    //        // Assert
    //        Assert.False(_service.HasYesNoDialog);
    //    }

    //    [Fact]
    //    public void AddDialog_WithHasAsyncYesNoTrue_SetsHasYesNoAsyncDialogToTrue()
    //    {
    //        // Arrange
    //        var newDialogMock = new Mock<IUserDialog>();
    //        newDialogMock.SetupGet(d => d.HasAsyncYesNo).Returns(true);

    //        // Act
    //        _service.AddDialog(newDialogMock.Object);

    //        // Assert
    //        Assert.True(_service.HasYesNoAsyncDialog);
    //    }

    //    [Fact]
    //    public void AddDialog_WithHasAsyncYesNoFalse_SetsHasYesNoAsyncDialogToFalse()
    //    {
    //        // Arrange
    //        var newDialogMock = new Mock<IUserDialog>();
    //        newDialogMock.SetupGet(d => d.HasAsyncYesNo).Returns(false);

    //        // Act
    //        _service.AddDialog(newDialogMock.Object);

    //        // Assert
    //        Assert.False(_service.HasYesNoAsyncDialog);
    //    }

    //    [Fact]
    //    public void AddDialog_WithHasTimeoutDialogTrue_SetsHasTimeOutDialogToTrue()
    //    {
    //        // Arrange
    //        var newDialogMock = new Mock<IUserDialog>();
    //        newDialogMock.SetupGet(d => d.HasTimeoutDialog).Returns(true);

    //        // Act
    //        _service.AddDialog(newDialogMock.Object);

    //        // Assert
    //        Assert.True(_service.HasTimeOutDialog);
    //    }

    //    [Fact]
    //    public void AddDialog_WithHasTimeoutDialogFalse_SetsHasTimeOutDialogToFalse()
    //    {
    //        // Arrange
    //        var newDialogMock = new Mock<IUserDialog>();
    //        newDialogMock.SetupGet(d => d.HasTimeoutDialog).Returns(false);

    //        // Act
    //        _service.AddDialog(newDialogMock.Object);

    //        // Assert
    //        Assert.False(_service.HasTimeOutDialog);
    //    }

    //    [Fact]
    //    public void AddDialog_WithHasAsyncTimeoutDialogTrue_SetsHasTimeOutAsyncDialogToTrue()
    //    {
    //        // Arrange
    //        var newDialogMock = new Mock<IUserDialog>();
    //        newDialogMock.SetupGet(d => d.HasAsyncTimeoutDialog).Returns(true);

    //        // Act
    //        _service.AddDialog(newDialogMock.Object);

    //        // Assert
    //        Assert.True(_service.HasTimeOutAsyncDialog);
    //    }

    //    [Fact]
    //    public void AddDialog_WithHasAsyncTimeoutDialogFalse_SetsHasTimeOutAsyncDialogToFalse()
    //    {
    //        // Arrange
    //        var newDialogMock = new Mock<IUserDialog>();
    //        newDialogMock.SetupGet(d => d.HasAsyncTimeoutDialog).Returns(false);

    //        // Act
    //        _service.AddDialog(newDialogMock.Object);

    //        // Assert
    //        Assert.False(_service.HasTimeOutAsyncDialog);
    //    }

    //    [Fact]
    //    public void AddDialog_WithAllFlagsSet_SetsAllHasDialogPropertiesToTrue()
    //    {
    //        // Arrange
    //        var newDialogMock = new Mock<IUserDialog>();
    //        newDialogMock.SetupGet(d => d.HasYesNo).Returns(true);
    //        newDialogMock.SetupGet(d => d.HasAsyncYesNo).Returns(true);
    //        newDialogMock.SetupGet(d => d.HasTimeoutDialog).Returns(true);
    //        newDialogMock.SetupGet(d => d.HasAsyncTimeoutDialog).Returns(true);

    //        // Act
    //        _service.AddDialog(newDialogMock.Object);

    //        // Assert
    //        Assert.True(_service.HasYesNoDialog);
    //        Assert.True(_service.HasYesNoAsyncDialog);
    //        Assert.True(_service.HasTimeOutDialog);
    //        Assert.True(_service.HasTimeOutAsyncDialog);
    //    }

    //    [Fact]
    //    public void AddDialog_WithNoFlagsSet_SetsAllHasDialogPropertiesToFalse()
    //    {
    //        // Arrange
    //        var newDialogMock = new Mock<IUserDialog>();
    //        newDialogMock.SetupGet(d => d.HasYesNo).Returns(false);
    //        newDialogMock.SetupGet(d => d.HasAsyncYesNo).Returns(false);
    //        newDialogMock.SetupGet(d => d.HasTimeoutDialog).Returns(false);
    //        newDialogMock.SetupGet(d => d.HasAsyncTimeoutDialog).Returns(false);

    //        // Act
    //        _service.AddDialog(newDialogMock.Object);

    //        // Assert
    //        Assert.False(_service.HasYesNoDialog);
    //        Assert.False(_service.HasYesNoAsyncDialog);
    //        Assert.False(_service.HasTimeOutDialog);
    //        Assert.False(_service.HasTimeOutAsyncDialog);
    //    }

    //    #endregion

    //    #region Property Tests


    //    [Fact]
    //    public void HasYesNoDialog_WhenDialogHasYesNoIsTrue_ReturnsTrue()
    //    {
    //        // Arrange
    //        _dialogMock.SetupGet(d => d.HasYesNo).Returns(true);

    //        // Act
    //        var result = _service.HasYesNoDialog;

    //        // Assert
    //        Assert.True(result);
    //    }

    //    [Fact]
    //    public void HasYesNoDialog_WhenDialogHasYesNoIsFalse_ReturnsFalse()
    //    {
    //        // Arrange
    //        _dialogMock.SetupGet(d => d.HasYesNo).Returns(false);

    //        // Act
    //        var result = _service.HasYesNoDialog;

    //        // Assert
    //        Assert.False(result);
    //    }

    //    [Fact]
    //    public void HasYesNoAsyncDialog_WhenDialogHasAsyncYesNoIsTrue_ReturnsTrue()
    //    {
    //        // Arrange
    //        _dialogMock.SetupGet(d => d.HasAsyncYesNo).Returns(true);

    //        // Act
    //        var result = _service.HasYesNoAsyncDialog;

    //        // Assert
    //        Assert.True(result);
    //    }

    //    [Fact]
    //    public void HasYesNoAsyncDialog_WhenDialogHasAsyncYesNoIsFalse_ReturnsFalse()
    //    {
    //        // Arrange
    //        _dialogMock.SetupGet(d => d.HasAsyncYesNo).Returns(false);

    //        // Act
    //        var result = _service.HasYesNoAsyncDialog;

    //        // Assert
    //        Assert.False(result);
    //    }

    //    [Fact]
    //    public void HasTimeOutDialog_WhenDialogHasTimeoutDialogIsTrue_ReturnsTrue()
    //    {
    //        // Arrange
    //        _dialogMock.SetupGet(d => d.HasTimeoutDialog).Returns(true);

    //        // Act
    //        var result = _service.HasTimeOutDialog;

    //        // Assert
    //        Assert.True(result);
    //    }

    //    [Fact]
    //    public void HasTimeOutDialog_WhenDialogHasTimeoutDialogIsFalse_ReturnsFalse()
    //    {
    //        // Arrange
    //        _dialogMock.SetupGet(d => d.HasTimeoutDialog).Returns(false);

    //        // Act
    //        var result = _service.HasTimeOutDialog;

    //        // Assert
    //        Assert.False(result);
    //    }

    //    [Fact]
    //    public void HasTimeOutAsyncDialog_WhenDialogHasAsyncTimeoutDialogIsTrue_ReturnsTrue()
    //    {
    //        // Arrange
    //        _dialogMock.SetupGet(d => d.HasAsyncTimeoutDialog).Returns(true);

    //        // Act
    //        var result = _service.HasTimeOutAsyncDialog;

    //        // Assert
    //        Assert.True(result);
    //    }

    //    [Fact]
    //    public void HasTimeOutAsyncDialog_WhenDialogHasAsyncTimeoutDialogIsFalse_ReturnsFalse()
    //    {
    //        // Arrange
    //        _dialogMock.SetupGet(d => d.HasAsyncTimeoutDialog).Returns(false);

    //        // Act
    //        var result = _service.HasTimeOutAsyncDialog;

    //        // Assert
    //        Assert.False(result);
    //    }

    //    #endregion

    //    #region SetTimeout and GetTimeout Tests

    //    [Fact]
    //    public void SetTimeout_StoresTimeoutValue()
    //    {
    //        // Arrange
    //        var timeout = TimeSpan.FromSeconds(15);

    //        // Act
    //        _service.SetTimeout(timeout);

    //        // Assert
    //        Assert.True(_service.HasTimeout);
    //    }

    //    [Fact]
    //    public void GetTimeout_ReturnsAndResetsTimeout()
    //    {
    //        // Arrange
    //        var timeout = TimeSpan.FromSeconds(15);
    //        _service.SetTimeout(timeout);

    //        // Act
    //        var returnedTimeout = _service.GetTimeout();

    //        // Assert
    //        Assert.Equal(timeout, returnedTimeout);
    //        Assert.False(_service.HasTimeout);
    //    }

    //    #endregion

    //    #region Logging Method Tests

    //    [Fact]
    //    public void LogInfo_CallsLoggerInformation()
    //    {
    //        // Arrange
    //        var message = "Information message";

    //        // Act
    //        _service.LogInfo(message);

    //        // Assert
    //        _loggerMock.Verify(
    //            l => l.Log(
    //                LogLevel.Information,
    //                It.IsAny<EventId>(),
    //                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
    //                It.IsAny<Exception>(),
    //                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
    //            ),
    //            Times.Once
    //        );
    //    }


    //    [Fact]
    //    public void LogWarning_CallsLoggerWarning()
    //    {
    //        // Arrange
    //        var message = "Warning message";

    //        // Act
    //        _service.LogWarning(message);

    //        // Assert
    //        _loggerMock.Verify(
    //            l => l.Log(
    //                LogLevel.Warning,
    //                It.IsAny<EventId>(),
    //                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
    //                It.IsAny<Exception>(),
    //                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
    //            ),
    //            Times.Once
    //        );
    //    }


    //    [Fact]
    //    public void LogError_CallsLoggerError()
    //    {
    //        // Arrange
    //        var message = "Error message";

    //        // Act
    //        _service.LogError(message);

    //        // Assert
    //        _loggerMock.Verify(
    //            l => l.Log(
    //                LogLevel.Error,
    //                It.IsAny<EventId>(),
    //                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
    //                It.IsAny<Exception>(),
    //                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
    //            ),
    //            Times.Once
    //        );
    //    }


    //    #endregion

    //    #region Synchronous Method Tests

    //    [Fact]
    //    public void AddDialogNull_ThrowsArgumetNullException()
    //    {
    //        // Arrange
    //        AnswerService.AnswerService service;

    //        // Act & Assert
    //        Assert.Throws<ArgumentNullException>(() => service = new AnswerService.AnswerService(null, _loggerMock.Object));
    //    }

    //    [Fact]
    //    public void AskYesNo_CallsDialogYesNo()
    //    {
    //        // Arrange
    //        var message = "Test message";
    //        _dialogMock.Setup(d => d.YesNo(message)).Returns(true);

    //        // Act
    //        var result = _service.AskYesNo(message);

    //        // Assert
    //        Assert.True(result);
    //        _dialogMock.Verify(d => d.YesNo(message), Times.Once);
    //    }

    //    [Fact]
    //    public void AskYesNoToWait_CallsDialogContinueTimedOutYesNo()
    //    {
    //        // Arrange
    //        var message = "Test message";
    //        var ct = new CancellationTokenSource().Token;
    //        _dialogMock.Setup(d => d.ContinueTimedOutYesNo(message, ct)).Returns(true);

    //        // Act
    //        var result = _service.AskYesNoToWait(message, ct);

    //        // Assert
    //        Assert.True(result);
    //        _dialogMock.Verify(d => d.ContinueTimedOutYesNo(message, ct), Times.Once);
    //    }

    //    #endregion

    //    #region Asynchronous Method Tests



    //    [Fact]
    //    public async Task AskYesNoAsync_CallsDialogYesNoAsync()
    //    {
    //        // Arrange
    //        var message = "Test message";
    //        _dialogMock.Setup(d => d.YesNoAsync(message, It.IsAny<CancellationToken>())).ReturnsAsync(true);

    //        // Act
    //        var result = await _service.AskYesNoAsync(message, CancellationToken.None);

    //        // Assert
    //        Assert.True(result);
    //        _dialogMock.Verify(d => d.YesNoAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    //    }

    //    [Fact]
    //    public async Task AskYesNoToWaitAsync_CallsDialogContinueTimedOutYesNoAsync()
    //    {
    //        // Arrange
    //        var message = "Test message";
    //        var ct = new CancellationTokenSource().Token;
    //        _dialogMock.Setup(d => d.ContinueTimedOutYesNoAsync(message, ct)).ReturnsAsync(true);

    //        // Act
    //        var result = await _service.AskYesNoToWaitAsync(message, ct);

    //        // Assert
    //        Assert.True(result);
    //        _dialogMock.Verify(d => d.ContinueTimedOutYesNoAsync(message, ct), Times.Once);
    //    }

    //    [Fact]
    //    public async Task AskYesNoAsync_CancellationRequested_ThrowsTaskCanceledException()
    //    {
    //        // Arrange
    //        var message = "Test message";
    //        var cts = new CancellationTokenSource();
    //        cts.Cancel();

    //        _dialogMock.Setup(d => d.YesNoAsync(message, It.IsAny<CancellationToken>()))
    //            .Returns<string, CancellationToken>((msg, token) => Task.FromCanceled<bool>(token));

    //        // Act & Assert
    //        await Assert.ThrowsAsync<TaskCanceledException>(() => _service.AskYesNoAsync(message, cts.Token));
    //    }

    //    #endregion


    //}

