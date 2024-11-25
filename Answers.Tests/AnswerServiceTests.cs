using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Answers.Dialogs;
using Answers;

namespace Answers.Tests
{
  

        public class AnswerServiceTests
        {
            [Fact]
            public void Constructor_WithNullLoggerAndNullDialog_ThrowsArgumentNullException()
            {
                // Arrange & Act & Assert
                Assert.Throws<ArgumentNullException>(() => new Answers.AnswerService(null,null));
            }

            [Fact]
            public void HasYesNoDialog_WhenIsSyncAvailableIsTrue_ReturnsTrue()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                dialogMock.Setup(d => d.HasYesNo).Returns(true);

                var loggerMock = new Mock<ILogger>();
                var answerService = new Answers.AnswerService(dialogMock.Object, loggerMock.Object);

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
                var answerService = new Answers.AnswerService(dialogMock.Object, loggerMock.Object);

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
                var answerService = new Answers.AnswerService(dialogMock.Object, loggerMock.Object);

                // Act
                var result = answerService.HasTimeOutAsyncDialog;

                // Assert
                Assert.True(result);
            }

            [Fact]
            public void HasTimeout_WhenTimeoutIsSet_ReturnsTrue()
            {
                // Arrange
                var answerService = new Answers.AnswerService();
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
                var answerService = new Answers.AnswerService();

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
                var answerService = new Answers.AnswerService(loggerMock.Object);

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
                var answerService = new Answers.AnswerService(loggerMock.Object);

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
                var answerService = new Answers.AnswerService(loggerMock.Object);

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
                var answerService = new Answers.AnswerService(loggerMock.Object);

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
                var answerService = new Answers.AnswerService(loggerMock.Object);

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
                var answerService = new Answers.AnswerService(loggerMock.Object);

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
                var answerService = new Answers.AnswerService(loggerMock.Object);

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
                var answerService = new Answers.AnswerService(loggerMock.Object);

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
                var answerService = new Answers.AnswerService(loggerMock.Object);

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
                var answerService = new Answers.AnswerService(loggerMock.Object);

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
                var answerService = new Answers.AnswerService(dialogMock.Object, loggerMock.Object);

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
                var answerService = new Answers.AnswerService(dialogMock.Object, loggerMock.Object);

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
                var answerService = new Answers.AnswerService(dialogMock.Object, loggerMock.Object);

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
                var answerService = new Answers.AnswerService(dialogMock.Object, loggerMock.Object);

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
                var answerService = new Answers.AnswerService(dialogMock.Object, loggerMock.Object);

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
                var answerService = new Answers.AnswerService(dialogMock.Object, loggerMock.Object);

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
                var answerService = new Answers.AnswerService(dialogMock.Object, loggerMock.Object);

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
                var answerService = new Answers.AnswerService(dialogMock.Object, loggerMock.Object);

                // Act
                var result = answerService.HasTimeOutAsyncDialog;

                // Assert
                Assert.False(result);
            }

            [Fact]
            public void SetTimeout_StoresTimeoutValue()
            {
                // Arrange
                var answerService = new Answers.AnswerService();
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
                var answerService = new Answers.AnswerService();
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
            var loggerMock = new Mock<ILogger<Answers.AnswerService>>();
            var answerService = new Answers.AnswerService(loggerMock.Object);
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
            var loggerMock = new Mock<ILogger<Answers.AnswerService>>();
            var answerService = new Answers.AnswerService(loggerMock.Object);
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
            var loggerMock = new Mock<ILogger<Answers.AnswerService>>();
            var answerService = new Answers.AnswerService(loggerMock.Object);
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
                var answerService = new Answers.AnswerService(loggerMock.Object);

                // Act & Assert
                Assert.Throws<ArgumentNullException>(() => answerService.AddDialog(null));
            }

            [Fact]
            public void AskYesNo_CallsDialogYesNo()
            {
                // Arrange
                var dialogMock = new Mock<IUserDialog>();
                var loggerMock = new Mock<ILogger>();
                var answerService = new Answers.AnswerService(dialogMock.Object, loggerMock.Object);
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
                var answerService = new Answers.AnswerService(dialogMock.Object, loggerMock.Object);
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
                var answerService = new Answers.AnswerService(dialogMock.Object, loggerMock.Object);
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
                var answerService = new Answers.AnswerService(dialogMock.Object, loggerMock.Object);
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
                var answerService = new Answers.AnswerService(dialogMock.Object, loggerMock.Object);

                // Act & Assert
                await Assert.ThrowsAsync<TaskCanceledException>(() =>
                    answerService.AskYesNoAsync("Test message", cts.Token));
            }
        }
    }

