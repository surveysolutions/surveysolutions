using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Template;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.Applications.Headquarters.DesignerQuestionnairesApiControllerTests
{
    internal class when_importing_questionnaire_with_attachments : DesignerQuestionnairesApiControllerTestsContext
    {
        Establish context = () =>
        {
            importRequest = new ImportQuestionnaireRequest(){ Questionnaire = new DesignerQuestionnaireListViewItem()};

            var versionProvider = Setup.SupportedVersionProvider(1);

            var zipUtils = Setup.StringCompressor_Decompress(new QuestionnaireDocument() {Attachments = questionnaireAttachments});

            mockOfRestService.Setup(x =>
                x.DownloadFileAsync(Moq.It.IsAny<string>(), null, Moq.It.IsAny<RestCredentials>(), null)).Returns(Task.FromResult(new RestFile(new byte[] { 1 }, "image/png", "content id", 0, "file.png")));
            mockOfRestService.Setup(x => x.GetAsync<QuestionnaireCommunicationPackage>(Moq.It.IsAny<string>(), Moq.It.IsAny<Action<DownloadProgressChangedEventArgs>>(), Moq.It.IsAny<object>(), Moq.It.IsAny<RestCredentials>(), Moq.It.IsAny<CancellationToken?>()))
                .Returns(Task.FromResult(new QuestionnaireCommunicationPackage()));

            mockOfAttachmentContentService.Setup(x => x.HasAttachmentContent(questionnaireAttachments[0].ContentId)).Returns(true);
            mockOfAttachmentContentService.Setup(x => x.HasAttachmentContent(questionnaireAttachments[1].ContentId)).Returns(false);
            mockOfAttachmentContentService.Setup(x => x.HasAttachmentContent(questionnaireAttachments[2].ContentId)).Returns(false);

            controller = CreateDesignerQuestionnairesApiController(attachmentContentService: mockOfAttachmentContentService.Object,
                supportedVersionProvider: versionProvider, zipUtils: zipUtils, restService: mockOfRestService.Object);
        };

        Because of = () => controller.GetQuestionnaire(importRequest).GetAwaiter().GetResult();

        It should_download_and_save_only_not_existing_attachemt_contents = () =>
        {
            mockOfRestService.Verify(x=>x.DownloadFileAsync(Moq.It.IsAny<string>(), null, Moq.It.IsAny<RestCredentials>(), null), Times.Exactly(2));
            mockOfAttachmentContentService.Verify(x=>x.SaveAttachmentContent(questionnaireAttachments[0].ContentId, Moq.It.IsAny<string>(), Moq.It.IsAny<byte[]>()), Times.Never);
            mockOfAttachmentContentService.Verify(x => x.SaveAttachmentContent(questionnaireAttachments[1].ContentId, Moq.It.IsAny<string>(), Moq.It.IsAny<byte[]>()), Times.Once);
            mockOfAttachmentContentService.Verify(x => x.SaveAttachmentContent(questionnaireAttachments[2].ContentId, Moq.It.IsAny<string>(), Moq.It.IsAny<byte[]>()), Times.Once);
        };
        
        private static DesignerQuestionnairesApiController controller;
        private static ImportQuestionnaireRequest importRequest;
        private static readonly Mock<IRestService> mockOfRestService = new Mock<IRestService>();
        private static readonly Mock<IAttachmentContentService> mockOfAttachmentContentService = new Mock<IAttachmentContentService>();

        private static readonly List<Attachment> questionnaireAttachments =
            new List<Attachment>(new[]
            {
                Create.Entity.Attachment("Content 1"),
                Create.Entity.Attachment("Content 2"),
                Create.Entity.Attachment("Content 3")
            });
    }
}