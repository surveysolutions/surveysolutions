using System;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class SupervisorCompleteInterviewViewModel : CompleteInterviewViewModel
    {
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IViewModelNavigationService navigationService;
        private InterviewStatus status;
        private Guid interviewResponsible;

        public SupervisorCompleteInterviewViewModel(
            ICommandService commandService, 
            IPrincipal principal, 
            IMvxMessenger messenger, 
            IStatefulInterviewRepository interviewRepository,
            IEntitiesListViewModelFactory entitiesListViewModelFactory, 
            ILastCompletionComments lastCompletionComments, 
            InterviewStateViewModel interviewState, 
            DynamicTextViewModel dynamicTextViewModel, 
            IViewModelNavigationService navigationService,
            ILogger logger) : 
                base(navigationService,
                commandService,
                principal,
                messenger,
                entitiesListViewModelFactory,
                lastCompletionComments,
                interviewState,
                dynamicTextViewModel,
                logger)
        {
            this.commandService = commandService;
            this.principal = principal;
            this.interviewRepository = interviewRepository;
            this.navigationService = navigationService;
        }

        public override void Configure(string interviewId, NavigationState navigationState)
        {
            base.Configure(interviewId, navigationState);

            this.Name.InitAsStatic(InterviewDetails.Resolve);

            this.CompleteCommentLabel = InterviewDetails.ResolveComment;

            var interview = this.interviewRepository.Get(interviewId);
            this.status = interview.Status;
            this.interviewResponsible = interview.CurrentResponsibleId;
        }

        public IMvxAsyncCommand Approve => new MvxAsyncCommand(async () =>
        {
            var command = new ApproveInterviewCommand(interviewId, this.principal.CurrentUserIdentity.UserId,
                CompleteComment, DateTime.UtcNow);
            await this.commandService.ExecuteAsync(command);
            await this.navigationService.NavigateToDashboardAsync(interviewId.FormatGuid());

        }, ApproveRejectAllowed);

        public IMvxAsyncCommand Reject => new MvxAsyncCommand(async () =>
        {
            var command = new RejectInterviewCommand(interviewId, this.principal.CurrentUserIdentity.UserId,
                CompleteComment, DateTime.UtcNow);
            await this.commandService.ExecuteAsync(command);
            await this.navigationService.NavigateToDashboardAsync(interviewId.FormatGuid());
        }, ApproveRejectAllowed);

        private bool ApproveRejectAllowed()
        {
            return this.status == InterviewStatus.Completed || this.status == InterviewStatus.RejectedByHeadquarters;
        }

        public IMvxCommand Assign => new MvxCommand(() => { });
    }
}
