using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Commanding;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Template;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Headquarters.Controllers;
using DownloadProgressChangedEventArgs = WB.Core.GenericSubdomains.Portable.Implementation.DownloadProgressChangedEventArgs;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.Applications.Headquarters.DesignerQuestionnairesApiControllerTests
{
    internal class when_importing_questionnaire_and_server_return_expectation_failed_status : DesignerQuestionnairesApiControllerTestsContext
    {
        Establish context = () =>
        {
            exprectedErrorMessageFromServer = "expected error message from server";
            importRequest = new ImportQuestionnaireRequest(){ Questionnaire = new DesignerQuestionnaireListViewItem()};

            var versionProvider = Setup.SupportedVersionProvider(ApiVersion.MaxQuestionnaireVersion);

            var mockOfRestService = new Mock<IRestService>();
            mockOfRestService.Setup(x => x.GetAsync<QuestionnaireCommunicationPackage>(Moq.It.IsAny<string>(), Moq.It.IsAny<Action<DownloadProgressChangedEventArgs>>(), Moq.It.IsAny<object>(), Moq.It.IsAny<RestCredentials>(), Moq.It.IsAny<CancellationToken?>()))
                .Throws(new RestException(exprectedErrorMessageFromServer, HttpStatusCode.ExpectationFailed));

            controller = CreateDesignerQuestionnairesApiController(
                supportedVersionProvider: versionProvider, restService: mockOfRestService.Object);
        };

        Because of = () => response = controller.GetQuestionnaire(importRequest).GetAwaiter().GetResult();

        It should_response_import_error_has_specified_error_message_from_server = () => response.ImportError.ShouldEqual(exprectedErrorMessageFromServer);

        private static QuestionnaireImportResult response;
        private static DesignerQuestionnairesApiController controller;
        private static ImportQuestionnaireRequest importRequest;

        private static string exprectedErrorMessageFromServer;
    }
}