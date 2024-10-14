using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace Answers.Tests
{
    public class TryAsyncTests
    {
        private Mock<IAnswerService> _answerServiceMock;
        private CancellationToken _cancellationToken;
        private TimeSpan _defaultTimeout;
        private TestClassForTryAsync _testClass;

        public TryAsyncTests()
        {
            _answerServiceMock = new Mock<IAnswerService>();
            _cancellationToken = CancellationToken.None;
            _defaultTimeout = TimeSpan.FromSeconds(1);
            _testClass = new TestClassForTryAsync(_answerServiceMock.Object);
        }

        [Fact]
        public async Task TryAsync_MethodCompletesSuccessfully_ReturnsAnswer()
        {
            // Arrange
            var expectedAnswer = new Answer();
            Func<Task<IAnswer>> method = () => Task.FromResult<IAnswer>(expectedAnswer);

            // Act
            var result = await _testClass.TryAsync(method, _cancellationToken, _defaultTimeout);

            // Assert
            Assert.Equal(expectedAnswer, result);
            _answerServiceMock.Verify(x => x.LogInfo(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task TryAsync_MethodFails_UserRetriesUntilSuccess()
        {
            // Arrange
            var callCount = 0;
            Func<Task<IAnswer>> method = () =>
            {
                callCount++;
                if (callCount < 2)
                {
                    var answer = new Answer().Error("");
                    answer.Error("Error occurred.");
                    return Task.FromResult<IAnswer>(answer);
                }
                else
                {
                    var answer = new Answer();
                    return Task.FromResult<IAnswer>(answer);
                }
            };

            _answerServiceMock.Setup(x => x.HasDialog).Returns(true);
            _answerServiceMock.Setup(x => x.AskYesNoAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(true);

            // Act
            var result = await _testClass.TryAsync(method, _cancellationToken, _defaultTimeout);

            // Assert
            Assert.Equal(2, callCount);
            Assert.True(result.IsSuccess);
            _answerServiceMock.Verify(x => x.AskYesNoAsync(It.IsAny<string>(), _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task TryAsync_MethodFails_UserDeclinesToRetry_ReturnsAnswer()
        {
            // Arrange
            var callCount = 0;
            Func<Task<IAnswer>> method = () =>
            {
                callCount++;
                var answer = new Answer().Error("");
                answer.Error("Error occurred.");
                return Task.FromResult<IAnswer>(answer);
            };

            _answerServiceMock.Setup(x => x.HasDialog).Returns(true);
            _answerServiceMock.Setup(x => x.AskYesNoAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(false);

            // Act
            var result = await _testClass.TryAsync(method, _cancellationToken, _defaultTimeout);

            // Assert
            Assert.Equal(1, callCount);
            Assert.False(result.IsSuccess);
            _answerServiceMock.Verify(x => x.AskYesNoAsync(It.IsAny<string>(), _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task TryAsync_MethodTimesOut_UserChoosesToRetry_RetriesMethod()
        {
            // Arrange
            var callCount = 0;
            Func<Task<IAnswer>> method = async () =>
            {
                callCount++;
                await Task.Delay(_defaultTimeout + TimeSpan.FromSeconds(1));
                var answer = new Answer();
                return answer;
            };

            _answerServiceMock.Setup(x => x.HasTimeOutDialog).Returns(true);
            _answerServiceMock.SetupSequence(x => x.AskYesNoToWaitAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            // Act
            var result = await _testClass.TryAsync(method, _cancellationToken, _defaultTimeout);

            // Assert
            Assert.Equal(2, callCount);
            Assert.False(result.IsSuccess);
            Assert.True(result.IsTimedOut);
            _answerServiceMock.Verify(x => x.AskYesNoToWaitAsync(It.IsAny<string>(), _cancellationToken), Times.Exactly(2));
        }

        [Fact]
        public async Task TryAsync_MethodTimesOut_UserDeclinesToRetry_ReturnsTimedOutAnswer()
        {
            // Arrange
            Func<Task<IAnswer>> method = async () =>
            {
                await Task.Delay(_defaultTimeout + TimeSpan.FromSeconds(1));
                var answer = new Answer();
                return answer;
            };

            _answerServiceMock.Setup(x => x.HasTimeOutDialog).Returns(true);
            _answerServiceMock.Setup(x => x.AskYesNoToWaitAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(false);

            // Act
            var result = await _testClass.TryAsync(method, _cancellationToken, _defaultTimeout);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsTimedOut);
            _answerServiceMock.Verify(x => x.AskYesNoToWaitAsync(It.IsAny<string>(), _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task TryAsync_MethodThrowsException_ThrowsException()
        {
            // Arrange
            Func<Task<IAnswer>> method = () => throw new InvalidOperationException("Test exception");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _testClass.TryAsync(method, _cancellationToken, _defaultTimeout));
        }

        [Fact]
        public async Task TryAsync_MethodReturnsAnswerWithDialogConcluded_ReturnsAnswer()
        {
            // Arrange
            var answer = new Answer().Error(""); 
            answer.DialogConcluded= true;
            
            Func<Task<IAnswer>> method = () => Task.FromResult<IAnswer>(answer);

            _answerServiceMock.Setup(x => x.HasDialog).Returns(true);

            // Act
            var result = await _testClass.TryAsync(method, _cancellationToken, _defaultTimeout);

            // Assert
            Assert.Equal(answer, result);
            _answerServiceMock.Verify(x => x.AskYesNoAsync(It.IsAny<string>(), _cancellationToken), Times.Never);
        }

        [Fact]
        public async Task TryAsync_MethodFails_NoDialog_ReturnsAnswer()
        {
            // Arrange
            var answer = new Answer();
            answer=answer.Error("error");
            Func<Task<IAnswer>> method = () => Task.FromResult<IAnswer>(answer);

            _answerServiceMock.Setup(x => x.HasDialog).Returns(false);

            // Act
            var result = await _testClass.TryAsync(method, _cancellationToken, _defaultTimeout);

            // Assert
            Assert.Equal(answer, result);
            _answerServiceMock.Verify(x => x.AskYesNoAsync(It.IsAny<string>(), _cancellationToken), Times.Never);
        }
    }

}
