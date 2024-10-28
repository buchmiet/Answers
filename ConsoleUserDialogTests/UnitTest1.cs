namespace ConsoleUserDialogTests
{

        public class ConsoleUserDialogTests
        {
            [Fact]
            public void YesNo_ReturnsTrue_WhenUserInputsYes()
            {
                // Arrange
                var dialog = new ConsoleUserDialog.ConsoleUserDialog();
                var input = new StringReader("yes\n");
                var output = new StringWriter();
                Console.SetIn(input);
                Console.SetOut(output);

                // Act
                var result = dialog.YesNo("Are you sure?");

                // Assert
                Assert.True(result);
                Assert.Contains("Are you sure?", output.ToString());
            }

            //[Fact]
            //public void YesNo_ReturnsFalse_WhenUserInputsNo()
            //{
            //    // Arrange
            //    var dialog = new ConsoleUserDialog.ConsoleUserDialog();
            //    var input = new StringReader("no\n");
            //    var output = new StringWriter();
            //    Console.SetIn(input);
            //    Console.SetOut(output);

            //    // Act
            //    var result = dialog.YesNo("Do you want to proceed?");

            //    // Assert
            //    Assert.False(result);
            //    Assert.Contains("Do you want to proceed?", output.ToString());
            //}

            //[Fact]
            //public void YesNo_PromptsAgain_OnInvalidInput()
            //{
            //    // Arrange
            //    var dialog = new ConsoleUserDialog.ConsoleUserDialog();
            //    var input = new StringReader("maybe\nyes\n");
            //    var output = new StringWriter();
            //    Console.SetIn(input);
            //    Console.SetOut(output);

            //    // Act
            //    var result = dialog.YesNo("Confirm action:");

            //    // Assert
            //    Assert.True(result);
            //    var consoleOutput = output.ToString();
            //    Assert.Contains("Invalid input. Please try again.", consoleOutput);
            //    Assert.Equal(2, CountOccurrences(consoleOutput, "Please enter '(y)es' or '(n)o':"));
            //}

            //[Fact]
            //public void ContinueTimedOutYesNo_ReturnsTrue_WhenUserInputsYesBeforeCancellation()
            //{
            //    // Arrange
            //    var dialog = new ConsoleUserDialog.ConsoleUserDialog();
            //    var input = new StringReader("y\n");
            //    var output = new StringWriter();
            //    Console.SetIn(input);
            //    Console.SetOut(output);

            //    using var localCts = new CancellationTokenSource();
            //    using var cts = new CancellationTokenSource();

            //    // Act
            //    var result = dialog.ContinueTimedOutYesNo("Continue?", localCts.Token, cts.Token);

            //    // Assert
            //    Assert.True(result);
            //}

            //[Fact]
            //public void ContinueTimedOutYesNo_ReturnsFalse_WhenCancelled()
            //{
            //    // Arrange
            //    var dialog = new ConsoleUserDialog.ConsoleUserDialog();
            //    var output = new StringWriter();
            //    Console.SetOut(output);

            //    using var localCts = new CancellationTokenSource();
            //    using var cts = new CancellationTokenSource();

            //    // Simulate cancellation
            //    localCts.Cancel();

            //    // Act
            //    var result = dialog.ContinueTimedOutYesNo("Continue?", localCts.Token, cts.Token);

            //    // Assert
            //    Assert.False(result);
            //    Assert.Contains("Operation canceled.", output.ToString());
            //}

            //[Fact]
            //public async Task YesNoAsync_ReturnsTrue_WhenUserInputsYes()
            //{
            //    // Arrange
            //    var dialog = new ConsoleUserDialog.ConsoleUserDialog();
            //    var input = new StringReader("yes\n");
            //    var output = new StringWriter();
            //    Console.SetIn(input);
            //    Console.SetOut(output);
            //    var cts = new CancellationTokenSource();

            //    // Act
            //    var result = await dialog.YesNoAsync("Proceed?", cts.Token);

            //    // Assert
            //    Assert.True(result);
            //    Assert.Contains("Proceed?", output.ToString());
            //}

            //[Fact]
            //public async Task YesNoAsync_ReturnsFalse_WhenCancelled()
            //{
            //    // Arrange
            //    var dialog = new ConsoleUserDialog.ConsoleUserDialog();
            //    var output = new StringWriter();
            //    Console.SetOut(output);
            //    var cts = new CancellationTokenSource();

            //    // Simulate cancellation
            //    cts.Cancel();

            //    // Act
            //    var result = await dialog.YesNoAsync("Proceed?", cts.Token);

            //    // Assert
            //    Assert.False(result);
            //    Assert.Contains("Operation canceled.", output.ToString());
            //}

            //// Helper method to count occurrences of a substring in a string
            //private int CountOccurrences(string text, string substring)
            //{
            //    int count = 0;
            //    int index = 0;
            //    while ((index = text.IndexOf(substring, index)) != -1)
            //    {
            //        index += substring.Length;
            //        count++;
            //    }
            //    return count;
            //}
        }
    }
