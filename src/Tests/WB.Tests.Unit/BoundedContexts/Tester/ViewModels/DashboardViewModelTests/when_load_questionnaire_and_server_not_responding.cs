using System;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_load_questionnaire_and_server_not_responding : DashboardViewModelTestContext
    {
        [OneTimeSetUp]
        public void Establish()
        {
            mockOfDesignerApiService.Setup(_ => _.GetQuestionnaireAsync(selectedQuestionnaire.Id, Moq.It.IsAny<IProgress<TransferProgress>>(),
                    Moq.It.IsAny<CancellationToken>())).Returns(Task.FromResult(downloadedQuestionnaire));

            viewModel = CreateDashboardViewModel(designerApiService: mockOfDesignerApiService.Object,
                commandService: mockOfCommandService.Object,
                questionnaireImportService: mockOfQuestionnaireImportService.Object,
                viewModelNavigationService: mockOfViewModelNavigationService.Object
                );
            Because();
        }

        public void Because() => viewModel.LoadQuestionnaireCommand.Execute(selectedQuestionnaire);

        [Test]
        public void should_be_downloaded_questionnaire() => mockOfDesignerApiService.Verify(
            _ => _.GetQuestionnaireAsync(selectedQuestionnaire.Id, Moq.It.IsAny<IProgress<TransferProgress>>(), Moq.It.IsAny<CancellationToken>()),
            Times.Once);

        [Test]
        public void should_not_be_questionnaire_stored_to_local_storage() => mockOfQuestionnaireImportService.Verify(
            _ => _.ImportQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<string>(), new TranslationDto[0]),
            Times.Never);

        [Test]
        public void should_be_executed_CreateInterviewCommand() => mockOfCommandService.Verify(
            _ => _.ExecuteAsync(Moq.It.IsAny<CreateInterview>(), null, Moq.It.IsAny<CancellationToken>()),
            Times.Never);

        [Test]
        public void should_be_navigated_to_prefilled_questions_view_model() => mockOfViewModelNavigationService.Verify(
            _ => _.NavigateToAsync<PrefilledQuestionsViewModel, InterviewViewModelArgs>(Moq.It.IsAny<InterviewViewModelArgs>()),
            Times.Never);

        private static DashboardViewModel viewModel;
        private static readonly Questionnaire downloadedQuestionnaire = null;
        private static readonly QuestionnaireListItem selectedQuestionnaire = new QuestionnaireListItem() { Id = "11111111111111111111111111111111" };
        private static readonly Mock<IDesignerApiService> mockOfDesignerApiService = new Mock<IDesignerApiService>();
        private static readonly Mock<IQuestionnaireImportService> mockOfQuestionnaireImportService = new Mock<IQuestionnaireImportService>();
        private static readonly Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
        private static readonly Mock<IViewModelNavigationService> mockOfViewModelNavigationService = new Mock<IViewModelNavigationService>();
    }
}
