using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using MvvmCross.Tests;
using Ncqrs.Eventing.Storage;
using NSubstitute;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewLoading;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Infrastructure.Native.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.LoadingViewModelTests
{
    [TestFixture]
    internal class LoadingViewModelNUnitTests : MvxTestFixture
    {
        [Test]
        public async Task LoadingViewModel_when_interview_is_created_on_client_should_open_interview_on_prefilled_questions_section()
        {
            var interview = Substitute.For<IStatefulInterview>();
            interview.HasEditableIdentifyingQuestions.Returns(true);

            var statefulInterviewRepository = Substitute.For<IStatefulInterviewRepository>();
            statefulInterviewRepository.GetAsync(null, It.IsAny<IProgress<EventReadingProgress>>(),It.IsAny<CancellationToken>()).ReturnsForAnyArgs(Task.FromResult(interview));

            var navigationServiceMock = Substitute.For<IViewModelNavigationService>();

            var loadingViewModel = CreateLoadingViewModel(viewModelNavigationService: navigationServiceMock,
                interviewRepository: statefulInterviewRepository);

            await loadingViewModel.LoadAndNavigateToInterviewAsync(Guid.NewGuid());

            await navigationServiceMock.ReceivedWithAnyArgs().NavigateToPrefilledQuestionsAsync(interview.Id.FormatGuid());
        }

        [Test]
        public async Task LoadingViewModel_when_interview_is_not_created_on_client_should_open_interview_section()
        {
            var interview = Substitute.For<IStatefulInterview>();

            var statefulInterviewRepository = Substitute.For<IStatefulInterviewRepository>();
            statefulInterviewRepository.GetAsync(null, It.IsAny<IProgress<EventReadingProgress>>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(Task.FromResult(interview));

            var navigationServiceMock = Substitute.For<IViewModelNavigationService>();

            var loadingViewModel = CreateLoadingViewModel(viewModelNavigationService: navigationServiceMock,
                interviewRepository: statefulInterviewRepository);

            await loadingViewModel.LoadAndNavigateToInterviewAsync(Guid.NewGuid());

            await navigationServiceMock.ReceivedWithAnyArgs().NavigateToInterviewAsync(null, null);
        }

        [Test]
        public async Task LoadingViewModel_when_interview_is_completed_should_restart_interview_and_open_interview_section()
        {
            var interview = Substitute.For<IStatefulInterview>();
            interview.Status.Returns(InterviewStatus.Completed);
            interview.GetInterviewKey().Returns(new InterviewKey(111111));

            var statefulInterviewRepository = Substitute.For<IStatefulInterviewRepository>();
            statefulInterviewRepository.GetAsync(null, It.IsAny<IProgress<EventReadingProgress>>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(Task.FromResult(interview));

            var navigationServiceMock = Substitute.For<IViewModelNavigationService>();
            var commandService = Substitute.For<ICommandService>();
            var loadingViewModel = CreateLoadingViewModel(viewModelNavigationService: navigationServiceMock,
                interviewRepository: statefulInterviewRepository, commandService: commandService);
            loadingViewModel.Prepare(new LoadingViewModelArg
            {
                ShouldReopen = true
            });

            await loadingViewModel.LoadAndNavigateToInterviewAsync(Guid.NewGuid());

            await navigationServiceMock.ReceivedWithAnyArgs().NavigateToInterviewAsync(null, null);
            await commandService.ReceivedWithAnyArgs().ExecuteAsync(It.IsAny<RestartInterviewCommand>());
        }

        [Test]
        public async Task LoadingViewModel_when_stateful_repository_has_no_interview_should_remove_interview()
        {
            var interviewId = Guid.NewGuid();
            var interview = Substitute.For<IStatefulInterview>();
            interview.HasEditableIdentifyingQuestions.Returns(true);

            var statefulInterviewRepository = Substitute.For<IStatefulInterviewRepository>();
            statefulInterviewRepository.GetAsync(null, It.IsAny<IProgress<EventReadingProgress>>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(Task.FromResult(
                (IStatefulInterview)null));

            var navigationServiceMock = Substitute.For<IViewModelNavigationService>();

            var interviewsRepository = Substitute.For<IPlainStorage<InterviewView>>();

            var loadingViewModel = CreateLoadingViewModel(viewModelNavigationService: navigationServiceMock,
                interviewRepository: statefulInterviewRepository, interviewsRepository: interviewsRepository);

            await loadingViewModel.LoadAndNavigateToInterviewAsync(interviewId);

            interviewsRepository.Received().Remove(interviewId.FormatGuid());

            await navigationServiceMock.ReceivedWithAnyArgs().NavigateToDashboardAsync();
        }

        protected static LoadingInterviewViewModel CreateLoadingViewModel(
          IViewModelNavigationService viewModelNavigationService = null,
          IStatefulInterviewRepository interviewRepository = null,
          ICommandService commandService = null,
          IPrincipal principal = null,
          IPlainStorage<InterviewView> interviewsRepository = null,
          IAuditLogService auditLogService = null)
        {
            var loadingViewModel = new LoadingInterviewViewModel(
                principal ?? Substitute.For<IPrincipal>(),
                viewModelNavigationService ?? Substitute.For<IViewModelNavigationService>(), 
                interviewRepository ?? Substitute.For<IStatefulInterviewRepository>(),
                commandService ?? Substitute.For<ICommandService>(),
                logger: Mock.Of<ILogger>(),
                interactionService: Mock.Of<IUserInteractionService>(),
                interviewsRepository: interviewsRepository ?? Mock.Of<IPlainStorage<InterviewView>>(),
                new JsonAllTypesSerializer(),
                auditLogService ?? Substitute.For<IAuditLogService>(),
                Mock.Of<IViewModelEventRegistry>());

            return loadingViewModel;
        }
    }
}
