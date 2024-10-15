using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Answers.Tests
{

    public class ValueRecordTests
    {
        [Fact]
        public void ValueRecord_ShouldStoreIntValue()
        {
            // Arrange
            var valueRecord = new ValueRecord<int>(42);

            // Act
            var value = valueRecord.GetValue();

            // Assert
            Assert.Equal(42, value);
        }

        [Fact]
        public void ValueRecord_ShouldStoreStringValue()
        {
            // Arrange
            var valueRecord = new ValueRecord<string>("TestValue");

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
            var valueRecord = new ValueRecord<CustomType>(customObject);

            // Act
            var value = valueRecord.GetValue();

            // Assert
            Assert.Equal(customObject, value);
        }

        [Fact]
        public void ValueRecord_GetValueFromInterface_ShouldReturnSameValue()
        {
            // Arrange
            var valueRecord = new ValueRecord<int>(100);

            // Act
            var interfaceValueRecord = (IValueRecord)valueRecord;
            var value = interfaceValueRecord.GetValue();

            // Assert
            Assert.Equal(100, value);
        }

        [Fact]
        public void ValueRecord_GetValueFromInterface_ShouldReturnSameStringValue()
        {
            // Arrange
            var valueRecord = new ValueRecord<string>("InterfaceValue");

            // Act
            var interfaceValueRecord = (IValueRecord)valueRecord;
            var value = interfaceValueRecord.GetValue();

            // Assert
            Assert.Equal("InterfaceValue", value);
        }

        [Fact]
        public void ValueRecord_GetValueFromInterface_ShouldReturnCustomObjectValue()
        {
            // Arrange
            var customObject = new CustomType { Id = 2, Name = "CustomObject" };
            var valueRecord = new ValueRecord<CustomType>(customObject);

            // Act
            var interfaceValueRecord = (IValueRecord)valueRecord;
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
