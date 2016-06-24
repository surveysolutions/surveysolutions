using System;
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
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.Applications.Headquarters.DesignerQuestionnairesApiControllerTests
{
    internal class when_importing_questionnaire_from_designer_and_command_service_throws_not_a_questionnaire_exception : DesignerQuestionnairesApiControllerTestsContext
    {
        Establish context = () =>
        {
            importRequest = new ImportQuestionnaireRequest(){ Questionnaire = new DesignerQuestionnaireListViewItem()};

            var supportedVerstion = 1;

            var versionProvider = new Mock<ISupportedVersionProvider>();
            versionProvider.Setup(x => x.GetSupportedQuestionnaireVersion()).Returns(supportedVerstion);

            commandServiceException = new Exception();

            var commandService = Mock.Of<ICommandService>();
            Mock.Get(commandService)
                .Setup(cs => cs.Execute(it.IsAny<ICommand>(), it.IsAny<string>()))
                .Throws(commandServiceException);

            var zipUtilsMock = new Mock<IStringCompressor>();
            zipUtilsMock.Setup(_ => _.DecompressString<QuestionnaireDocument>(Moq.It.IsAny<string>()))
                .Returns(new QuestionnaireDocument());

            var restServiceMock = new Mock<IRestService>();
            restServiceMock.Setup(x => x.GetAsync<QuestionnaireCommunicationPackage>(Moq.It.IsAny<string>(), Moq.It.IsAny<Action<DownloadProgressChangedEventArgs>>(), Moq.It.IsAny<object>(), Moq.It.IsAny<RestCredentials>(), Moq.It.IsAny<CancellationToken?>()))
                .Returns(Task.FromResult(new QuestionnaireCommunicationPackage()));

            controller = CreateDesignerQuestionnairesApiController(commandService: commandService,
                supportedVersionProvider: versionProvider.Object, zipUtils: zipUtilsMock.Object, restService: restServiceMock.Object);
        };

        Because of = () => exception = Catch.Exception(() => controller.GetQuestionnaire(importRequest).GetAwaiter().GetResult());

        It should_rethrow_command_service_exception = () =>
            exception.ShouldEqual(commandServiceException);

        private static Exception exception;
        private static Exception commandServiceException;
        private static DesignerQuestionnairesApiController controller;
        private static ImportQuestionnaireRequest importRequest;
    }
}