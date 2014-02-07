using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using Web.Supervisor.Controllers;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace Web.Supervisor.Tests.DesignerQuestionnairesApiControllerTests
{
    internal class when_importing_questionnaire_from_designer_and_command_service_throws_not_a_questionnaire_exception : DesignerQuestionnairesApiControllerTestsContext
    {
        Establish context = () =>
        {
            importRequest = new DesignerQuestionnairesApiController.ImportQuestionnaireRequest();

            commandServiceException = new Exception();

            var commandService = Mock.Of<ICommandService>();
            Mock.Get(commandService)
                .Setup(service => service.Execute(it.IsAny<ICommand>()))
                .Throws(commandServiceException);

            controller = CreateDesignerQuestionnairesApiController(commandService: commandService);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                controller.GetQuestionnaire(importRequest));

        It should_rethrow_command_service_exception = () =>
            exception.ShouldEqual(commandServiceException);

        private static Exception exception;
        private static Exception commandServiceException;
        private static DesignerQuestionnairesApiController controller;
        private static DesignerQuestionnairesApiController.ImportQuestionnaireRequest importRequest;
    }
}