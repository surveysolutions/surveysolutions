using System.Threading;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_refresh_questionnaire_list_and_previous_request_is_in_progress : DashboardViewModelTestContext
    {
        [OneTimeSetUp]
        public void Establish()
        {
            viewModel = CreateDashboardViewModel(
                designerApiService: mockOfDesignerApiService.Object);
            viewModel.IsInProgress = true;

            Because();
        }

        public void Because() => viewModel.RefreshQuestionnairesCommand.Execute();

        [Test]
        public void should_not_questionnaire_list_be_updated() 
            => mockOfDesignerApiService.Verify(_ => _.GetQuestionnairesAsync(Moq.It.IsAny<CancellationToken>()),
                Times.Never);

        private static DashboardViewModel viewModel;
        private static readonly Mock<IDesignerApiService> mockOfDesignerApiService = new Mock<IDesignerApiService>();
    }
}
