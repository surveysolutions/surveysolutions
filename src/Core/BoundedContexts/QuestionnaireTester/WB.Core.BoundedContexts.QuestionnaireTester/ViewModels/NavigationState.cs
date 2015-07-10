using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class NavigationState
    {
        private readonly ICommandService commandService;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IUserInteractionAwaiter userInteractionAwaiter;

        public event GroupChanged OnGroupChanged;
        public event BeforeGroupChanged OnBeforeGroupChanged;

        public string InterviewId { get; private set; }
        public string QuestionnaireId { get; private set; }
        public Identity CurrentGroup { get; private set; }

        private readonly Stack<NavigationParams> navigationStack = new Stack<NavigationParams>();

        protected NavigationState(IUserInteractionAwaiter userInteractionAwaiter)
        {
            this.userInteractionAwaiter = userInteractionAwaiter;
        }

        public NavigationState(ICommandService commandService, IStatefulInterviewRepository interviewRepository, IUserInteractionAwaiter userInteractionAwaiter)
        {
            this.commandService = commandService;
            this.interviewRepository = interviewRepository;
            this.userInteractionAwaiter = userInteractionAwaiter;
        }

        public void Init(string interviewId, string questionnaireId)
        {
            this.InterviewId = interviewId;
            this.QuestionnaireId = questionnaireId;
        }

        public async Task NavigateTo(Identity groupIdentity, Identity anchoredElementIdentity = null)
        {
            await this.userInteractionAwaiter.WaitPendingUserInteractionsAsync();
            await this.commandService.WaitPendingCommandsAsync();

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

        public async Task NavigateBack(Action navigateToIfHistoryIsEmpty)
        {
            await this.userInteractionAwaiter.WaitPendingUserInteractionsAsync();
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
            if (this.OnBeforeGroupChanged != null)
            {
                this.OnBeforeGroupChanged(new BeforeGroupChangedEventArgs(this.CurrentGroup, navigationParams.TargetGroup));
            }

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

    public delegate void BeforeGroupChanged(BeforeGroupChangedEventArgs eventArgs);

    public delegate void GroupChanged(GroupChangedEventArgs newGroupIdentity);
}