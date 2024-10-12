using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Answers;
using System;

namespace Answers.Tests
{
   

    public class AnswerGenericTests
    {
        [Fact]
        public void WithValue_ShouldSetAndReturnCorrectValue()
        {
            // Arrange
            var answer = new Answer<int>();

            // Act
            answer.WithValue(42);

            // Assert
            Assert.Equal(42, answer.GetValue());
        }

        [Fact]
        public void GetValue_ShouldThrowIfValueNotSet()
        {
            // Arrange
            var answer = new Answer<int>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => answer.GetValue());
        }

        [Fact]
        public void Out_ShouldReturnSuccessAndSetValue()
        {
            // Arrange
            var answer = new Answer<int>().WithValue(42);

            // Act
            bool success = answer.Out(out int result);

            // Assert
            Assert.True(success);
            Assert.Equal(42, result);
        }
    }

}
