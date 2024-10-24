using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Answers;

namespace Answers.Tests
{


    using Xunit;
    using System;

    public class AnswerTests
    {
        #region Constructor and Basic Properties

        [Fact]
        public void Constructor_ShouldInitializeWithAction()
        {
            // Arrange & Act
            var answer = new Answer("TestAction");

            // Assert
            Assert.True(answer.IsSuccess);
            Assert.False(answer.HasValue);
            Assert.False(answer.DialogConcluded);
            Assert.Contains("TestAction", answer.Message);
        }

        [Fact]
        public void Prepare_ShouldCreateNewInstanceWithAction()
        {
            // Arrange & Act
            var answer = Answer.Prepare("TestAction");

            // Assert
            Assert.NotNull(answer);
            Assert.Contains("TestAction", answer.Message);
        }

        #endregion

        #region Value Management

        [Fact]
        public void WithValue_ShouldSetValueAndReturnSameInstance()
        {
            // Arrange
            var answer = new Answer("TestAction");
            const int testValue = 42;

            // Act
            var result = answer.WithValue(testValue);

            // Assert
            Assert.Same(answer, result);
            Assert.True(answer.HasValue);
            Assert.Equal(testValue, answer.GetValue<int>());
        }

        [Fact]
        public void AddValue_WhenSuccessState_ShouldAddValue()
        {
            // Arrange
            var answer = new Answer("TestAction");
            const string testValue = "test";

            // Act
            answer.AddValue(testValue);

            // Assert
            Assert.True(answer.HasValue);
            Assert.Equal(testValue, answer.GetValue<string>());
        }

        [Fact]
        public void AddValue_WhenErrorState_ShouldThrowException()
        {
            // Arrange
            var answer = new Answer("TestAction").Error("Error occurred");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => answer.AddValue("test"));
        }

        [Fact]
        public void GetValue_WhenValueNotSet_ShouldThrowException()
        {
            // Arrange
            var answer = new Answer("TestAction");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => answer.GetValue<string>());
        }

        [Fact]
        public void GetValue_WhenWrongType_ShouldThrowException()
        {
            // Arrange
            var answer = new Answer("TestAction").WithValue(42);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => answer.GetValue<string>());
        }

        [Fact]
        public void Out_WhenValueSet_ShouldSetOutParameterAndReturnTrue()
        {
            // Arrange
            var answer = new Answer("TestAction").WithValue(42);

            // Act
            var success = answer.Out(out int value);

            // Assert
            Assert.True(success);
            Assert.Equal(42, value);
        }

        [Fact]
        public void Out_WhenValueNotSet_ShouldThrowException()
        {
            // Arrange
            var answer = new Answer("TestAction");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => answer.Out(out int value));
        }

        #endregion

        #region Error Handling

        [Fact]
        public void Error_WhenNoValue_ShouldSetErrorState()
        {
            // Arrange
            var answer = new Answer("TestAction");

            // Act
            answer.Error("Error message");

            // Assert
            Assert.False(answer.IsSuccess);
            Assert.Contains("Error message", answer.Message);
        }

        [Fact]
        public void Error_WhenAlreadyHasValue_ShouldThrowException()
        {
            // Arrange
            var answer = new Answer("TestAction").WithValue(42);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => answer.Error("Error message"));
        }

        [Fact]
        public void Error_WhenAlreadyInErrorState_ShouldThrowException()
        {
            // Arrange
            var answer = new Answer("TestAction").Error("First error");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => answer.Error("Second error"));
        }

        #endregion

        #region Dialog Management

        [Fact]
        public void ConcludeDialog_ShouldSetDialogConcludedToTrue()
        {
            // Arrange
            var answer = new Answer("TestAction");

            // Act
            answer.ConcludeDialog();

            // Assert
            Assert.True(answer.DialogConcluded);
        }

        [Fact]
        public void DialogConcluded_ShouldBeSettableDirectly()
        {
            // Arrange
            var answer = new Answer("TestAction");

            // Act
            answer.DialogConcluded = true;

            // Assert
            Assert.True(answer.DialogConcluded);
        }

        #endregion

        #region Answer Attachment

        [Fact]
        public void Attach_WhenBothHaveNoValue_ShouldMergeSuccessfully()
        {
            // Arrange
            var answer1 = new Answer("Action1");
            var answer2 = new Answer("Action2");

            // Act
            var result = answer1.Attach(answer2);

            // Assert
            Assert.Same(answer1, result);
            Assert.Contains("Action1", result.Message);
            Assert.Contains("Action2", result.Message);
        }

        [Fact]
        public void Attach_WhenSecondHasValue_ShouldTransferValue()
        {
            // Arrange
            var answer1 = new Answer("Action1");
            var answer2 = new Answer("Action2").WithValue(42);

            // Act
            answer1.Attach(answer2);

            // Assert
            Assert.True(answer1.HasValue);
            Assert.Equal(42, answer1.GetValue<int>());
        }

        [Fact]
        public void Attach_WhenBothHaveValues_ShouldThrowException()
        {
            // Arrange
            var answer1 = new Answer("Action1").WithValue(42);
            var answer2 = new Answer("Action2").WithValue("test");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => answer1.Attach(answer2));
        }

        [Fact]
        public void Attach_WhenSecondInErrorState_AndFirstHasValue_ShouldThrowException()
        {
            // Arrange
            var answer1 = new Answer("Action1").WithValue(42);
            var answer2 = new Answer("Action2").Error("Error occurred");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => answer1.Attach(answer2));
        }

        [Fact]
        public void Attach_ShouldCombineDialogConcludedStates()
        {
            // Arrange
            var answer1 = new Answer("Action1");
            var answer2 = new Answer("Action2");
            answer2.ConcludeDialog();

            // Act
            answer1.Attach(answer2);

            // Assert
            Assert.True(answer1.DialogConcluded);
        }

        #endregion

        #region Complex Scenarios

        [Fact]
        public void ComplexScenario_SuccessfulChain()
        {
            // Arrange
            var answer1 = new Answer("Action1").WithValue(42);
            var answer2 = new Answer("Action2");
            var answer3 = new Answer("Action3");

            // Act
            answer2.Attach(answer3).Attach(answer1);

            // Assert
            Assert.True(answer2.IsSuccess);
            Assert.True(answer2.HasValue);
            Assert.Equal(42, answer2.GetValue<int>());
            Assert.Contains("Action1", answer2.Message);
            Assert.Contains("Action2", answer2.Message);
            Assert.Contains("Action3", answer2.Message);
        }

        [Fact]
        public void ComplexScenario_ErrorPropagation()
        {
            // Arrange
            var answer1 = new Answer("Action1");
            var answer2 = new Answer("Action2").Error("Error in Action2");
            var answer3 = new Answer("Action3");

            // Act
            answer1.Attach(answer3);
            answer1.Attach(answer2);

            // Assert
            Assert.False(answer1.IsSuccess); // Stan błędu powinien się propagować
            Assert.Contains("Action1", answer1.Message);
            Assert.Contains("Action2", answer1.Message);
            Assert.Contains("Action3", answer1.Message);
            Assert.Contains("Error in Action2", answer1.Message);
        }

        [Fact]
        public void ToString_ShouldReturnMessage()
        {
            // Arrange
            var answer = new Answer("TestAction");

            // Act
            var result = answer.ToString();

            // Assert
            Assert.Equal(answer.Message, result);
        }

        #endregion
    }

}
