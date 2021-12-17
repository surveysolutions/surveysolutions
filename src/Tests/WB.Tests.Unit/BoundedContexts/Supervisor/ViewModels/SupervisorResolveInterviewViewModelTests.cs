using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.ViewModels
{
    [TestOf(typeof(SupervisorResolveInterviewViewModel))]
    public class SupervisorResolveInterviewViewModelTests : MvxIoCSupportingTest
    {
        public SupervisorResolveInterviewViewModelTests()
        {
            base.Setup();
            Ioc.RegisterSingleton(Stub.MvxMainThreadAsyncDispatcher());
            Ioc.RegisterSingleton<IMvxMessenger>(Mock.Of<IMvxMessenger>());
        }
        
        private readonly Guid InterviewId = Id.g1;

        [Test]
        public void should_be_named_resolve()
        {
            var viewModel = CreateViewModel();
            viewModel.Configure(InterviewId.FormatGuid(), Create.Other.NavigationState());
            Assert.That(viewModel.Name.PlainText, Is.EqualTo(InterviewDetails.Resolve));
        }

        [Test]
        public void should_allow_approve_and_reject_for_completed_interview()
        {
            var interview = Create.AggregateRoot.StatefulInterview(interviewId: InterviewId);
            interview.Complete(Id.gA, "", DateTime.UtcNow);

            var statefulInterviewRepository = Abc.SetUp.StatefulInterviewRepository(interview);
            var viewModel = CreateViewModel(interviewRepository: statefulInterviewRepository);

            // Act
            viewModel.Configure(InterviewId.FormatGuid(), Create.Other.NavigationState(statefulInterviewRepository));
            
            // Assert
            viewModel.Approve.CanExecute().Should().BeTrue();
            viewModel.Reject.CanExecute().Should().BeTrue();
        }

        [Test]
        public async Task when_approved_should_execute_approve_command()
        {
            var commandService = new Mock<ICommandService>();
            var navigationService = new Mock<IViewModelNavigationService>();
            var auditLogService = new Mock<IAuditLogService>();
            
            var interview = Create.AggregateRoot.StatefulInterview(interviewId: InterviewId);
            var interviewKey = Create.Entity.InterviewKey(5);
            interview.Apply(Create.Event.InterviewKeyAssigned(interviewKey));
            interview.Complete(Id.gA, "", DateTime.UtcNow);

            var statefulInterviewRepository = Abc.SetUp.StatefulInterviewRepository(interview);
            var viewModel = CreateViewModel(interviewRepository: statefulInterviewRepository,
                commandService: commandService.Object,
                navigationService: navigationService.Object,
                auditLogService: auditLogService.Object);
            viewModel.Configure(InterviewId.FormatGuid(), Create.Other.NavigationState(statefulInterviewRepository));

            // Act
            await viewModel.Approve.ExecuteAsync();
            
            // Assert
            commandService.Verify(x => x.ExecuteAsync(It.Is<ApproveInterviewCommand>(c => c.InterviewId == InterviewId), null, CancellationToken.None));
            navigationService.Verify(x => x.NavigateToDashboardAsync(InterviewId.FormatGuid()));
            auditLogService.Verify(x => x.Write(It.Is<ApproveInterviewAuditLogEntity>(c => c.InterviewKey == interviewKey.ToString())));
        }

        [Test]
        public async Task when_rejected_should_execute_reject_command()
        {
            var commandService = new Mock<ICommandService>();
            var navigationService = new Mock<IViewModelNavigationService>();
            var auditLogService = new Mock<IAuditLogService>();

            var interview = Create.AggregateRoot.StatefulInterview(interviewId: InterviewId);
            var interviewKey = Create.Entity.InterviewKey(5);
            interview.Apply(Create.Event.InterviewKeyAssigned(interviewKey));
            interview.Complete(Id.gA, "", DateTime.UtcNow);

            var statefulInterviewRepository = Abc.SetUp.StatefulInterviewRepository(interview);
            var viewModel = CreateViewModel(interviewRepository: statefulInterviewRepository,
                commandService: commandService.Object,
                navigationService: navigationService.Object,
                auditLogService: auditLogService.Object);
            viewModel.Configure(InterviewId.FormatGuid(), Create.Other.NavigationState(statefulInterviewRepository));

            // Act
            await viewModel.Reject.ExecuteAsync();
            
            // Assert
            commandService.Verify(x => x.ExecuteAsync(It.Is<RejectInterviewCommand>(c => c.InterviewId == InterviewId), null, CancellationToken.None));
            navigationService.Verify(x => x.NavigateToDashboardAsync(InterviewId.FormatGuid()));
            auditLogService.Verify(x => x.Write(It.Is<RejectInterviewAuditLogEntity>(c => c.InterviewKey == interviewKey.ToString())));
        }

        [Test]
        public void should_not_allow_approving_rejected_interview_when_received_by_interviewer_tablet()
        {
            var status = InterviewStatus.RejectedBySupervisor;
            var interviewView = new InterviewView();
            interviewView.Id = InterviewId.FormatGuid();
            interviewView.ReceivedByInterviewerAtUtc = DateTime.UtcNow;

            var interviewsList = new SqliteInmemoryStorage<InterviewView>();
            interviewsList.Store(interviewView);
            
            var interview = Mock.Of<IStatefulInterview>(x => x.Status == status);
            var interviewRepository = Create.Storage.InterviewRepository(interview);

            var viewModel = CreateViewModel(interviewRepository: interviewRepository,
                interviewsList: interviewsList);

            // Act
            viewModel.Configure(interviewView.Id, Create.Other.NavigationState(interviewRepository));

            // Assert
            Assert.That(viewModel.Reject.CanExecute(), Is.False);
        }
        
        [Test]
        public void should_allow_supervisor_assigned_only_in_appropriate_status([Values]InterviewStatus status)
        {
            var allowedStatuses = new[]
                {InterviewStatus.SupervisorAssigned, InterviewStatus.RejectedBySupervisor, InterviewStatus.InterviewerAssigned};

            var interview = Mock.Of<IStatefulInterview>(x => x.Status == status);
            var interviewRepository = new Mock<IStatefulInterviewRepository>();
            interviewRepository.Setup(x => x.Get(It.IsAny<string>()))
                .Returns(interview);

            var viewModel = CreateViewModel(interviewRepository: interviewRepository.Object);

            // Act
            viewModel.Configure(Id.g1.FormatGuid(), Create.Other.NavigationState(interviewRepository.Object));

            // Assert
            if (allowedStatuses.Contains(status))
            {
                Assert.That(viewModel.Assign.CanExecute(), Is.True);
            }
            else
            {
                Assert.That(viewModel.Assign.CanExecute(), Is.False);
            }
        }

        [Test]
        public void should_include_supervisor_questions_in_counters()
        {
            var interview = Mock.Of<IStatefulInterview>(
                x => x.CountActiveAnsweredQuestionsInInterviewForSupervisor() == 3
                     && x.CountInvalidEntitiesInInterviewForSupervisor() == 2 
                     && x.CountActiveQuestionsInInterviewForSupervisor() == 8
            );
            var interviewRepository = new Mock<IStatefulInterviewRepository>();
            interviewRepository.Setup(x => x.Get(It.IsAny<string>()))
                .Returns(interview);

            var viewModel = CreateViewModel(interviewRepository: interviewRepository.Object);

            // Act
            viewModel.Configure(Id.g1.FormatGuid(), Create.Other.NavigationState(interviewRepository.Object));

            // Assert
            Assert.That(viewModel, Has.Property(nameof(viewModel.ErrorsCount)).EqualTo(2));
            Assert.That(viewModel, Has.Property(nameof(viewModel.AnsweredCount)).EqualTo(3));
            Assert.That(viewModel, Has.Property(nameof(viewModel.UnansweredCount)).EqualTo(5));
        }

        [Test]
        public void when_configure_and_interview_was_rejected_then_approve_and_reject_commands_should_not_be_executable()
        {
            var commandService = new Mock<ICommandService>();
            var navigationService = new Mock<IViewModelNavigationService>();
            var auditLogService = new Mock<IAuditLogService>();

            var interview = Create.AggregateRoot.StatefulInterview(interviewId: InterviewId);
            var interviewKey = Create.Entity.InterviewKey(5);
            interview.Apply(Create.Event.InterviewKeyAssigned(interviewKey));
            interview.Complete(Id.gA, "", DateTime.UtcNow);
            interview.Reject(Id.gA, "", DateTime.UtcNow);

            var statefulInterviewRepository = Abc.SetUp.StatefulInterviewRepository(interview);
            var viewModel = CreateViewModel(interviewRepository: statefulInterviewRepository,
                commandService: commandService.Object,
                navigationService: navigationService.Object,
                auditLogService: auditLogService.Object);
            viewModel.Configure(InterviewId.FormatGuid(), Create.Other.NavigationState(statefulInterviewRepository));

            // Assert
            viewModel.Approve.CanExecute().Should().BeTrue();
            viewModel.Reject.CanExecute().Should().BeFalse();
        }

        private SupervisorResolveInterviewViewModel CreateViewModel(IViewModelNavigationService viewModelNavigationService = null,
            ICommandService commandService = null,
            IPrincipal principal = null,
            IMvxMessenger messenger = null,
            IStatefulInterviewRepository interviewRepository = null,
            IEntitiesListViewModelFactory entitiesListViewModelFactory = null,
            ILastCompletionComments lastCompletionComments = null,
            InterviewStateViewModel interviewState = null,
            DynamicTextViewModel dynamicTextViewModel = null,
            IViewModelNavigationService navigationService = null,
            ILogger logger = null,
            IAuditLogService auditLogService = null,
            IPlainStorage<InterviewView> interviewsList = null,
            IUserInteractionService userInteractionService = null)
        {
            
            var stubInterviewsList = new SqliteInmemoryStorage<InterviewView>();
            stubInterviewsList.Store(new InterviewView
            {
                InterviewId = InterviewId,
                Id = InterviewId.FormatGuid()
            });
            
            
            return new SupervisorResolveInterviewViewModel(
                commandService ?? Create.Service.CommandService(),
                principal ?? Mock.Of<IPrincipal>(x => x.IsAuthenticated == true && x.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Id.gA)),
                
                interviewRepository ?? Abc.SetUp.StatefulInterviewRepository(Create.AggregateRoot.StatefulInterview(interviewId: InterviewId)),
                entitiesListViewModelFactory ?? Mock.Of<IEntitiesListViewModelFactory>(),
                lastCompletionComments ?? Mock.Of<ILastCompletionComments>(),
                interviewState ?? Mock.Of<InterviewStateViewModel>(),
                dynamicTextViewModel ?? Create.ViewModel.DynamicTextViewModel(),
                navigationService ?? Mock.Of<IViewModelNavigationService>(),
                logger ?? Mock.Of<ILogger>(),
                auditLogService ?? Mock.Of<IAuditLogService>(),
                interviewsList ?? stubInterviewsList,
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
                Mock.Of<ICalendarEventStorage>()
            );
        }
    }
}
