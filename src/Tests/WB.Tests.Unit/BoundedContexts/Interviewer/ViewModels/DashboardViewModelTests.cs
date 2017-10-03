using Moq;
using MvvmCross.Plugins.Messenger;
using MvvmCross.Test.Core;
using NSubstitute;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels
{
    [TestOf(typeof(DashboardViewModel))]
    [TestFixture]
    internal class DashboardViewModelTests : MvxIoCSupportingTest
    {
        public DashboardViewModelTests()
        {
            base.Setup();
        }

        [Test]
        public void When_execute_SynchronizationCommand_and_census_interview_creating_Then_should_not_be_called_synchronization_and_waiting_message_showed()
        {
            // arrange
            var mockOfViewModelNavigationService = new Mock<IViewModelNavigationService>();
            mockOfViewModelNavigationService.SetupGet(x => x.HasPendingOperations).Returns(true);

            var mockOfSynchronizationViewModel = new Mock<SynchronizationViewModel>();
            var viewModel = CreateDashboardViewModel(
                viewModelNavigationService: mockOfViewModelNavigationService.Object,
                synchronization: mockOfSynchronizationViewModel.Object);

            //act
            viewModel.SynchronizationCommand.Execute();

            //assert
            mockOfViewModelNavigationService.Verify(m => m.ShowWaitMessage(), Times.Once);
            mockOfSynchronizationViewModel.Verify(m => m.Synchronize(), Times.Never);
        }

        private static DashboardViewModel CreateDashboardViewModel(
            IViewModelNavigationService viewModelNavigationService = null,
            IInterviewerPrincipal principal = null,
            SynchronizationViewModel synchronization = null,
            IMvxMessenger messenger = null,
            IPlainStorage<InterviewView> interviewsRepository = null)
        {
            return new DashboardViewModel(
                    viewModelNavigationService: viewModelNavigationService ?? Mock.Of<IViewModelNavigationService>(),
                    principal: principal ?? Mock.Of<IInterviewerPrincipal>(),
                    synchronization: synchronization ?? Substitute.For<SynchronizationViewModel>(),
                    messenger: messenger ?? Mock.Of<IMvxMessenger>(),
                    createNewViewModel: DashboardQuestionnairesViewModel(),
                    startedInterviewsViewModel: DashboardStartedInterviewsViewModel(),
                    completedInterviewsViewModel: DashboardCompletedInterviewsViewModel(),
                    rejectedInterviewsViewModel: DashboardRejectedInterviewsViewModel(), 
                    interviewsRepository: interviewsRepository ?? Substitute.For<IPlainStorage<InterviewView>>());
        }

        private static CreateNewViewModel DashboardQuestionnairesViewModel()
            => new CreateNewViewModel(
                Substitute.For<IPlainStorage<QuestionnaireView>>(),
                Substitute.For<IInterviewViewModelFactory>(),
                Substitute.For<IAssignmentDocumentsStorage>(),
                Substitute.For<IPlainStorage<InterviewView>>(),
                Substitute.For<IPlainStorage<AssignmentDocument, int>>(),
                Mock.Of<IViewModelNavigationService>()
            );

        private static StartedInterviewsViewModel DashboardStartedInterviewsViewModel()
            => new StartedInterviewsViewModel(
                Substitute.For<IPlainStorage<InterviewView>>(),
                Substitute.For<IInterviewViewModelFactory>(),
                Substitute.For<IPlainStorage<PrefilledQuestionView>>(),
                Substitute.For<IPrincipal>());

        private static CompletedInterviewsViewModel DashboardCompletedInterviewsViewModel()
            => new CompletedInterviewsViewModel(
                Substitute.For<IPlainStorage<InterviewView>>(),
                Substitute.For<IInterviewViewModelFactory>(),
                Substitute.For<IPlainStorage<PrefilledQuestionView>>(),
                Substitute.For<IPrincipal>());

        private static RejectedInterviewsViewModel DashboardRejectedInterviewsViewModel()
            => new RejectedInterviewsViewModel(
                Substitute.For<IPlainStorage<InterviewView>>(),
                Substitute.For<IInterviewViewModelFactory>(),
                Substitute.For<IPlainStorage<PrefilledQuestionView>>(),
                Substitute.For<IPrincipal>());
    }
}
