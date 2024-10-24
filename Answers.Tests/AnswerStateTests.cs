using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Answers.Tests
{
    using Xunit;

    public class AnswerStateTests
    {
        [Fact]
        public void Constructor_ShouldSetDefaultValues()
        {
            // Arrange & Act
            var answerState = new AnswerState();

            // Assert
            Assert.True(answerState.IsSuccess);
            Assert.False(answerState.DialogConcluded);
            Assert.False(answerState.HasValueSet);
        }

        [Fact]
        public void IsSuccess_WhenSetToFalse_ShouldReturnFalse()
        {
            // Arrange
            var answerState = new AnswerState();

            // Act
            answerState.IsSuccess = false;

            // Assert
            Assert.False(answerState.IsSuccess);
        }

        [Fact]
        public void HasValueSet_WhenSetToTrue_ShouldReturnTrue()
        {
            // Arrange
            var answerState = new AnswerState();

            // Act
            answerState.HasValueSet = true;

            // Assert
            Assert.True(answerState.HasValueSet);
        }

        [Fact]
        public void ConcludeDialog_WhenCalled_ShouldSetDialogConcludedToTrue()
        {
            // Arrange
            var answerState = new AnswerState();

            // Act
            answerState.ConcludeDialog();

            // Assert
            Assert.True(answerState.DialogConcluded);
        }

        [Fact]
        public void Properties_ShouldBeIndependentlyMutable()
        {
            // Arrange
            var answerState = new AnswerState();

            // Act
            answerState.IsSuccess = false;
            answerState.HasValueSet = true;
            answerState.DialogConcluded = true;

            // Assert
            Assert.False(answerState.IsSuccess);
            Assert.True(answerState.HasValueSet);
            Assert.True(answerState.DialogConcluded);
        }
    }
}
