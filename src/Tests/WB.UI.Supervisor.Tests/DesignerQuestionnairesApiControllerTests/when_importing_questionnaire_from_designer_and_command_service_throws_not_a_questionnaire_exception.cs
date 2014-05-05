using System;
using System.ServiceModel;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.UI.Supervisor.Controllers;
using WB.UI.Supervisor.DesignerPublicService;
using It = Machine.Specifications.It;
using it = Moq.It;
using QuestionnaireVersion = WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects.QuestionnaireVersion;

namespace Web.Supervisor.Tests.DesignerQuestionnairesApiControllerTests
{
    internal class when_importing_questionnaire_from_designer_and_command_service_throws_not_a_questionnaire_exception : DesignerQuestionnairesApiControllerTestsContext
    {
        private Establish context = () =>
        {
            importRequest = new DesignerQuestionnairesApiController.ImportQuestionnaireRequest();

            var supportedVerstion = new QuestionnaireVersion(1, 2, 3);
            var versionProvider = Mock.Of<ISupportedVersionProvider>(x => x.GetSupportedQuestionnaireVersion() == supportedVerstion);

            commandServiceException = new Exception();

            var commandService = Mock.Of<ICommandService>();
            Mock.Get(commandService)
                .Setup(cs => cs.Execute(it.IsAny<ICommand>(), it.IsAny<string>()))
                .Throws(commandServiceException);

            controller = CreateDesignerQuestionnairesApiController(commandService: commandService,
                supportedVersionProvider: versionProvider);
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