using System.Threading;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.DashboardViewModelTests
{
    public class when_refresh_questionnaire_list_and_previous_request_is_in_progress : DashboardViewModelTestContext
    {
        Establish context = () =>
        {
            viewModel = CreateDashboardViewModel(
                designerApiService: mockOfDesignerApiService.Object);
            viewModel.IsInProgress = true;
        };

        Because of = () => viewModel.RefreshQuestionnairesCommand.Execute();

        It should_not_questionnaire_list_be_updated = () => mockOfDesignerApiService.Verify(_=>_.GetQuestionnairesAsync(Moq.It.IsAny<bool>(), Moq.It.IsAny<CancellationToken>()), Times.Never);

        private static DashboardViewModel viewModel;
        private static readonly Mock<IDesignerApiService> mockOfDesignerApiService = new Mock<IDesignerApiService>();
        
    }
}