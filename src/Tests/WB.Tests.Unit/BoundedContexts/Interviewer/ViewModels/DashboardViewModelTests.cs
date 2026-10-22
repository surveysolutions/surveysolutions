using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Moq;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using Ncqrs;
using NSubstitute;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels
{
    [TestOf(typeof(DashboardViewModel))]
    [TestFixture]
    internal class DashboardViewModelTests : MvxIoCSupportingTest
    {
        public DashboardViewModelTests()
        {
            base.Setup();
            Ioc.RegisterSingleton<IMvxMessenger>(Mock.Of<IMvxMessenger>());
        }

        [Test]
        public void When_execute_SynchronizationCommand_and_census_interview_creating_Then_should_not_be_called_synchronization_and_waiting_message_showed()
        {
            // arrange
            var mockOfViewModelNavigationService = new Mock<IViewModelNavigationService>();
            mockOfViewModelNavigationService.SetupGet(x => x.HasPendingOperations).Returns(true);

            var mockOfSynchronizationViewModel = new Mock<LocalSynchronizationViewModel>(
                new SynchronizationCompleteSource(),
                Mock.Of<ITabletDiagnosticService>(),
                Mock.Of<ILogger>());

            var mockOfdashboardNotifications = Create.ViewModel.DashboardNotificationsViewModel(
                mockOfViewModelNavigationService.Object
            );
            
            var viewModel = CreateDashboardViewModel(
                viewModelNavigationService: mockOfViewModelNavigationService.Object,
                synchronization: mockOfSynchronizationViewModel.Object,
                dashboardNotifications: mockOfdashboardNotifications);

            //act
            viewModel.SynchronizationCommand.Execute();

            //assert
            mockOfViewModelNavigationService.Verify(m => m.ShowWaitMessage(), Times.Once);
            mockOfSynchronizationViewModel.Verify(m => m.Synchronize(), Times.Never);
        }

        [Test]
        public async Task When_synchronization_completes_and_rejected_interviews_exist_then_select_rejected_tab()
        {
            var synchronizationCompleteSource = new SynchronizationCompleteSource();
            var rejectedInterviews = new TestRejectedInterviewsViewModel(1);
            var viewModel = CreateDashboardViewModel(
                synchronizationCompleteSource: synchronizationCompleteSource,
                rejectedInterviewsViewModel: rejectedInterviews);
            viewModel.TypeOfInterviews = DashboardGroupType.Assignments;

            var refresh = WaitForDashboardRefresh(viewModel);
            synchronizationCompleteSource.NotifyOnCompletedSynchronization(true);
            await refresh;

            Assert.That(viewModel.TypeOfInterviews, Is.EqualTo(DashboardGroupType.InvalidInterviews));
        }

        [Test]
        public async Task When_synchronization_completes_and_no_rejected_interviews_exist_then_keep_selected_tab()
        {
            var synchronizationCompleteSource = new SynchronizationCompleteSource();
            var rejectedInterviews = new TestRejectedInterviewsViewModel(0);
            var viewModel = CreateDashboardViewModel(
                synchronizationCompleteSource: synchronizationCompleteSource,
                rejectedInterviewsViewModel: rejectedInterviews);
            viewModel.TypeOfInterviews = DashboardGroupType.Assignments;

            var refresh = WaitForDashboardRefresh(viewModel);
            synchronizationCompleteSource.NotifyOnCompletedSynchronization(true);
            await refresh;

            Assert.That(viewModel.TypeOfInterviews, Is.EqualTo(DashboardGroupType.Assignments));
        }

        private static DashboardViewModel CreateDashboardViewModel(
            IViewModelNavigationService viewModelNavigationService = null,
            IInterviewerPrincipal principal = null,
            LocalSynchronizationViewModel synchronization = null,
            IMvxMessenger messenger = null,
            IPlainStorage<InterviewView> interviewsRepository = null,
            ISynchronizationCompleteSource synchronizationCompleteSource = null,
            DashboardNotificationsViewModel dashboardNotifications = null,
            RejectedInterviewsViewModel rejectedInterviewsViewModel = null)
        {
            return new DashboardViewModel(
                    viewModelNavigationService: viewModelNavigationService ?? Mock.Of<IViewModelNavigationService>(),
                    principal: principal ?? Mock.Of<IInterviewerPrincipal>(),
                    synchronization: synchronization ?? Create.ViewModel.LocalSynchronizationViewModel(),
                    interviewerSettings: Mock.Of<IInterviewerSettings>(),
                    createNewViewModel: DashboardQuestionnairesViewModel(),
                    startedInterviewsViewModel: DashboardStartedInterviewsViewModel(),
                    completedInterviewsViewModel: DashboardCompletedInterviewsViewModel(),
                    rejectedInterviewsViewModel: rejectedInterviewsViewModel ?? DashboardRejectedInterviewsViewModel(),
                    interviewsRepository: interviewsRepository ?? Substitute.For<IPlainStorage<InterviewView>>(),
                    auditLogService: Mock.Of<IAuditLogService>(),
                    synchronizationCompleteSource: synchronizationCompleteSource ?? SyncCompleteSource, 
                    permissionsService: Mock.Of<IPermissionsService>(), 
                    nearbyConnection: Mock.Of<INearbyConnection>(x => x.Events == new Subject<INearbyEvent>()), 
                    syncClient: Mock.Of<IOfflineSyncClient>(),
                    userInteractionService: Mock.Of<IUserInteractionService>(),
                    googleApiService: Mock.Of<IGoogleApiService>(),
                    mapInteractionService: Mock.Of<IMapInteractionService>(),
                    dashboardNotifications: dashboardNotifications ?? Create.ViewModel.DashboardNotificationsViewModel(),
                    workspaceService: Mock.Of<IWorkspaceService>(),
                    onlineSynchronizationService: Mock.Of<IOnlineSynchronizationService>(),
                    memoryCacheSource: Mock.Of<IWorkspaceMemoryCacheSource>(),
                    webInterviews: DashboardWebInterviewsViewModel(),
                    messenger: Mock.Of<IMvxMessenger>());
        }

        private static async Task WaitForDashboardRefresh(DashboardViewModel viewModel)
        {
            var refreshCompleted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            viewModel.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(DashboardViewModel.DashboardTitle))
                    refreshCompleted.TrySetResult(true);
            };

            await refreshCompleted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await Task.Delay(20);
        }

        private static ISynchronizationCompleteSource SyncCompleteSource = new SynchronizationCompleteSource();

        private static CreateNewViewModel DashboardQuestionnairesViewModel()
        {
            var questionnaires = Substitute.For<IPlainStorage<QuestionnaireView>>();
            questionnaires.Where(Arg.Any<System.Linq.Expressions.Expression<Func<QuestionnaireView, bool>>>())
                .Returns(Array.Empty<QuestionnaireView>());
            var assignments = Substitute.For<IAssignmentDocumentsStorage>();
            assignments.LoadAll().Returns(Array.Empty<AssignmentDocument>());

            return new CreateNewViewModel(
                questionnaires,
                DashboardInterviewViewModelFactory(),
                assignments,
                Mock.Of<IViewModelNavigationService>(),
                Mock.Of<IInterviewerSettings>(x => x.AllowSyncWithHq == true));
        }

        private static IInterviewViewModelFactory DashboardInterviewViewModelFactory()
        {
            var viewModelFactory = Substitute.For<IInterviewViewModelFactory>();
            viewModelFactory.GetNew<DashboardSubTitleViewModel>()
                .Returns(_ => new DashboardSubTitleViewModel());
            return viewModelFactory;
        }

        private static IPlainStorage<PrefilledQuestionView> DashboardPrefilledQuestionsRepository()
        {
            var repository = Substitute.For<IPlainStorage<PrefilledQuestionView>>();
            repository.LoadAll().Returns(Array.Empty<PrefilledQuestionView>());
            return repository;
        }

        private static StartedInterviewsViewModel DashboardStartedInterviewsViewModel()
            => new StartedInterviewsViewModel(
                Substitute.For<IPlainStorage<InterviewView>>(),
                DashboardInterviewViewModelFactory(),
                DashboardPrefilledQuestionsRepository(),
                Substitute.For<IPrincipal>());

        private static CompletedInterviewsViewModel DashboardCompletedInterviewsViewModel()
            => new CompletedInterviewsViewModel(
                Substitute.For<IPlainStorage<InterviewView>>(),
                DashboardInterviewViewModelFactory(),
                DashboardPrefilledQuestionsRepository(),
                Substitute.For<IPrincipal>(),
                Substitute.For<IAssignmentDocumentsStorage>());

        private static RejectedInterviewsViewModel DashboardRejectedInterviewsViewModel()
            => new RejectedInterviewsViewModel(
                Substitute.For<IPlainStorage<InterviewView>>(),
                DashboardInterviewViewModelFactory(),
                DashboardPrefilledQuestionsRepository(),
                Substitute.For<IPrincipal>());

        private static WebInterviewsViewModel DashboardWebInterviewsViewModel()
            => new WebInterviewsViewModel(
                Substitute.For<IPlainStorage<InterviewView>>(),
                DashboardInterviewViewModelFactory(),
                DashboardPrefilledQuestionsRepository(),
                Substitute.For<IPrincipal>());

        private sealed class TestRejectedInterviewsViewModel : RejectedInterviewsViewModel
        {
            private readonly int itemsCount;

            public TestRejectedInterviewsViewModel(int itemsCount)
                : base(
                    Substitute.For<IPlainStorage<InterviewView>>(),
                    Substitute.For<IInterviewViewModelFactory>(),
                    Substitute.For<IPlainStorage<PrefilledQuestionView>>(),
                    Substitute.For<IPrincipal>())
            {
                this.itemsCount = itemsCount;
            }

            public override Task LoadAsync(Guid? lastVisitedInterviewId)
            {
                ItemsCount = this.itemsCount;
                return Task.CompletedTask;
            }
        }
    }
}
