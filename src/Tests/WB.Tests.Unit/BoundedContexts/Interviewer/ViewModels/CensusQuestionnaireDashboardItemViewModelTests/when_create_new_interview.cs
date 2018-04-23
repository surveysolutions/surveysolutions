using System;
using System.Threading;
using Moq;
using MvvmCross.Plugin.Messenger;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.CensusQuestionnaireDashboardItemViewModelTests
{
    internal class when_create_new_interview : CensusQuestionnaireDashboardItemViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var principal = Mock.Of<IInterviewerPrincipal>(x => x.CurrentUserIdentity == Mock.Of<IInterviewerUserIdentity>());
            viewModel = CreateCensusQuestionnaireDashboardItemViewModel(
                messenger: mockOfMvxMessenger.Object,
                commandService: mockOfCommandService.Object,
                viewModelNavigationService: mockOfViewModelNavigationService.Object,
                principal: principal);

            viewModel.Init(Create.Entity.QuestionnaireView(Create.Entity.QuestionnaireIdentity(Guid.NewGuid(), 1)));
            BecauseOf();
        }

        public void BecauseOf() => viewModel.CreateNewInterviewCommand.Execute();

        [NUnit.Framework.Test] public void should_view_model_notify_about_starting_long_operation () =>
            mockOfMvxMessenger.Verify(x=>x.Publish(Moq.It.IsAny<StartingLongOperationMessage>()), Times.Once);

        [NUnit.Framework.Test] public void should_execute_create_intervew_on_client_command () =>
            mockOfCommandService.Verify(x =>
                x.ExecuteAsync(Moq.It.IsAny<CreateInterview>(), Moq.It.IsAny<string>(),
                    Moq.It.IsAny<CancellationToken>()), Times.Once);

        [NUnit.Framework.Test] public void should_navigate_to_prefilled_questions_view_model () =>
            mockOfViewModelNavigationService.Verify(x=>x.NavigateToPrefilledQuestionsAsync(Moq.It.IsAny<string>()), Times.Once);

        static CensusQuestionnaireDashboardItemViewModel viewModel;
        static readonly Mock<IMvxMessenger> mockOfMvxMessenger = new Mock<IMvxMessenger>();
        static readonly Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
        static readonly Mock<IViewModelNavigationService> mockOfViewModelNavigationService = new Mock<IViewModelNavigationService>();
    }
}
