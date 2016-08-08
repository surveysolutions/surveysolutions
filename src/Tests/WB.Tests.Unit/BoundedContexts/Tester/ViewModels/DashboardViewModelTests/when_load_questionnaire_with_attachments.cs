using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Machine.Specifications;

using Main.Core.Documents;

using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using It = Machine.Specifications.It;
using QuestionnaireListItem = WB.Core.BoundedContexts.Tester.Views.QuestionnaireListItem;
using AttachmentContentEnumerable = WB.Core.SharedKernels.Enumerator.Views.AttachmentContent;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_load_questionnaire_with_attachments : DashboardViewModelTestContext
    {
        Establish context = () =>
        {
            downloadedQuestionnaire = new Questionnaire
            {
                Document = Create.Entity.QuestionnaireDocumentWithAttachments(attachments: new[]
                {
                    Create.Entity.Attachment("1"),
                    Create.Entity.Attachment("5"),
                    Create.Entity.Attachment("2"),
                }),
                Assembly = "assembly"
            };

            mockOfAttachmentContentStorage.Setup(_ => _.Exists("1")).Returns(false);
            mockOfAttachmentContentStorage.Setup(_ => _.Exists("2")).Returns(false);
            mockOfAttachmentContentStorage.Setup(_ => _.Exists("5")).Returns(true);

            mockOfDesignerApiService
                .Setup(_ => _.GetQuestionnaireAsync(selectedQuestionnaire, Moq.It.IsAny<Action<DownloadProgressChangedEventArgs>>(), Moq.It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(downloadedQuestionnaire));
            mockOfDesignerApiService
                .Setup(_ => _.GetAttachmentContentAsync("1", Moq.It.IsAny<Action<DownloadProgressChangedEventArgs>>(), Moq.It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Create.Entity.AttachmentContent_Enumerator("1")));
            mockOfDesignerApiService
                .Setup(_ => _.GetAttachmentContentAsync("2", Moq.It.IsAny<Action<DownloadProgressChangedEventArgs>>(), Moq.It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Create.Entity.AttachmentContent_Enumerator("2")));


            viewModel = CreateDashboardViewModel(designerApiService: mockOfDesignerApiService.Object,
                commandService: mockOfCommandService.Object,
                questionnaireImportService: mockOfQuestionnaireImportService.Object,
                viewModelNavigationService: mockOfViewModelNavigationService.Object,
                attachmentContentStorage: mockOfAttachmentContentStorage.Object
                );
        };

        Because of = () => viewModel.LoadQuestionnaireCommand.Execute(selectedQuestionnaire);

        It should_be_downloaded_questionnaire = () => 
            mockOfDesignerApiService.Verify(_ => _.GetQuestionnaireAsync(selectedQuestionnaire, Moq.It.IsAny<Action<DownloadProgressChangedEventArgs>>(), Moq.It.IsAny<CancellationToken>()), Times.Once);

        It should_store_attachment_1_to_local_storage = () =>
            mockOfAttachmentContentStorage.Verify(_ => _.StoreAsync(Moq.It.Is<AttachmentContentEnumerable>(ac => ac.Id == "1")), Times.Once);

        It should_store_attachment_2_to_local_storage = () =>
            mockOfAttachmentContentStorage.Verify(_ => _.StoreAsync(Moq.It.Is<AttachmentContentEnumerable>(ac => ac.Id == "2")), Times.Once);

        It should_not_store_attachment_5_to_local_storage = () =>
            mockOfAttachmentContentStorage.Verify(_ => _.StoreAsync(Moq.It.Is<AttachmentContentEnumerable>(ac => ac.Id == "5")), Times.Never);

        It should_be_questionnaire_stored_to_local_storage = () => 
            mockOfQuestionnaireImportService.Verify(
                _ => _.ImportQuestionnaireAsync(Moq.It.IsAny<QuestionnaireIdentity>(), downloadedQuestionnaire.Document, downloadedQuestionnaire.Assembly, new TranslationDto[0]), 
                Times.Once);

        It should_be_executed_CreateInterviewOnClientCommand = () => 
            mockOfCommandService.Verify(_ => _.ExecuteAsync(Moq.It.IsAny<ICommand>(), null, Moq.It.IsAny<CancellationToken>()), Times.Once);

        It should_be_navigated_to_prefilled_questions_view_model = () => 
            mockOfViewModelNavigationService.Verify(_ => _.NavigateToPrefilledQuestions(Moq.It.IsAny<string>()), Times.Once);
        
        private static DashboardViewModel viewModel;
        private static Questionnaire downloadedQuestionnaire;

        private static readonly QuestionnaireListItem selectedQuestionnaire = new QuestionnaireListItem() { Id = "11111111111111111111111111111111"};
        private static readonly Mock<IDesignerApiService> mockOfDesignerApiService = new Mock<IDesignerApiService>();
        private static readonly Mock<IQuestionnaireImportService> mockOfQuestionnaireImportService = new Mock<IQuestionnaireImportService>();
        private static readonly Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
        private static readonly Mock<IViewModelNavigationService> mockOfViewModelNavigationService = new Mock<IViewModelNavigationService>();
        private static readonly Mock<IAttachmentContentStorage> mockOfAttachmentContentStorage = new Mock<IAttachmentContentStorage>();
    }
}