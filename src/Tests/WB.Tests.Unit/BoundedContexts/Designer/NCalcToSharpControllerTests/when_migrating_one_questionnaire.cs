using System;
using System.Net;
using System.Net.Http;
using Machine.Specifications;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Api;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.NCalcToSharpControllerTests
{
    internal class when_migrating_one_questionnaire
    {
        Establish context = () =>
        {
            commandServiceMock = new Mock<ICommandService>();

            controller = Create.NCalcToSharpController(commandService: commandServiceMock.Object);
        };

        Because of = () =>
            result = controller.MigrateOne(Create.OneQuestionnaireModel(questionnaireId));

        It should_return_http_response_OK = () =>
            result.StatusCode.ShouldEqual(HttpStatusCode.OK);

        It should_execute_MigrateExpressionsToCSharp_command_with_id_of_specified_questionnaire = () =>
            commandServiceMock.Verify(
                service => service.Execute(
                    Moq.It.Is<MigrateExpressionsToCSharp>(command => command.QuestionnaireId == questionnaireId),
                    Moq.It.IsAny<string>()),
                Times.Once);

        private static HttpResponseMessage result;
        private static NCalcToSharpController controller;
        private static Guid questionnaireId = Guid.Parse("77777777777777777777777777777777");
        private static Mock<ICommandService> commandServiceMock;
    }
}