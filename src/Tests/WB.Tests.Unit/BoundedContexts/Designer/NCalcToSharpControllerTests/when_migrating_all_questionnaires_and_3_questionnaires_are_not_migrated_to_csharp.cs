using System;
using System.Net;
using System.Net.Http;
using Machine.Specifications;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.UI.Designer.Api;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.NCalcToSharpControllerTests
{
    internal class when_migrating_all_questionnaires_and_3_questionnaires_are_not_migrated_to_csharp
    {
        Establish context = () =>
        {
            commandServiceMock = new Mock<ICommandService>();

            var questionnaireInfoViewFactory = Mock.Of<IQuestionnaireInfoViewFactory>(factory
                => factory.CountQuestionnairesNotMigratedToCSharp() == 3
                    && factory.GetQuestionnairesNotMigratedToCSharp() == new[] { questionnaire1, questionnaire2, questionnaire3 });

            controller = Create.NCalcToSharpController(
                questionnaireInfoViewFactory: questionnaireInfoViewFactory, commandService: commandServiceMock.Object);
        };

        Because of = () =>
            result = controller.MigrateAll();

        It should_return_http_response_OK = () =>
            result.StatusCode.ShouldEqual(HttpStatusCode.OK);

        It should_execute_MigrateExpressionsToCSharp_command_with_id_of_first_not_migrated_questionnaire = () =>
            commandServiceMock.Verify(
                service => service.Execute(
                    Moq.It.Is<MigrateExpressionsToCSharp>(command => command.QuestionnaireId == questionnaire1),
                    Moq.It.IsAny<string>()),
                Times.Once);

        It should_execute_MigrateExpressionsToCSharp_command_with_id_of_second_not_migrated_questionnaire = () =>
            commandServiceMock.Verify(
                service => service.Execute(
                    Moq.It.Is<MigrateExpressionsToCSharp>(command => command.QuestionnaireId == questionnaire2),
                    Moq.It.IsAny<string>()),
                Times.Once);

        It should_execute_MigrateExpressionsToCSharp_command_with_id_of_third_not_migrated_questionnaire = () =>
            commandServiceMock.Verify(
                service => service.Execute(
                    Moq.It.Is<MigrateExpressionsToCSharp>(command => command.QuestionnaireId == questionnaire3),
                    Moq.It.IsAny<string>()),
                Times.Once);

        It should_not_execute_MigrateExpressionsToCSharp_command_with_any_other_id = () =>
            commandServiceMock.Verify(
                service => service.Execute(
                    Moq.It.Is<MigrateExpressionsToCSharp>(command =>
                        command.QuestionnaireId != questionnaire1 &&
                            command.QuestionnaireId != questionnaire2 &&
                            command.QuestionnaireId != questionnaire3),
                    Moq.It.IsAny<string>()),
                Times.Never);

        private static HttpResponseMessage result;
        private static NCalcToSharpController controller;
        private static Guid questionnaire1 = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionnaire2 = Guid.Parse("22222222222222222222222222222222");
        private static Guid questionnaire3 = Guid.Parse("33333333333333333333333333333333");
        private static Mock<ICommandService> commandServiceMock;
    }
}