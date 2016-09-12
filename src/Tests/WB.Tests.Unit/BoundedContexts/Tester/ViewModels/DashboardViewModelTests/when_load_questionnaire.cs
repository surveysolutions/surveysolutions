using System;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using It = Machine.Specifications.It;
using QuestionnaireListItem = WB.Core.BoundedContexts.Tester.Views.QuestionnaireListItem;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_load_questionnaire : DashboardViewModelTestContext
    {
        Establish context = () =>
        {
            mockOfDesignerApiService.Setup(_ => _.GetQuestionnaireAsync(selectedQuestionnaire, Moq.It.IsAny<Action<DownloadProgressChangedEventArgs>>(),
                    Moq.It.IsAny<CancellationToken>())).Returns(Task.FromResult(downloadedQuestionnaire));

            viewModel = CreateDashboardViewModel(designerApiService: mockOfDesignerApiService.Object,
                commandService: mockOfCommandService.Object,
                questionnaireImportService: mockOfQuestionnaireImportService.Object,
                viewModelNavigationService: mockOfViewModelNavigationService.Object
                );
        };

        Because of = () => viewModel.LoadQuestionnaireCommand.Execute(selectedQuestionnaire);

        It should_be_downloaded_questionnaire = () => 
            mockOfDesignerApiService.Verify(
                _ => _.GetQuestionnaireAsync(selectedQuestionnaire, Moq.It.IsAny<Action<DownloadProgressChangedEventArgs>>(), Moq.It.IsAny<CancellationToken>()), 
                Times.Once);

        It should_be_questionnaire_stored_to_local_storage = () => 
            mockOfQuestionnaireImportService.Verify(
                _ => _.ImportQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), downloadedQuestionnaire.Document, downloadedQuestionnaire.Assembly, new TranslationDto[0]), 
                Times.Once);

        It should_be_executed_CreateInterviewOnClientCommand = () => 
            mockOfCommandService.Verify(_ => _.ExecuteAsync(Moq.It.IsAny<ICommand>(), null, Moq.It.IsAny<CancellationToken>()), Times.Once);

        It should_be_navigated_to_prefilled_questions_view_model = () => 
            mockOfViewModelNavigationService.Verify(_ => _.NavigateToPrefilledQuestions(Moq.It.IsAny<string>()), Times.Once);
        
        private static DashboardViewModel viewModel;
        private static readonly Questionnaire downloadedQuestionnaire = new Questionnaire
        {
            Document = new QuestionnaireDocument(),
            Assembly =  "assembly"
        };
        private static readonly QuestionnaireListItem selectedQuestionnaire = new QuestionnaireListItem() { Id = "11111111111111111111111111111111"};
        private static readonly Mock<IDesignerApiService> mockOfDesignerApiService = new Mock<IDesignerApiService>();
        private static readonly Mock<IQuestionnaireImportService> mockOfQuestionnaireImportService = new Mock<IQuestionnaireImportService>();
        private static readonly Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
        private static readonly Mock<IViewModelNavigationService> mockOfViewModelNavigationService = new Mock<IViewModelNavigationService>();
    }
}