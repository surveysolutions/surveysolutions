using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Commanding;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Template;
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

            var supportedVerstion = new Version(1,2,3);

            var versionProvider = new Mock<ISupportedVersionProvider>();
            versionProvider.Setup(x => x.GetSupportedQuestionnaireVersion()).Returns(supportedVerstion);

            var zipUtilsMock = new Mock<IStringCompressor>();

            zipUtilsMock.Setup(_ => _.DecompressString<QuestionnaireDocument>(Moq.It.IsAny<string>()))
                .Returns(new QuestionnaireDocument() { Attachments = questionnaireAttachments});

            mockOfRestService.Setup(x =>
                x.DownloadFileAsync(Moq.It.IsAny<string>(), null, Moq.It.IsAny<RestCredentials>(), null)).Returns(Task.FromResult(new RestFile(new byte[] { 1 }, "image/png", "content id", 0, "file.png")));
            mockOfRestService.Setup(x => x.PostAsync<QuestionnaireCommunicationPackage>(Moq.It.IsAny<string>(), Moq.It.IsAny<Action<DownloadProgressChangedEventArgs>>(), Moq.It.IsAny<object>(), Moq.It.IsAny<RestCredentials>(), Moq.It.IsAny<CancellationToken?>()))
                .Returns(Task.FromResult(new QuestionnaireCommunicationPackage()));

            mockOfAttachmentContentService.Setup(x => x.HasAttachmentContent(questionnaireAttachments[0].ContentId)).Returns(true);
            mockOfAttachmentContentService.Setup(x => x.HasAttachmentContent(questionnaireAttachments[1].ContentId)).Returns(false);
            mockOfAttachmentContentService.Setup(x => x.HasAttachmentContent(questionnaireAttachments[2].ContentId)).Returns(false);

            

            controller = CreateDesignerQuestionnairesApiController(attachmentContentService: mockOfAttachmentContentService.Object,
                supportedVersionProvider: versionProvider.Object, zipUtils: zipUtilsMock.Object, restService: mockOfRestService.Object);
        };

        Because of = () => controller.GetQuestionnaire(importRequest).GetAwaiter().GetResult();

        It should_rethrow_command_service_exception = () =>
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
                new Attachment {ContentId = "Content 1"},
                new Attachment {ContentId = "Content 2"},
                new Attachment {ContentId = "Content 3"}
            });
    }
}