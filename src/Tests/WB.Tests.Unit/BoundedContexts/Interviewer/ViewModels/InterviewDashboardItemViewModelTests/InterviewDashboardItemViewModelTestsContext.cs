using Machine.Specifications;
using Moq;
using MvvmCross.Plugins.Messenger;
using NSubstitute;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.InterviewDashboardItemViewModelTests
{
    [Subject(typeof(InterviewDashboardItemViewModel))]    
    [TestOf(typeof(InterviewDashboardItemViewModel))]
    public class InterviewDashboardItemViewModelTestsContext
    {
        protected static InterviewDashboardItemViewModel GetViewModel(
            IViewModelNavigationService viewModelNavigationService = null,
            IUserInteractionService userInteractionService = null,
            IStatefulInterviewRepository interviewRepository = null,
            ICommandService commandService = null,
            IPrincipal principal = null,
            IMvxMessenger messenger = null,
            IPlainStorage<PrefilledQuestionView> prefilledQuestions = null,
            IPlainStorage<QuestionnaireView> questionnaireViewRepository = null)
        {
            return new InterviewDashboardItemViewModel(viewModelNavigationService ?? Substitute.For<IViewModelNavigationService>(),
                userInteractionService ?? Substitute.For<IUserInteractionService>(),
                messenger ?? Substitute.For<IMvxMessenger>(),
                Substitute.For<IExternalAppLauncher>(),
                questionnaireViewRepository ?? Substitute.For<IPlainStorage<QuestionnaireView>>(),
                prefilledQuestions ?? Mock.Of<IPlainStorage<PrefilledQuestionView>>(),
                Substitute.For<IInterviewerInterviewAccessor>());
        }
    }
}