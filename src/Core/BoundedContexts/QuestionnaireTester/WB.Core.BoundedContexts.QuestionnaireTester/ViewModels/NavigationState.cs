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

        public async Task NavigateTo(Identity groupIdentity, Identity anchoredElementIdentity = null)
        {
            await this.commandService.WaitPendingCommandsAsync();

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
                previousNavigationItem.AnchoredElementIdentity = this.CurrentGroup;

                this.ChangeCurrentGroupAndFireEvent(previousNavigationItem.TargetGroup, previousNavigationItem);
            }
        }

        private void ChangeCurrentGroupAndFireEvent(Identity groupIdentity, NavigationParams navigationParams)
        {
            this.CurrentGroup = groupIdentity;

            if (this.OnGroupChanged != null)
            {
                var groupChangedEventArgs = new GroupChangedEventArgs
                {
                    TargetGroup = navigationParams.TargetGroup,
                    AnchoredElementIdentity = navigationParams.AnchoredElementIdentity
                };

                this.OnGroupChanged(groupChangedEventArgs);
            }

            GC.Collect();
        }
    }

    public delegate void GroupChanged(GroupChangedEventArgs newGroupIdentity);
}