using System;
using System.Net;
using System.Threading;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Template;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.DesignerQuestionnairesApiControllerTests
{
    internal class when_requesting_questionnaire_and_designer_service_throws_fault_exception : DesignerQuestionnairesApiControllerTestsContext
    {
        Establish context = () =>
        {
            var supportedVerstion = new Version(1, 2, 3);

            request = new ImportQuestionnaireRequest
            {
                Questionnaire = new DesignerQuestionnaireListViewItem() {Id = questionnaireId}
            };

            var versionProvider = new Mock<ISupportedVersionProvider>();
            versionProvider.Setup(x => x.GetSupportedQuestionnaireVersion()).Returns(supportedVerstion);

            var service = new Mock<IRestService>();

            service
                .Setup(x => x.PostAsync<QuestionnaireCommunicationPackage>(Moq.It.IsAny<string>(), Moq.It.IsAny<object>(), Moq.It.IsAny<RestCredentials>()))
                .Throws(new RestException(someFaultReason, HttpStatusCode.Unauthorized));

            controller = CreateDesignerQuestionnairesApiController(
                restService: service.Object,
                supportedVersionProvider: versionProvider.Object);
        };

        Because of = async () =>
            response = await controller.GetQuestionnaire(request);

        It should_handle_exception_and_set_response_status_IsSuccess_in_true = () => 
            response.IsSuccess.ShouldBeTrue();

        It should_handle_exception_and_set_ImportError_in_someFaultReason = () =>
            response.ImportError.ShouldEqual(someFaultReason);

        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static string someFaultReason = "some fault reason";
        private static DesignerQuestionnairesApiController controller;
        private static ImportQuestionnaireRequest request;
        private static QuestionnaireVerificationResponse response;
    }
}