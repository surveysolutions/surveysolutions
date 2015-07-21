using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class NavigationState
    {
        private readonly ICommandService commandService;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IUserInteraction userInteractionAwaiter;

        protected NavigationState()
        {
        }

        public virtual event GroupChanged GroupChanged;
        public virtual event BeforeGroupChanged BeforeGroupChanged;

        public virtual string InterviewId { get; private set; }
        public virtual string QuestionnaireId { get; private set; }
        public virtual Identity CurrentGroup { get; private set; }

        private readonly Stack<NavigationParams> navigationStack = new Stack<NavigationParams>();

        protected NavigationState(IUserInteraction userInteractionAwaiter)
        {
            this.userInteractionAwaiter = userInteractionAwaiter;
        }

        public NavigationState(
            ICommandService commandService, 
            IStatefulInterviewRepository interviewRepository,
            IUserInteraction userInteractionAwaiter)
        {
            this.commandService = commandService;
            this.interviewRepository = interviewRepository;
            this.userInteractionAwaiter = userInteractionAwaiter;
        }

        public void Init(string interviewId, string questionnaireId)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (questionnaireId == null) throw new ArgumentNullException("questionnaireId");

            this.InterviewId = interviewId;
            this.QuestionnaireId = questionnaireId;
        }

        public async Task NavigateToAsync(Identity groupIdentity, Identity anchoredElementIdentity = null)
        {
            await this.userInteractionAwaiter.WaitPendingUserInteractionsAsync().ConfigureAwait(false);
            await this.commandService.WaitPendingCommandsAsync().ConfigureAwait(false);

            if (!this.CanNavigateTo(groupIdentity))
                return;

            var navigationItem = new NavigationParams { TargetGroup = groupIdentity };

            while (this.navigationStack.Contains(navigationItem))
            {
                this.navigationStack.Pop();
            }

            if (anchoredElementIdentity != null)
            {
                navigationItem.AnchoredElementIdentity = anchoredElementIdentity;
            }

            this.navigationStack.Push(navigationItem);

            this.ChangeCurrentGroupAndFireEvent(groupIdentity, navigationItem);
        }

        private bool CanNavigateTo(Identity group)
        {
            var interview = this.interviewRepository.Get(this.InterviewId);

            return interview.HasGroup(group) && interview.IsEnabled(group);
        }

        public async Task NavigateBackAsync(Action navigateToIfHistoryIsEmpty)
        {
            await this.userInteractionAwaiter.WaitPendingUserInteractionsAsync().ConfigureAwait(false);
            await this.commandService.WaitPendingCommandsAsync().ConfigureAwait(false);

            if (navigateToIfHistoryIsEmpty == null) throw new ArgumentNullException("navigateToIfHistoryIsEmpty");

            // remove current group from stack
            if (this.navigationStack.Count != 0)
            {
                this.navigationStack.Pop();
            }

            if (this.navigationStack.Count == 0)
                navigateToIfHistoryIsEmpty.Invoke();
            else
            {
                NavigationParams previousNavigationItem = this.navigationStack.Peek();
                previousNavigationItem.AnchoredElementIdentity = this.CurrentGroup;

                while (!CanNavigateTo(previousNavigationItem.TargetGroup) || previousNavigationItem.TargetGroup.Equals(this.CurrentGroup))
                {
                    if (this.navigationStack.Count == 0)
                    {
                        navigateToIfHistoryIsEmpty.Invoke();
                        return;
                    }

                    previousNavigationItem = this.navigationStack.Pop();
                } 

                this.ChangeCurrentGroupAndFireEvent(previousNavigationItem.TargetGroup, previousNavigationItem);
            }
        }

        private void ChangeCurrentGroupAndFireEvent(Identity groupIdentity, NavigationParams navigationParams)
        {
            if (this.BeforeGroupChanged != null)
            {
                this.BeforeGroupChanged(new BeforeGroupChangedEventArgs(this.CurrentGroup, navigationParams.TargetGroup));
            }

            this.CurrentGroup = groupIdentity;

            if (this.GroupChanged != null)
            {
                var groupChangedEventArgs = new GroupChangedEventArgs
                {
                    TargetGroup = navigationParams.TargetGroup,
                    AnchoredElementIdentity = navigationParams.AnchoredElementIdentity
                };
                
                this.GroupChanged(groupChangedEventArgs);
            }

            GC.Collect();
        }
    }

    public delegate void BeforeGroupChanged(BeforeGroupChangedEventArgs eventArgs);

    public delegate void GroupChanged(GroupChangedEventArgs newGroupIdentity);
}