using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MvvmCross.Plugin.Messenger;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.ViewModels
{
    [TestOf(typeof(SupervisorCompleteInterviewViewModel))]
    public class SupervisorCompleteInterviewViewModelTests
    {
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

            var statefulInterviewRepository = Setup.StatefulInterviewRepository(interview);
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
            
            var interview = Create.AggregateRoot.StatefulInterview(interviewId: InterviewId);
            interview.Complete(Id.gA, "", DateTime.UtcNow);

            var statefulInterviewRepository = Setup.StatefulInterviewRepository(interview);
            var viewModel = CreateViewModel(interviewRepository: statefulInterviewRepository,
                commandService: commandService.Object,
                navigationService: navigationService.Object);
            viewModel.Configure(InterviewId.FormatGuid(), Create.Other.NavigationState(statefulInterviewRepository));

            // Act
            await viewModel.Approve.ExecuteAsync();
            
            // Assert
            commandService.Verify(x => x.ExecuteAsync(It.Is<ApproveInterviewCommand>(c => c.InterviewId == InterviewId), null, CancellationToken.None));
            navigationService.Verify(x => x.NavigateToDashboardAsync(InterviewId.FormatGuid()));
        }

        [Test]
        public async Task when_rejected_should_execute_reject_command()
        {
            var commandService = new Mock<ICommandService>();
            var navigationService = new Mock<IViewModelNavigationService>();
            
            var interview = Create.AggregateRoot.StatefulInterview(interviewId: InterviewId);
            interview.Complete(Id.gA, "", DateTime.UtcNow);

            var statefulInterviewRepository = Setup.StatefulInterviewRepository(interview);
            var viewModel = CreateViewModel(interviewRepository: statefulInterviewRepository,
                commandService: commandService.Object,
                navigationService: navigationService.Object);
            viewModel.Configure(InterviewId.FormatGuid(), Create.Other.NavigationState(statefulInterviewRepository));

            // Act
            await viewModel.Reject.ExecuteAsync();
            
            // Assert
            commandService.Verify(x => x.ExecuteAsync(It.Is<RejectInterviewCommand>(c => c.InterviewId == InterviewId), null, CancellationToken.None));
            navigationService.Verify(x => x.NavigateToDashboardAsync(InterviewId.FormatGuid()));
        }

        private SupervisorCompleteInterviewViewModel CreateViewModel(IViewModelNavigationService viewModelNavigationService = null,
            ICommandService commandService = null,
            IPrincipal principal = null,
            IMvxMessenger messenger = null,
            IStatefulInterviewRepository interviewRepository = null,
            IEntitiesListViewModelFactory entitiesListViewModelFactory = null,
            ILastCompletionComments lastCompletionComments = null,
            InterviewStateViewModel interviewState = null,
            DynamicTextViewModel dynamicTextViewModel = null,
            IViewModelNavigationService navigationService = null,
            ILogger logger = null)
        {
            return new SupervisorCompleteInterviewViewModel(
                commandService ?? Create.Service.CommandService(),
                principal ?? Mock.Of<IPrincipal>(x => x.IsAuthenticated == true && x.CurrentUserIdentity == Mock.Of<IUserIdentity>(y => y.UserId == Id.gA)),
                messenger ?? Mock.Of<IMvxMessenger>(),
                interviewRepository ?? Setup.StatefulInterviewRepository(Create.AggregateRoot.StatefulInterview(interviewId: InterviewId)),
                entitiesListViewModelFactory ?? Mock.Of<IEntitiesListViewModelFactory>(),
                lastCompletionComments ?? Mock.Of<ILastCompletionComments>(),
                interviewState ?? Mock.Of<InterviewStateViewModel>(),
                dynamicTextViewModel ?? Create.ViewModel.DynamicTextViewModel(),
                navigationService ?? Mock.Of<IViewModelNavigationService>(),
                logger ?? Mock.Of<ILogger>()
            );
        }
    }
}
