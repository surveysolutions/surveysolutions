using System;
using Main.Core.Documents;
using Main.Core.Domain;
using Main.Core.Domain.Exceptions;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.GenericSubdomains.Logging;
using WB.UI.Designer.Controllers;
using WB.UI.Designer.Utils;
using WB.UI.Designer.Views.Questionnaire;
using WB.UI.Shared.Web.CommandDeserialization;
using WB.UI.Shared.Web.Membership;

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
            var responsibleId = Guid.NewGuid();
            var updateGroupCommand = CreateInvalidUpdateGroupCommand(responsibleId: responsibleId);

            var commandService = Mock.Of<ICommandService>();
            Mock.Get(commandService).Setup(x => x.Execute(updateGroupCommand)).Throws(CreateTwoLevelException());

            var commandDeserializer = Mock.Of<ICommandDeserializer>(serializer => serializer.Deserialize(commandType, commandJSON) == updateGroupCommand);

            var controller = CreateCommandController(commandService: commandService, commandDeserializer: commandDeserializer, responsibleId: responsibleId);

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
            var responsibleId = Guid.NewGuid();

            var updateGroupCommand = CreateInvalidUpdateGroupCommand(responsibleId: responsibleId);

            var commandService = Mock.Of<ICommandService>();
            Mock.Get(commandService).Setup(x => x.Execute(updateGroupCommand)).Throws(CreateThreeLevelException());

            var commandDeserializer = Mock.Of<ICommandDeserializer>(serializer => serializer.Deserialize(commandType, commandJSON) == updateGroupCommand);

            var controller = CreateCommandController(commandService: commandService, commandDeserializer: commandDeserializer, responsibleId: responsibleId);

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

        private static UpdateGroupCommand CreateInvalidUpdateGroupCommand(Guid responsibleId, Guid? questionnaireId = null)
        {
            var qId = questionnaireId.HasValue ? questionnaireId.Value : Guid.NewGuid();

            var command = new UpdateGroupCommand(qId, Guid.NewGuid(), string.Empty, Propagate.None, string.Empty, string.Empty, responsibleId: responsibleId);

            return command;
        }

        private CommandController CreateCommandController(Guid responsibleId, ICommandService commandService = null, ICommandDeserializer commandDeserializer = null,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory = null, ILogger logReplacer = null, IMembershipUserService userHelper = null)
        {
            var membershipUserService = Mock.Of<IMembershipUserService>();
            Mock.Get(membershipUserService).Setup(x => x.WebUser).Returns(new MembershipWebUser(Mock.Of<IMembershipHelper>()));
            Mock.Get(membershipUserService).Setup(x => x.WebUser.UserId).Returns(responsibleId);

            return new CommandController(
                commandService ?? Mock.Of<ICommandService>(),
                commandDeserializer ?? Mock.Of<ICommandDeserializer>(),
                userHelper ?? membershipUserService,
                questionnaireViewFactory ??
                    Mock.Of<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>(
                        _ => _.Load(It.IsAny<QuestionnaireViewInputModel>()) == new QuestionnaireView(new QuestionnaireDocument())));
        }
    }
}
