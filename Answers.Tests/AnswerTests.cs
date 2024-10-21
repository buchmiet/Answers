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
            string action = "Test Action";
            var answer = Answer.Prepare(action);

            Assert.True(answer.IsSuccess);
            Assert.Equal(action, answer.Message);
        }


        [Fact]
        public void Attach_ShouldCombineMessagesAndStates()
        {
            var answer1 = Answer.Prepare("Action 1");
            var answer2 = Answer.Prepare("Action 2");

            var combinedAnswer = answer1.Attach(answer2);

            Assert.True(combinedAnswer.IsSuccess);
            Assert.Contains("Action 1", combinedAnswer.Message);
            Assert.Contains("Action 2", combinedAnswer.Message);
        }

        [Fact]
        public void Error_ShouldSetErrorStateAndMessage()
        {
            var answer = Answer.Prepare("Initial Action");
            answer.Error("Error Message");

            Assert.False(answer.IsSuccess);
            Assert.Contains("Error Message", answer.Message);
        }

        [Fact]
        public void ConcludeDialog_ShouldSetDialogConcludedToTrue()
        {
            var answer = Answer.Prepare("Action");
            answer.ConcludeDialog();

            Assert.True(answer.DialogConcluded);
        }

        [Fact]
        public void WithValue_ShouldSetValueAndHasValueState()
        {
            var answer = Answer.Prepare("Action").WithValue(42);

            Assert.True(answer.HasValue);
            Assert.Equal(42, answer.GetValue<int>());
        }

        [Fact]
        public void GetValue_ShouldThrowWhenNoValueSet()
        {
            var answer = Answer.Prepare("Action");

            Assert.Throws<InvalidOperationException>(() => answer.GetValue<int>());
        }

        [Fact]
        public void GetValue_ShouldThrowWhenWrongTypeRequested()
        {
            var answer = Answer.Prepare("Action").WithValue(42);

            Assert.Throws<InvalidOperationException>(() => answer.GetValue<string>());
        }

        [Fact]
        public void Out_ShouldReturnTrueAndSetValueWhenSuccessful()
        {
            var answer = Answer.Prepare("Action").WithValue(42);

            bool result = answer.Out(out int value);

            Assert.True(result);
            Assert.Equal(42, value);
        }

        [Fact]
        public void Out_ShouldThroweWhenNotSuccessful()
        {
            var answer = Answer.Prepare("Action").Error("Some error");

            Assert.Throws<InvalidOperationException>(() => answer.Out(out int value));

     
        }

        [Fact]
        public void ToString_ShouldReturnMessage()
        {
            var answer = Answer.Prepare("Test Action");

            Assert.Equal(answer.Message, answer.ToString());
        }

        [Fact]
        public void IsSuccess_ShouldBeTrueByDefault()
        {
            var answer = new Answer("IsSuccess_ShouldBeTrueByDefault");

            Assert.True(answer.IsSuccess);
        }

     

        [Fact]
        public void DialogConcluded_ShouldBeFalseByDefault()
        {
            var answer = new Answer("DialogConcluded_ShouldBeFalseByDefault");

            Assert.False(answer.DialogConcluded);
        }

        [Fact]
        public void DialogConcluded_ShouldBeSettable()
        {
            var answer = new Answer("DialogConcluded_ShouldBeSettable");
            answer.DialogConcluded = true;

            Assert.True(answer.DialogConcluded);
        }

        [Fact]
        public void Attach_ShouldReturnFalseIsSuccessWhenOneAnswerFails()
        {
            var answer1 = Answer.Prepare("Action 1");
            var answer2 = Answer.Prepare("Action 2").Error("Error");

            var combinedAnswer = answer1.Attach(answer2);

            Assert.False(combinedAnswer.IsSuccess);
        }

        [Fact]
        public void Error_CanBeSetOnlyOnce()
        {
            var answer = Answer.Prepare("Initial action");

            answer.Error("First error");
            var ex = Assert.Throws<InvalidOperationException>(() => answer.Error("Second error"));

            Assert.Equal("Error can only be set once.", ex.Message);
        }

        [Fact]
        public void CannotAddValueWhenIsSuccessIsFalse()
        {
            var answer = Answer.Prepare("Initial action");

            answer.Error("Some error");

            Assert.Throws<InvalidOperationException>(() => answer.AddValue(42));
        }

        [Fact]
        public void CannotSetIsSuccessToFalseIfValueExists()
        {
            var answer = Answer.Prepare("Initial action");
            answer.AddValue(42);

            Assert.Throws<InvalidOperationException>(() => answer.Error("Trying to set error after value is set"));
        }

      


    }

}
