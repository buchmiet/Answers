using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Answers.Tests
{
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class MessageAggregatorTests
    {
        [Fact]
        public void NewMessageAggregator_ShouldHaveEmptyActions()
        {
            // Arrange
            var aggregator = new MessageAggregator();

            // Assert
            Assert.Empty(aggregator.Actions);
        }

        [Fact]
        public void NewMessageAggregator_MessageShouldBeEmpty()
        {
            // Arrange
            var aggregator = new MessageAggregator();

            // Assert
            Assert.Equal(string.Empty, aggregator.Message);
        }

        [Fact]
        public void AddAction_ShouldAddActionToList()
        {
            // Arrange
            var aggregator = new MessageAggregator();
            var action = "Action1";

            // Act
            aggregator.AddAction(action);

            // Assert
            Assert.Single(aggregator.Actions);
            Assert.Equal(action, aggregator.Actions[0]);
        }

        [Fact]
        public void AddAction_ShouldUpdateMessage()
        {
            // Arrange
            var aggregator = new MessageAggregator();
            var action = "Action1";

            // Act
            aggregator.AddAction(action);

            // Assert
            Assert.Equal("Action1", aggregator.Message);
        }

        [Fact]
        public void AddMultipleActions_ShouldAddActionsToList()
        {
            // Arrange
            var aggregator = new MessageAggregator();
            var actions = new List<string> { "Action1", "Action2", "Action3" };

            // Act
            aggregator.AddActions(actions);

            // Assert
            Assert.Equal(3, aggregator.Actions.Count);
            Assert.Equal(actions, aggregator.Actions);
        }

        [Fact]
        public void AddMultipleActions_ShouldUpdateMessage()
        {
            // Arrange
            var aggregator = new MessageAggregator();
            var actions = new List<string> { "Action1", "Action2", "Action3" };

            // Act
            aggregator.AddActions(actions);

            // Assert
            Assert.Equal("Action1 > Action2 > Action3", aggregator.Message);
        }

        [Fact]
        public void SetConnector_ShouldUpdateMessageSeparator()
        {
            // Arrange
            var aggregator = new MessageAggregator();
            var actions = new List<string> { "Action1", "Action2" };

            // Act
            aggregator.AddActions(actions);
            aggregator.SetConnector(" | ");

            // Assert
            Assert.Equal("Action1 | Action2", aggregator.Message);
        }

        [Fact]
        public void AddEmptyAction_ShouldThrowArgumentException()
        {
            // Arrange
            var aggregator = new MessageAggregator();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => aggregator.AddAction(""));
            
        }

        [Fact]
        public void AddNullAction_ShouldThrowArgumentNullException()
        {
            // Arrange
            var aggregator = new MessageAggregator();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => aggregator.AddAction(null));
        }

        [Fact]
        public void AddNullActions_ShouldThrowArgumentNullException()
        {
            // Arrange
            var aggregator = new MessageAggregator();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => aggregator.AddActions(null));
        }
    }

}
