using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_load_questionnaire_and_previous_request_is_in_progress : DashboardViewModelTestContext
    {
        [OneTimeSetUp]
        public async Task Establish()
        {
            viewModel = CreateDashboardViewModel(
                designerApiService: mockOfDesignerApiService.Object
            );
            viewModel.IsInProgress = true;
            await Because();
        }

        public Task Because() => viewModel.LoadQuestionnaireCommand.ExecuteAsync(new QuestionnaireListItemViewModel());

        [Test]
        public void should_not_be_loaded_new_questionnaire () => mockOfDesignerApiService.Verify(
                _ => _.GetQuestionnaireAsync(Moq.It.IsAny<string>(),
                    Moq.It.IsAny<IProgress<TransferProgress>>(), Moq.It.IsAny<CancellationToken>()),
                Times.Never);

        private static DashboardViewModel viewModel;
        private static readonly Mock<IDesignerApiService> mockOfDesignerApiService = new Mock<IDesignerApiService>();
    }
}
