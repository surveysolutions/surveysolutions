using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Commanding;

using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.UI.Headquarters.Tests.DesignerQuestionnairesApiControllerTests
{
    internal class when_importing_questionnaire_from_designer_and_command_service_throws_not_a_questionnaire_exception : DesignerQuestionnairesApiControllerTestsContext
    {
        private Establish context = () =>
        {
            importRequest = new ImportQuestionnaireRequest();

            var supportedVerstion = new ApplicationVersionSettings()
            {
                SupportedQuestionnaireVersionMajor = 1,
                SupportedQuestionnaireVersionMinor = 2,
                SupportedQuestionnaireVersionPatch = 3
            };
            var versionProvider = new Mock<ISupportedVersionProvider>();
            versionProvider.Setup(x => x.GetSupportedQuestionnaireVersion()).Returns(supportedVerstion);

            commandServiceException = new Exception();

            var commandService = Mock.Of<ICommandService>();
            Mock.Get(commandService)
                .Setup(cs => cs.Execute(it.IsAny<ICommand>(), it.IsAny<string>()))
                .Throws(commandServiceException);

            controller = CreateDesignerQuestionnairesApiController(commandService: commandService,
                supportedVersionProvider: versionProvider.Object);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                controller.GetQuestionnaire(importRequest));

        It should_rethrow_command_service_exception = () =>
            exception.ShouldEqual(commandServiceException);

        private static Exception exception;
        private static Exception commandServiceException;
        private static DesignerQuestionnairesApiController controller;
        private static ImportQuestionnaireRequest importRequest;
    }
}