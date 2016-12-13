using System;
using System.Net;
using System.Threading;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Template;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;
using DownloadProgressChangedEventArgs = WB.Core.GenericSubdomains.Portable.Implementation.DownloadProgressChangedEventArgs;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.DesignerQuestionnairesApiControllerTests
{
    internal class when_requesting_questionnaire_and_designer_service_throws_fault_exception : DesignerQuestionnairesApiControllerTestsContext
    {
        Establish context = () =>
        {
            var supportedVerstion = 1;

            request = new ImportQuestionnaireRequest
            {
                Questionnaire = new DesignerQuestionnaireListViewItem() {Id = questionnaireId}
            };

            var versionProvider = new Mock<ISupportedVersionProvider>();
            versionProvider.Setup(x => x.GetSupportedQuestionnaireVersion()).Returns(supportedVerstion);

            var service = new Mock<IRestService>();

            service
                .Setup(x => x.GetAsync<QuestionnaireCommunicationPackage>(Moq.It.IsAny<string>(), Moq.It.IsAny<Action<DownloadProgressChangedEventArgs>>(), Moq.It.IsAny<object>(), Moq.It.IsAny<RestCredentials>(), Moq.It.IsAny<CancellationToken?>()))
                .Throws(new RestException(someFaultReason, HttpStatusCode.Unauthorized));

            controller = CreateDesignerQuestionnairesApiController(
                restService: service.Object,
                supportedVersionProvider: versionProvider.Object);
        };

        Because of = async () =>
            response = await controller.GetQuestionnaire(request);

        It should_handle_exception_and_set_response_status_IsSuccess_in_true = () => 
            response.IsSuccess.ShouldBeFalse();

        It should_handle_exception_and_set_ImportError_in_someFaultReason = () =>
            response.ImportError.ShouldEqual(someFaultReason);

        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static string someFaultReason = "some fault reason";
        private static DesignerQuestionnairesApiController controller;
        private static ImportQuestionnaireRequest request;
        private static QuestionnaireImportResult response;
    }
}