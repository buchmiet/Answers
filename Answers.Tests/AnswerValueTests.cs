using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Answers;
using Xunit;

namespace Answers.Tests
{

    public class AnswerValueTests
    {
        [Fact]
        public void Constructor_WhenValueIsNull_ShouldCreateInstance()
        {
            // Arrange & Act
            var answer = new AnswerValue<string>(null);

            // Assert
            Assert.NotNull(answer);
        }

        [Fact]
        public void GetValue_WhenStringValue_ShouldReturnCorrectValue()
        {
            // Arrange
            const string expectedValue = "test";
            var answer = new AnswerValue<string>(expectedValue);

            // Act
            var result = answer.GetValue();

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetValue_WhenIntValue_ShouldReturnCorrectValue()
        {
            // Arrange
            const int expectedValue = 42;
            var answer = new AnswerValue<int>(expectedValue);

            // Act
            var result = answer.GetValue();

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetValue_WhenCustomObjectValue_ShouldReturnCorrectValue()
        {
            // Arrange
            var expectedValue = new TestObject { Id = 1, Name = "Test" };
            var answer = new AnswerValue<TestObject>(expectedValue);

            // Act
            var result = answer.GetValue();

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void Interface_GetValue_ShouldReturnSameValueAsGenericMethod()
        {
            // Arrange
            const int expectedValue = 42;
            var answer = new AnswerValue<int>(expectedValue);

            // Act
            var genericResult = answer.GetValue();
            var interfaceResult = ((IAnswerValue)answer).GetValue();

            // Assert
            Assert.Equal(genericResult, interfaceResult);
        }

        [Fact]
        public void Record_ShouldImplementValueEquality()
        {
            // Arrange
            var answer1 = new AnswerValue<int>(42);
            var answer2 = new AnswerValue<int>(42);
            var answer3 = new AnswerValue<int>(43);

            // Assert
            Assert.Equal(answer1, answer2); // Same value should be equal
            Assert.NotEqual(answer1, answer3); // Different values should not be equal
        }

        [Fact]
        public void Record_ShouldImplementValueHashCode()
        {
            // Arrange
            var answer1 = new AnswerValue<int>(42);
            var answer2 = new AnswerValue<int>(42);

            // Assert
            Assert.Equal(answer1.GetHashCode(), answer2.GetHashCode());
        }

        [Fact]
        public void DifferentTypes_ShouldNotBeEqual()
        {
            // Arrange
            var intAnswer = new AnswerValue<int>(42);
            var stringAnswer = new AnswerValue<string>("42");

            // Assert
            Assert.NotEqual(intAnswer.GetHashCode(), stringAnswer.GetHashCode());
        }

        [Fact]
        public void Interface_ShouldWorkWithDifferentTypes()
        {
            // Arrange
            IAnswerValue intAnswer = new AnswerValue<int>(42);
            IAnswerValue stringAnswer = new AnswerValue<string>("test");
            IAnswerValue boolAnswer = new AnswerValue<bool>(true);

            // Act & Assert
            Assert.IsType<int>(intAnswer.GetValue());
            Assert.IsType<string>(stringAnswer.GetValue());
            Assert.IsType<bool>(boolAnswer.GetValue());
        }

        [Fact]
        public void GetValue_WithComplexType_ShouldMaintainReferenceEquality()
        {
            // Arrange
            var complexObject = new TestObject { Id = 1, Name = "Test" };
            var answer = new AnswerValue<TestObject>(complexObject);

            // Act
            var result = answer.GetValue();

            // Assert
            Assert.Same(complexObject, result); // Verifies reference equality
        }

        private class TestObject
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
