using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Answers.Tests
{

    public class AnswerAnswerValueTests
    {
        [Fact]
        public void ValueRecord_ShouldStoreIntValue()
        {
            // Arrange
            var valueRecord = new AnswerAnswerValue<int>(42);

            // Act
            var value = valueRecord.GetValue();

            // Assert
            Assert.Equal(42, value);
        }

        [Fact]
        public void ValueRecord_ShouldStoreStringValue()
        {
            // Arrange
            var valueRecord = new AnswerAnswerValue<string>("TestValue");

            // Act
            var value = valueRecord.GetValue();

            // Assert
            Assert.Equal("TestValue", value);
        }

        [Fact]
        public void ValueRecord_ShouldStoreCustomObjectValue()
        {
            // Arrange
            var customObject = new CustomType { Id = 1, Name = "TestName" };
            var valueRecord = new AnswerAnswerValue<CustomType>(customObject);

            // Act
            var value = valueRecord.GetValue();

            // Assert
            Assert.Equal(customObject, value);
        }

        [Fact]
        public void ValueRecord_GetValueFromInterface_ShouldReturnSameValue()
        {
            // Arrange
            var valueRecord = new AnswerAnswerValue<int>(100);

            // Act
            var interfaceValueRecord = (IAnswerValue)valueRecord;
            var value = interfaceValueRecord.GetValue();

            // Assert
            Assert.Equal(100, value);
        }

        [Fact]
        public void ValueRecord_GetValueFromInterface_ShouldReturnSameStringValue()
        {
            // Arrange
            var valueRecord = new AnswerAnswerValue<string>("InterfaceValue");

            // Act
            var interfaceValueRecord = (IAnswerValue)valueRecord;
            var value = interfaceValueRecord.GetValue();

            // Assert
            Assert.Equal("InterfaceValue", value);
        }

        [Fact]
        public void ValueRecord_GetValueFromInterface_ShouldReturnCustomObjectValue()
        {
            // Arrange
            var customObject = new CustomType { Id = 2, Name = "CustomObject" };
            var valueRecord = new AnswerAnswerValue<CustomType>(customObject);

            // Act
            var interfaceValueRecord = (IAnswerValue)valueRecord;
            var value = interfaceValueRecord.GetValue();

            // Assert
            Assert.Equal(customObject, value);
        }
    }

    public class CustomType
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            return obj is CustomType other &&
                   Id == other.Id &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name);
        }
    }



}
