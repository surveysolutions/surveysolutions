﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Ncqrs.Eventing.Storage;
using NSubstitute;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.LoadingViewModelTests
{
    [TestFixture]
    internal class LoadingViewModelNUnitTests
    {
        [Test]
        public async Task LoadingViewModel_when_interview_is_created_on_client_should_open_prefilled_questions_section()
        {
            var interview = Substitute.For<IStatefulInterview>();
            interview.CreatedOnClient.Returns(true);

            var statefulInterviewRepository = Substitute.For<IStatefulInterviewRepository>();
            statefulInterviewRepository.GetAsync(null, Moq.It.IsAny<IProgress<EventReadingProgress>>(),Moq.It.IsAny<CancellationToken>()).ReturnsForAnyArgs(Task.FromResult(interview));

            var navigationServiceMock = Substitute.For<IViewModelNavigationService>();

            var loadingViewModel = CreateLoadingViewModel(viewModelNavigationService: navigationServiceMock,
                interviewRepository: statefulInterviewRepository);

            await loadingViewModel.RestoreInterviewAndNavigateThereAsync();

            navigationServiceMock.ReceivedWithAnyArgs().NavigateToPrefilledQuestions(null);
        }

        [Test]
        public async Task LoadingViewModel_when_interview_is_not_created_on_client_should_open_interview_section()
        {
            var interview = Substitute.For<IStatefulInterview>();
            interview.CreatedOnClient.Returns(false);

            var statefulInterviewRepository = Substitute.For<IStatefulInterviewRepository>();
            statefulInterviewRepository.GetAsync(null, Moq.It.IsAny<IProgress<EventReadingProgress>>(), Moq.It.IsAny<CancellationToken>()).ReturnsForAnyArgs(Task.FromResult(interview));

            var navigationServiceMock = Substitute.For<IViewModelNavigationService>();

            var loadingViewModel = CreateLoadingViewModel(viewModelNavigationService: navigationServiceMock,
                interviewRepository: statefulInterviewRepository);

            await loadingViewModel.RestoreInterviewAndNavigateThereAsync();

            navigationServiceMock.ReceivedWithAnyArgs().NavigateToInterview(null, null);
        }

        [Test]
        public async Task LoadingViewModel_when_interview_is_completed_should_restart_interview_and_open_interview_section()
        {
            var interview = Substitute.For<IStatefulInterview>();
            interview.CreatedOnClient.Returns(false);
            interview.Status.Returns(InterviewStatus.Completed);

            var statefulInterviewRepository = Substitute.For<IStatefulInterviewRepository>();
            statefulInterviewRepository.GetAsync(null, Moq.It.IsAny<IProgress<EventReadingProgress>>(), Moq.It.IsAny<CancellationToken>()).ReturnsForAnyArgs(Task.FromResult(interview));

            var navigationServiceMock = Substitute.For<IViewModelNavigationService>();
            var commandService = Substitute.For<ICommandService>();
            var loadingViewModel = CreateLoadingViewModel(viewModelNavigationService: navigationServiceMock,
                interviewRepository: statefulInterviewRepository, commandService: commandService);

            await loadingViewModel.RestoreInterviewAndNavigateThereAsync();

            navigationServiceMock.ReceivedWithAnyArgs().NavigateToInterview(null, null);
            await commandService.ReceivedWithAnyArgs().ExecuteAsync(Moq.It.IsAny<RestartInterviewCommand>());
        }
        protected static LoadingViewModel CreateLoadingViewModel(
          IViewModelNavigationService viewModelNavigationService = null,
          IStatefulInterviewRepository interviewRepository = null,
          ICommandService commandService = null,
          IPrincipal principal = null)
        {
            return new LoadingViewModel(
                principal ?? Substitute.For<IPrincipal>(),
                viewModelNavigationService ?? Substitute.For<IViewModelNavigationService>(), 
                interviewRepository ?? Substitute.For<IStatefulInterviewRepository>(),
                commandService ?? Substitute.For<ICommandService>(),
                questionnaireRepository: Substitute.For<IPlainQuestionnaireRepository>(),
                logger: Mock.Of<ILogger>(),
                interactionService: Mock.Of<IUserInteractionService>());
        }
    }
}