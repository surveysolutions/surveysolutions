using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class NavigationState
    {
        private readonly ICommandService commandService;

        public event GroupChanged OnGroupChanged;
        public string InterviewId { get; private set; }
        public string QuestionnaireId { get; private set; }
        public Identity CurrentGroup { get; private set; }

        private readonly Stack<NavigationParams> navigationStack = new Stack<NavigationParams>();

        protected NavigationState() { }

        public NavigationState(ICommandService commandService)
        {
            this.commandService = commandService;
        }

        public void Init(string interviewId, string questionnaireId)
        {
            this.InterviewId = interviewId;
            this.QuestionnaireId = questionnaireId;
        }

        public async Task NavigateTo(Identity groupIdentity)
        {
            await this.commandService.WaitPendingCommandsAsync();

            var navigationItem = new NavigationParams { TargetGroup = groupIdentity };

            while (this.navigationStack.Contains(navigationItem))
            {
                this.navigationStack.Pop();
            }

            this.navigationStack.Push(navigationItem);

            this.ChangeCurrentGroupAndFireEvent(groupIdentity, navigationItem);
        }

        public async Task LeaveAnchorAndNavigateTo(Identity groupIdentity, Identity parentGroupIdentity = null)
        {
            await this.commandService.WaitPendingCommandsAsync();

            if (parentGroupIdentity != null && this.navigationStack.Count > 0)
            {
                var previousNavigationItem = this.navigationStack.Peek();

                if (previousNavigationItem != null && previousNavigationItem.TargetGroup.Equals(parentGroupIdentity))
                {
                    previousNavigationItem.AnchoredElementIdentity = groupIdentity;
                }
            }
            var navigationItem = new NavigationParams { TargetGroup = groupIdentity };

            this.navigationStack.Push(navigationItem);

            this.ChangeCurrentGroupAndFireEvent(groupIdentity, navigationItem);
        }

        public async Task NavigateToGroupWithAnchor(Identity groupIdentity, Identity anchoredElementIdentity)
        {
            await this.commandService.WaitPendingCommandsAsync();

            var navigationItem = new NavigationParams { TargetGroup = groupIdentity, AnchoredElementIdentity = anchoredElementIdentity };

            this.navigationStack.Push(navigationItem);

            this.ChangeCurrentGroupAndFireEvent(groupIdentity, navigationItem);
        }

        public async Task NavigateBack(Action navigateToIfHistoryIsEmpty)
        {
            await this.commandService.WaitPendingCommandsAsync();

            if (navigateToIfHistoryIsEmpty == null) throw new ArgumentNullException("navigateToIfHistoryIsEmpty");

            // remove current group from stack
            this.navigationStack.Pop();

            if (this.navigationStack.Count == 0)
                navigateToIfHistoryIsEmpty.Invoke();
            else
            {
                var previousNavigationItem = this.navigationStack.Peek();

                this.ChangeCurrentGroupAndFireEvent(previousNavigationItem.TargetGroup, previousNavigationItem);
            }
        }

        private void ChangeCurrentGroupAndFireEvent(Identity groupIdentity, NavigationParams navigationParams)
        {
            this.CurrentGroup = groupIdentity;

            if (this.OnGroupChanged != null)
            {
                this.OnGroupChanged(navigationParams);
            }
        }
    }

    public delegate void GroupChanged(NavigationParams newGroupIdentity);
}