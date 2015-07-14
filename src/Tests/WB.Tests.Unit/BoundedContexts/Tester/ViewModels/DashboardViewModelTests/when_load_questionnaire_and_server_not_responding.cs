using System;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using It = Machine.Specifications.It;
using QuestionnaireListItem = WB.Core.BoundedContexts.QuestionnaireTester.Views.QuestionnaireListItem;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.DashboardViewModelTests
{
    public class when_load_questionnaire_and_server_not_responding : DashboardViewModelTestContext
    {
        Establish context = () =>
        {
            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == Guid.Parse("11111111111111111111111111111111"));
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            mockOfDesignerApiService.Setup(_ => _.GetQuestionnaireAsync(selectedQuestionnaire, Moq.It.IsAny<Action<decimal>>(),
                    Moq.It.IsAny<CancellationToken>())).Returns(Task.FromResult(downloadedQuestionnaire));

            viewModel = CreateDashboardViewModel(
                principal: principal,
                designerApiService: mockOfDesignerApiService.Object,
                commandService: mockOfCommandService.Object,
                questionnaireImportService: mockOfQuestionnaireImportService.Object,
                viewModelNavigationService: mockOfViewModelNavigationService.Object
                );
        };

        Because of = () => viewModel.LoadQuestionnaireCommand.Execute(selectedQuestionnaire);

        It should_be_downloaded_questionnaire = () => mockOfDesignerApiService.Verify(_ => _.GetQuestionnaireAsync(selectedQuestionnaire, Moq.It.IsAny<Action<decimal>>(), Moq.It.IsAny<CancellationToken>()), Times.Once);
        It should_not_be_questionnaire_stored_to_local_storage = () => mockOfQuestionnaireImportService.Verify(_ => _.ImportQuestionnaire(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<string>()), Times.Never);
        It should_be_executed_CreateInterviewOnClientCommand = () => mockOfCommandService.Verify(_ => _.ExecuteAsync(Moq.It.IsAny<ICommand>(), null, Moq.It.IsAny<CancellationToken>()), Times.Never);
        It should_be_navigated_to_prefilled_questions_view_model = () => mockOfViewModelNavigationService.Verify(_ => _.NavigateTo<PrefilledQuestionsViewModel>(Moq.It.IsAny<object>()), Times.Never);
        
        private static DashboardViewModel viewModel;
        private static readonly Questionnaire downloadedQuestionnaire = null;
        private static readonly QuestionnaireListItem selectedQuestionnaire = new QuestionnaireListItem() { Id = "11111111111111111111111111111111"};
        private static readonly Mock<IDesignerApiService> mockOfDesignerApiService = new Mock<IDesignerApiService>();
        private static readonly Mock<IQuestionnaireImportService> mockOfQuestionnaireImportService = new Mock<IQuestionnaireImportService>();
        private static readonly Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
        private static readonly Mock<IViewModelNavigationService> mockOfViewModelNavigationService = new Mock<IViewModelNavigationService>();
    }
}