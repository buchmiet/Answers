using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Answers;

namespace Answers.Tests
{
  

    public class AnswerTests
    {
        [Fact]
        public void Prepare_ShouldCreateSuccessAnswerWithMessage()
        {
            // Arrange
            string action = "Test Action";

            // Act
            var answer = Answer.Prepare(action);

            // Assert
            Assert.True(answer.IsSuccess);
            Assert.Equal(action, answer.Message);
        }

        [Fact]
        public void TimedOut_ShouldSetTimedOutState()
        {
            // Act
            var answer = Answer.TimedOut();

            // Assert
            Assert.True(answer.IsTimedOut);
        }

        [Fact]
        public void Attach_ShouldCombineMessagesAndStates()
        {
            // Arrange
            var answer1 = Answer.Prepare("Action 1");
            var answer2 = Answer.Prepare("Action 2");

            // Act
            var combinedAnswer = answer1.Attach(answer2);

            // Assert
            Assert.True(combinedAnswer.IsSuccess);
            Assert.Contains("Action 1", combinedAnswer.Message);
            Assert.Contains("Action 2", combinedAnswer.Message);
        }
    }

}
