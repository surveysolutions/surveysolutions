using Machine.Specifications;
using Moq;
using MvvmCross.Plugins.Messenger;
using NSubstitute;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Tests.Abc;

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
            var serviceLocator = Create.Fake.ServiceLocator()
                    .With(viewModelNavigationService ?? Substitute.For<IViewModelNavigationService>())
                    .With(userInteractionService ?? Substitute.For<IUserInteractionService>())
                    .With(Substitute.For<IExternalAppLauncher>())
                    .With(questionnaireViewRepository ?? Substitute.For<IPlainStorage<QuestionnaireView>>())
                    .With(prefilledQuestions ?? Substitute.For<IPlainStorage<PrefilledQuestionView>>())
                    .With(Substitute.For<IInterviewerInterviewAccessor>())
                .Object;

            return new InterviewDashboardItemViewModel(serviceLocator);
        }
    }
}