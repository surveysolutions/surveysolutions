using System.Reactive.Subjects;
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
using WB.Core.SharedKernels.Enumerator.ViewModels;
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
        }

        private static DashboardViewModel CreateDashboardViewModel(
            IViewModelNavigationService viewModelNavigationService = null,
            IInterviewerPrincipal principal = null,
            LocalSynchronizationViewModel synchronization = null,
            IMvxMessenger messenger = null,
            IPlainStorage<InterviewView> interviewsRepository = null,
            ISynchronizationCompleteSource synchronizationCompleteSource = null,
            DashboardNotificationsViewModel dashboardNotifications = null)
        {
            return new DashboardViewModel(
                    viewModelNavigationService: viewModelNavigationService ?? Mock.Of<IViewModelNavigationService>(),
                    principal: principal ?? Mock.Of<IInterviewerPrincipal>(),
                    synchronization: synchronization ?? Create.ViewModel.LocalSynchronizationViewModel(),
                    messenger: messenger ?? Mock.Of<IMvxMessenger>(),
                    interviewerSettings: Mock.Of<IInterviewerSettings>(),
                    createNewViewModel: DashboardQuestionnairesViewModel(),
                    startedInterviewsViewModel: DashboardStartedInterviewsViewModel(),
                    completedInterviewsViewModel: DashboardCompletedInterviewsViewModel(),
                    rejectedInterviewsViewModel: DashboardRejectedInterviewsViewModel(), 
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
                    webInterviews: Mock.Of<WebInterviewsViewModel>());
        }

        private static ISynchronizationCompleteSource SyncCompleteSource = new SynchronizationCompleteSource();

        private static CreateNewViewModel DashboardQuestionnairesViewModel()
            => new CreateNewViewModel(
                Substitute.For<IPlainStorage<QuestionnaireView>>(),
                Substitute.For<IInterviewViewModelFactory>(),
                Substitute.For<IAssignmentDocumentsStorage>(),
                Mock.Of<IViewModelNavigationService>(),
                Mock.Of<IInterviewerSettings>(x => x.AllowSyncWithHq == true)    
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

        private static WebInterviewsViewModel DashboardWebInterviewsViewModel()
            => new WebInterviewsViewModel(
                Substitute.For<IPlainStorage<InterviewView>>(),
                Substitute.For<IInterviewViewModelFactory>(),
                Substitute.For<IPlainStorage<PrefilledQuestionView>>(),
                Substitute.For<IPrincipal>());
    }
}
