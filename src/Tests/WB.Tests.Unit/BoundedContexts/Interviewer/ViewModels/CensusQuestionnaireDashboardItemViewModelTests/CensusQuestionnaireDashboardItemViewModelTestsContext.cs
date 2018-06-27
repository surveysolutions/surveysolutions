using Moq;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.CensusQuestionnaireDashboardItemViewModelTests
{
    internal class CensusQuestionnaireDashboardItemViewModelTestsContext : MvxIoCSupportingTest
    {
        protected CensusQuestionnaireDashboardItemViewModelTestsContext()
        {
            base.Setup();
        }

        public static CensusQuestionnaireDashboardItemViewModel CreateCensusQuestionnaireDashboardItemViewModel(
            ICommandService commandService = null,
            IInterviewerPrincipal principal = null,
            IViewModelNavigationService viewModelNavigationService = null,
            IMvxMessenger messenger = null,
            IPlainStorage<InterviewView> interviewViewRepository = null)
        {
            return new CensusQuestionnaireDashboardItemViewModel(
                commandService: commandService ?? Mock.Of<ICommandService>(),
                principal: principal ?? Mock.Of<IInterviewerPrincipal>(),
                viewModelNavigationService: viewModelNavigationService ?? Mock.Of<IViewModelNavigationService>(),
                messenger: messenger ?? Mock.Of<IMvxMessenger>(),
                interviewViewRepository: interviewViewRepository ?? Mock.Of<IPlainStorage<InterviewView>>(),
                keyGenerator: Mock.Of<IInterviewUniqueKeyGenerator>());
        }
    }
}
