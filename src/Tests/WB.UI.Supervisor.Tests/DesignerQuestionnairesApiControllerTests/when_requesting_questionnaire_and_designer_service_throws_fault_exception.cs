using System;
using System.ServiceModel;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Questionnaire.Core.Web.Helpers;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Supervisor.Controllers;
using WB.UI.Supervisor.DesignerPublicService;
using WB.UI.Supervisor.Models;
using It = Machine.Specifications.It;
using QuestionnaireVersion = WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects.QuestionnaireVersion;

namespace WB.UI.Supervisor.Tests.DesignerQuestionnairesApiControllerTests
{
    internal class when_requesting_questionnaire_and_designer_service_throws_fault_exception : DesignerQuestionnairesApiControllerTestsContext
    {
        Establish context = () =>
        {
            var supportedVerstion = new QuestionnaireVersion(1, 2, 3);
            request = new DesignerQuestionnairesApiController.ImportQuestionnaireRequest{ QuestionnaireId = questionnaireId };

            var versionProvider = Mock.Of<ISupportedVersionProvider>(x => x.GetSupportedQuestionnaireVersion() == supportedVerstion);

            var service = new Mock<IPublicService>();

            service
                .Setup(x => x.DownloadQuestionnaire(Moq.It.IsAny<DownloadQuestionnaireRequest>()))
                .Callback((DownloadQuestionnaireRequest r) => downloadRequest = r)
                .Throws(new FaultException(someFaultReason));

            controller = CreateDesignerQuestionnairesApiController(
                getDesignerService: x => service.Object,
                supportedVersionProvider: versionProvider);
        };

        Because of = () =>
            response = controller.GetQuestionnaire(request);

        It should_handle_exception_and_set_response_status_IsSuccess_in_true = () => 
            response.IsSuccess.ShouldBeTrue();

        It should_handle_exception_and_set_ImportError_in_someFaultReason = () =>
            response.ImportError.ShouldEqual(someFaultReason);

        It should_set_questionnaireId_in_downloadRequest = () =>
            downloadRequest.QuestionnaireId.ShouldEqual(questionnaireId);

        It should_set_Major_in_1_for_supportedQuestionnaireVersion_in_downloadRequest= () =>
            downloadRequest.SupportedQuestionnaireVersion.Major.ShouldEqual(1);

        It should_set_Minor_in_2_for_supportedQuestionnaireVersion_in_downloadRequest = () =>
            downloadRequest.SupportedQuestionnaireVersion.Minor.ShouldEqual(2);

        It should_set_Patch_in_3_for_supportedQuestionnaireVersion_in_downloadRequest = () =>
            downloadRequest.SupportedQuestionnaireVersion.Patch.ShouldEqual(3);

        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static string someFaultReason = "some fault reason";
        private static DownloadQuestionnaireRequest downloadRequest;
        private static DesignerQuestionnairesApiController controller;
        private static DesignerQuestionnairesApiController.ImportQuestionnaireRequest request;
        private static QuestionnaireVerificationResponse response;
    }
}