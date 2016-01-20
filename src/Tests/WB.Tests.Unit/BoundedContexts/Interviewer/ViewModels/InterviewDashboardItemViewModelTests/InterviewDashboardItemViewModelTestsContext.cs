using Machine.Specifications;
using MvvmCross.Plugins.Messenger;
using NSubstitute;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

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
            IMvxMessenger messenger = null)
        {
            return new InterviewDashboardItemViewModel(viewModelNavigationService ?? Substitute.For<IViewModelNavigationService>(),
                userInteractionService ?? Substitute.For<IUserInteractionService>(),
                interviewRepository ?? Substitute.For<IStatefulInterviewRepository>(),
                commandService ?? Substitute.For<ICommandService>(),
                principal ?? Substitute.For<IPrincipal>(),
                messenger ?? Substitute.For<IMvxMessenger>(),
                Substitute.For<IExternalAppLauncher>(),
                Substitute.For<IAsyncPlainStorage<QuestionnaireView>>(),
                Substitute.For<IAsyncPlainStorage<InterviewView>>(),
                Substitute.For<IInterviewerInterviewAccessor>());
        }
    }
}