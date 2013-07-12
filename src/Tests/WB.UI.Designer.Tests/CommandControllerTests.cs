using System;
using Main.Core.Commands.Questionnaire.Group;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.UI.Designer.Code.Helpers;
using WB.UI.Designer.Controllers;
using WB.UI.Designer.Utils;

namespace WB.UI.Designer.Tests
{
    [TestFixture]
    public class CommandControllerTests
    {
        [Test]
        public void Execute_When_CommandService_throws_exception_with_inner_DomainExcetion_Then_()
        {
            // Arrange
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);

            var commandType = "UpdateGroup";
            var commandJSON = "some command";
            var updateGroupCommand = CreateInvalidUpdateGroupCommand();

            var commandService = Mock.Of<ICommandService>();
            Mock.Get(commandService).Setup(x => x.Execute(updateGroupCommand)).Throws(CreateTwoLevelException());

            var commandDeserializer = Mock.Of<ICommandDeserializer>(serializer => serializer.Deserialize(commandType, commandJSON) == updateGroupCommand);

            var controller = CreateCommandController(commandService: commandService, commandDeserializer: commandDeserializer);

            // Act
            var result = controller.Execute(commandType, commandJSON);
            
            // Assert
            Assert.That(result.Data, !Is.Null);
        }


        [Test]
        public void Execute_When_CommandService_throws_exception_with_inner_not_DomainExcetion_Then_exception_should_be_thrown()
        {
            // Arrange
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);

            var commandType = "UpdateGroup";
            var commandJSON = "some command";
            var updateGroupCommand = CreateInvalidUpdateGroupCommand();

            var commandService = Mock.Of<ICommandService>();
            Mock.Get(commandService).Setup(x => x.Execute(updateGroupCommand)).Throws(CreateThreeLevelException());

            var commandDeserializer = Mock.Of<ICommandDeserializer>(serializer => serializer.Deserialize(commandType, commandJSON) == updateGroupCommand);

            var controller = CreateCommandController(commandService: commandService, commandDeserializer: commandDeserializer);

            // Act
            var result = controller.Execute(commandType, commandJSON);

            // Assert
            Assert.That(result.Data, !Is.Null);
        }


        private Exception CreateTwoLevelException()
        {
            return new Exception("message", new DomainException(DomainExceptionType.GroupNotFound, "exception message"));
        }

        private Exception CreateThreeLevelException()
        {
            return new Exception("message", CreateTwoLevelException());
        }

        private static UpdateGroupCommand CreateInvalidUpdateGroupCommand(Guid? questionnaireId = null)
        {
            var qId = questionnaireId.HasValue ? questionnaireId.Value : Guid.NewGuid();

            var command = new UpdateGroupCommand(qId, Guid.NewGuid(), string.Empty, Propagate.None, string.Empty, string.Empty);

            return command;
        }

        private CommandController CreateCommandController(ICommandService commandService = null, ICommandDeserializer commandDeserializer = null, 
            IExpressionReplacer expressionReplacer = null, ILogger logReplacer = null)
        {
            return new CommandController(
                commandService ?? Mock.Of<ICommandService>(),
                commandDeserializer ?? Mock.Of<ICommandDeserializer>(),
                expressionReplacer ?? Mock.Of<IExpressionReplacer>());
        }
    }
}
