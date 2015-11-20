using Machine.Specifications;
using MvvmCross.Plugins.Messenger;
using NSubstitute;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.InterviewDashboardItemViewModelTests
{
    [Subject(typeof(InterviewDashboardItemViewModel))]    
    public class InterviewDashboardItemViewModelTestsContext
    {
        protected static InterviewDashboardItemViewModel GetViewModel(
            IViewModelNavigationService viewModelNavigationService = null,
            IUserInteractionService userInteractionService = null,
            IStatefulInterviewRepository interviewRepository = null,
            ICommandService commandService = null,
            IPrincipal principal = null,
            IChangeLogManipulator changeLogManipulator = null,
            ICapiCleanUpService capiCleanUpService = null,
            IMvxMessenger messenger = null,
            ISyncPackageRestoreService packageRestoreService = null)
        {
            return new InterviewDashboardItemViewModel(viewModelNavigationService ?? Substitute.For<IViewModelNavigationService>(),
                userInteractionService ?? Substitute.For<IUserInteractionService>(),
                interviewRepository ?? Substitute.For<IStatefulInterviewRepository>(),
                commandService ?? Substitute.For<ICommandService>(),
                principal ?? Substitute.For<IPrincipal>(),
                changeLogManipulator ?? Substitute.For<IChangeLogManipulator>(),
                capiCleanUpService ?? Substitute.For<ICapiCleanUpService>(),
                messenger ?? Substitute.For<IMvxMessenger>(),
                packageRestoreService ?? Substitute.For<ISyncPackageRestoreService>(),
                Substitute.For<IExternalAppLauncher>());
        }
    }
}