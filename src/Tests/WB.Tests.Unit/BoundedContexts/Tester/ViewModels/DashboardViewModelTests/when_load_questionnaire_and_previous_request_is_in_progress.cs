using System;
using System.Threading;

using Machine.Specifications;

using Moq;

using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.GenericSubdomains.Portable.Implementation;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_load_questionnaire_and_previous_request_is_in_progress : DashboardViewModelTestContext
    {
        Establish context = () =>
        {
            viewModel = CreateDashboardViewModel(
                designerApiService: mockOfDesignerApiService.Object
                );
            viewModel.IsInProgress = true;
        };

        Because of = () => viewModel.LoadQuestionnaireCommand.Execute(new QuestionnaireListItem());

        It should_not_be_loaded_new_questionnaire = () => mockOfDesignerApiService.Verify(_ => _.GetQuestionnaireAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<Action<DownloadProgressChangedEventArgs>>(), Moq.It.IsAny<CancellationToken>()), Times.Never);
        
        private static DashboardViewModel viewModel;
        private static readonly Mock<IDesignerApiService> mockOfDesignerApiService = new Mock<IDesignerApiService>();
    }
}