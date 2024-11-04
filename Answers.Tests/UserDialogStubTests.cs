using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Answers.Tests
{
    public class UserDialogStubTests
    {
        [Fact]
        public void Constructor_ValidInputs_ShouldCreateInstance()
        {
            // Arrange
            var responses = new List<bool> { true, false };
            var delays = new List<TimeSpan> { TimeSpan.FromMilliseconds(1000) };

            // Act
            var stub = new UserDialogStub(responses, delays);

            // Assert
            Assert.NotNull(stub);
        }

        [Fact]
        public void Constructor_EmptyResponses_ShouldThrowArgumentException()
        {
            // Arrange
            var responses = new List<bool>();
            var delays = new List<TimeSpan> { TimeSpan.FromMilliseconds(1000) };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new UserDialogStub(responses, delays));
        }

        [Fact]
        public void Constructor_EmptyDelays_ShouldThrowArgumentException()
        {
            // Arrange
            var responses = new List<bool> { true };
            var delays = new List<TimeSpan>();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new UserDialogStub(responses, delays));
        }

        [Fact]
        public void YesNo_ShouldReturnExpectedResponse()
        {
            // Arrange
            var responses = new List<bool> { true, false };
            var delays = new List<TimeSpan> { TimeSpan.Zero };
            var stub = new UserDialogStub(responses, delays);

            // Act & Assert
            Assert.True(stub.YesNo("Test"));
            Assert.False(stub.YesNo("Test"));
            Assert.True(stub.YesNo("Test")); // Should cycle back
        }

        [Fact]
        public async Task YesNoAsync_ShouldReturnExpectedResponse()
        {
            // Arrange
            var responses = new List<bool> { true, false };
            var delays = new List<TimeSpan> { TimeSpan.Zero };
            var stub = new UserDialogStub(responses, delays);
            var ct = CancellationToken.None;

            // Act & Assert
            Assert.True(await stub.YesNoAsync("Test", ct));
            Assert.False(await stub.YesNoAsync("Test", ct));
            Assert.True(await stub.YesNoAsync("Test", ct)); // Should cycle back
        }

        [Fact]
        public void ContinueTimedOutYesNo_ShouldReturnExpectedResponseAfterDelay()
        {
            // Arrange
            var responses = new List<bool> { true };
            var delays = new List<TimeSpan> { TimeSpan.FromMilliseconds(100) };
            var stub = new UserDialogStub(responses, delays);
            var cts1 = new CancellationTokenSource();


            // Act
            var startTime = DateTime.UtcNow;
            var result = stub.ContinueTimedOutYesNo("Test", cts1.Token);
            var elapsedTime = DateTime.UtcNow - startTime;

            // Assert
            Assert.True(result);
            Assert.True(elapsedTime >= TimeSpan.FromMilliseconds(100));
        }

        [Fact]
        public async Task ContinueTimedOutYesNoAsync_ShouldReturnExpectedResponseAfterDelay()
        {
            // Arrange
            var responses = new List<bool> { true };
            var delays = new List<TimeSpan> { TimeSpan.FromMilliseconds(100) };
            var stub = new UserDialogStub(responses, delays);
            var cts1 = new CancellationTokenSource();


            // Act
            var startTime = DateTime.UtcNow;
            var result = await stub.ContinueTimedOutYesNoAsync("Test", cts1.Token);
            var elapsedTime = DateTime.UtcNow - startTime;

            // Assert
            Assert.True(result);
            Assert.True(elapsedTime >= TimeSpan.FromMilliseconds(100));
        }

        [Fact]
        public void ContinueTimedOutYesNo_Cancellation_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var responses = new List<bool> { true };
            var delays = new List<TimeSpan> { TimeSpan.FromMilliseconds(500) };
            var stub = new UserDialogStub(responses, delays);
            var cts1 = new CancellationTokenSource();


            // Act
            cts1.CancelAfter(100); // Cancel after 100ms

            // Assert
            Assert.Throws<OperationCanceledException>(() => stub.ContinueTimedOutYesNo("Test", cts1.Token));

            // After cancellation, the object should be disposed
            Assert.Throws<ObjectDisposedException>(() => stub.YesNo("Test"));
        }

        [Fact]
        public async Task ContinueTimedOutYesNoAsync_Cancellation_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var responses = new List<bool> { true };
            var delays = new List<TimeSpan> { TimeSpan.FromMilliseconds(500) };
            var stub = new UserDialogStub(responses, delays);
            var cts1 = new CancellationTokenSource();


            // Act
            cts1.CancelAfter(100); // Cancel after 100ms

            // Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await stub.ContinueTimedOutYesNoAsync("Test", cts1.Token));

            // After cancellation, the object should be disposed
            Assert.Throws<ObjectDisposedException>(() => stub.YesNo("Test"));
        }

        [Fact]
        public async Task DisposedObject_ShouldThrowObjectDisposedException()
        {
            // Arrange
            var responses = new List<bool> { true };
            var delays = new List<TimeSpan> { TimeSpan.Zero };
            var stub = new UserDialogStub(responses, delays);

            // Act
            stub.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => stub.YesNo("Test"));
            Assert.Throws<ObjectDisposedException>(() => stub.ContinueTimedOutYesNo("Test",  CancellationToken.None));

            await Assert.ThrowsAsync<ObjectDisposedException>(() => stub.YesNoAsync("Test", CancellationToken.None));
            await Assert.ThrowsAsync<ObjectDisposedException>(() => stub.ContinueTimedOutYesNoAsync("Test",  CancellationToken.None));
        }

    }
}
