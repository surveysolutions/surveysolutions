using MvvmCross.Plugin.Messenger;
using Moq;
using MvvmCross.Logging;
using MvvmCross.Tests;
using NSubstitute;
using NUnit.Framework;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.InterviewDashboardItemViewModelTests
{
    [TestOf(typeof(InterviewDashboardItemViewModel))]
    public class InterviewDashboardItemViewModelTestsContext : MvxIoCSupportingTest
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
            var serviceLocator = Create.Fake.ServiceLocator()
                    .With(viewModelNavigationService ?? Substitute.For<IViewModelNavigationService>())
                    .With(userInteractionService ?? Substitute.For<IUserInteractionService>())
                    .With(Substitute.For<IExternalAppLauncher>())
                    .With(questionnaireViewRepository ?? Substitute.For<IPlainStorage<QuestionnaireView>>())
                    .With(prefilledQuestions ?? Substitute.For<IPlainStorage<PrefilledQuestionView>>())
                    .With(Substitute.For<IInterviewerInterviewAccessor>())
                    .With(Substitute.For<IMvxLog>())
                .Object;


            return new InterviewDashboardItemViewModel(serviceLocator, Mock.Of<IAuditLogService>());
        }
    }
}
