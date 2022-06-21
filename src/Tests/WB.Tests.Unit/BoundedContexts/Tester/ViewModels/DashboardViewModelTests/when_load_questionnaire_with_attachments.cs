using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_load_questionnaire_with_attachments : DashboardViewModelTestContext
    {
        [OneTimeSetUp]
        public async Task Establish()
        {
            downloadedQuestionnaire = new Questionnaire
            (
                document : Create.Entity.QuestionnaireDocumentWithAttachments(attachments: new[]
                {
                    Create.Entity.Attachment("1"),
                    Create.Entity.Attachment("5"),
                    Create.Entity.Attachment("2"),
                }),
                assembly : "assembly"
            );
            downloadedQuestionnaire.Document.PublicKey = Guid.Parse(selectedQuestionnaire.Id);

            mockOfAttachmentContentStorage.Setup(_ => _.Exists("1")).Returns(false);
            mockOfAttachmentContentStorage.Setup(_ => _.Exists("2")).Returns(false);
            mockOfAttachmentContentStorage.Setup(_ => _.Exists("5")).Returns(true);
            mockOfAttachmentContentStorage.Setup(x =>
                    x.StoreAsync(It.IsAny<WB.Core.SharedKernels.Questionnaire.Api.AttachmentContent>()))
                .Returns(Task.CompletedTask);

            mockOfDesignerApiService
                .Setup(_ => _.GetQuestionnaireAsync(selectedQuestionnaire.Id, Moq.It.IsAny<IProgress<TransferProgress>>(), Moq.It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(downloadedQuestionnaire));
            mockOfDesignerApiService
                .Setup(_ => _.GetAttachmentContentAsync(selectedQuestionnaire.Id, "1", Moq.It.IsAny<IProgress<TransferProgress>>(), Moq.It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Create.Entity.AttachmentContent_Enumerator("1")));
            mockOfDesignerApiService
                .Setup(_ => _.GetAttachmentContentAsync(selectedQuestionnaire.Id, "2", Moq.It.IsAny<IProgress<TransferProgress>>(), Moq.It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Create.Entity.AttachmentContent_Enumerator("2")));
            

            var questionnaireRepository = new Mock<IQuestionnaireStorage>();
            questionnaireRepository.Setup(x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), null))
                .Returns(Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter()));

            viewModel = CreateDashboardViewModel(designerApiService: mockOfDesignerApiService.Object,
                commandService: mockOfCommandService.Object,
                questionnaireImportService: mockOfQuestionnaireImportService.Object,
                viewModelNavigationService: mockOfViewModelNavigationService.Object,
                attachmentContentStorage: mockOfAttachmentContentStorage.Object,
                questionnaireRepository: questionnaireRepository.Object
                );
            await Because();
        }

        public Task Because() => viewModel.LoadQuestionnaireCommand.ExecuteAsync(selectedQuestionnaire);

        [Test] public void should_be_downloaded_questionnaire () => 
            mockOfDesignerApiService.Verify(_ => _.GetQuestionnaireAsync(selectedQuestionnaire.Id, Moq.It.IsAny<IProgress<TransferProgress>>(), Moq.It.IsAny<CancellationToken>()), Times.Once);

        [Test] public void should_store_attachment_1_to_local_storage () =>
            mockOfAttachmentContentStorage.Verify(_ => _.StoreAsync(Moq.It.Is<WB.Core.SharedKernels.Questionnaire.Api.AttachmentContent>(ac => ac.Id == "1")), Times.Once);

        [Test] public void should_store_attachment_2_to_local_storage () =>
            mockOfAttachmentContentStorage.Verify(_ => _.StoreAsync(Moq.It.Is<WB.Core.SharedKernels.Questionnaire.Api.AttachmentContent>(ac => ac.Id == "2")), Times.Once);

        [Test] public void should_not_store_attachment_5_to_local_storage () =>
            mockOfAttachmentContentStorage.Verify(_ => _.StoreAsync(Moq.It.Is<WB.Core.SharedKernels.Questionnaire.Api.AttachmentContent>(ac => ac.Id == "5")), Times.Never);

        [Test] public void should_be_questionnaire_stored_to_local_storage () => 
            mockOfQuestionnaireImportService.Verify(
                _ => _.ImportQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), downloadedQuestionnaire.Document, downloadedQuestionnaire.Assembly, new TranslationDto[0], new ReusableCategoriesDto[0]), 
                Times.Once);

        [Test] public void should_be_executed_CreateInterviewCommand () => 
            mockOfCommandService.Verify(_ => _.ExecuteAsync(Moq.It.IsAny<CreateInterview>(), null, Moq.It.IsAny<CancellationToken>()), Times.Once);

        private static DashboardViewModel viewModel;
        private static Questionnaire downloadedQuestionnaire;

        private static readonly QuestionnaireListItemViewModel selectedQuestionnaire = new QuestionnaireListItemViewModel() { Id = "11111111111111111111111111111111"};
        private static readonly Mock<IDesignerApiService> mockOfDesignerApiService = new Mock<IDesignerApiService>();
        private static readonly Mock<IQuestionnaireImportService> mockOfQuestionnaireImportService = new Mock<IQuestionnaireImportService>();
        private static readonly Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
        private static readonly Mock<IViewModelNavigationService> mockOfViewModelNavigationService = new Mock<IViewModelNavigationService>();
        private static readonly Mock<IAttachmentContentStorage> mockOfAttachmentContentStorage = new Mock<IAttachmentContentStorage>();
    }
}
