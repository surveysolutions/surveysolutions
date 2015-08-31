using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class NavigationState
    {
        private readonly ICommandService commandService;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IUserInteractionService userInteractionServiceAwaiter;
        private readonly IUserInterfaceStateService userInterfaceStateService;

        protected NavigationState()
        {
        }

        public virtual event GroupChanged GroupChanged;
        public virtual event BeforeGroupChanged BeforeGroupChanged;

        private bool isNavigatingInExecutionInCurrentMoment = false;
        public virtual string InterviewId { get; private set; }
        public virtual string QuestionnaireId { get; private set; }
        public virtual Identity CurrentGroup { get; private set; }
        public virtual ScreenType CurrentGroupType { get; private set; }

        private readonly Stack<NavigationIdentity> navigationStack = new Stack<NavigationIdentity>();

        protected NavigationState(IUserInteractionService userInteractionServiceAwaiter)
        {
            this.userInteractionServiceAwaiter = userInteractionServiceAwaiter;
        }

        public NavigationState(
            ICommandService commandService, 
            IStatefulInterviewRepository interviewRepository,
            IUserInteractionService userInteractionServiceAwaiter, 
            IUserInterfaceStateService userInterfaceStateService)
        {
            this.commandService = commandService;
            this.interviewRepository = interviewRepository;
            this.userInteractionServiceAwaiter = userInteractionServiceAwaiter;
            this.userInterfaceStateService = userInterfaceStateService;
        }

        public void Init(string interviewId, string questionnaireId)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (questionnaireId == null) throw new ArgumentNullException("questionnaireId");

            this.InterviewId = interviewId;
            this.QuestionnaireId = questionnaireId;
        }

        public async Task NavigateToAsync(NavigationIdentity navigationItem)
        {
            await this.DoNavigationActionAsync((() => this.NavigateTo(navigationItem)));
        }       
        
        private async Task DoNavigationActionAsync(Action action)
        {
            if (this.isNavigatingInExecutionInCurrentMoment)
                return;

            try
            {
                this.isNavigatingInExecutionInCurrentMoment = true;

                await this.userInteractionServiceAwaiter.WaitPendingUserInteractionsAsync().ConfigureAwait(false);
                await this.userInterfaceStateService.WaitWhileUserInterfaceIsRefreshingAsync().ConfigureAwait(false);
                await this.commandService.WaitPendingCommandsAsync().ConfigureAwait(false);

                action.Invoke();
            }
            finally 
            {
                this.isNavigatingInExecutionInCurrentMoment = false;
            }
        }

        private void NavigateTo(NavigationIdentity navigationItem)
        {
            if (navigationItem.ScreenType == ScreenType.Group)
            {
                if (!this.CanNavigateTo(navigationItem.TargetGroup)) return;

                while (this.navigationStack.Any(x => x.TargetGroup.Equals(navigationItem.TargetGroup)))
                {
                    this.navigationStack.Pop();
                }
            }

            this.navigationStack.Push(navigationItem);

            this.ChangeCurrentGroupAndFireEvent(navigationItem);
        }

        private bool CanNavigateTo(Identity group)
        {
            var interview = this.interviewRepository.Get(this.InterviewId);

            return interview.HasGroup(group) && interview.IsEnabled(group);
        }

        public async Task NavigateBackAsync(Action navigateToIfHistoryIsEmpty)
        {
            await this.DoNavigationActionAsync((() => this.NavigateBack(navigateToIfHistoryIsEmpty)));
        }

        private void NavigateBack(Action navigateToIfHistoryIsEmpty)
        {
            if (navigateToIfHistoryIsEmpty == null) 
                throw new ArgumentNullException("navigateToIfHistoryIsEmpty");

            // remove current group from stack
            if (this.navigationStack.Count != 0)
            {
                this.navigationStack.Pop();
            }

            if (this.navigationStack.Count == 0)
                navigateToIfHistoryIsEmpty.Invoke();
            else
            {
                NavigationIdentity previousNavigationItem = this.navigationStack.Peek();
                previousNavigationItem.AnchoredElementIdentity = this.CurrentGroup;

                while (!this.CanNavigateTo(previousNavigationItem.TargetGroup) ||
                       previousNavigationItem.TargetGroup.Equals(this.CurrentGroup))
                {
                    if (this.navigationStack.Count == 0)
                    {
                        navigateToIfHistoryIsEmpty.Invoke();
                        return;
                    }

                    previousNavigationItem = this.navigationStack.Pop();
                }

                this.ChangeCurrentGroupAndFireEvent(previousNavigationItem);
            }
        }

        private void ChangeCurrentGroupAndFireEvent(NavigationIdentity navigationIdentity)
        {
            if (this.BeforeGroupChanged != null)
            {
                this.BeforeGroupChanged(new BeforeGroupChangedEventArgs(this.CurrentGroup, navigationIdentity.TargetGroup));
            }

            this.CurrentGroup = navigationIdentity.TargetGroup;
            this.CurrentGroupType = navigationIdentity.ScreenType;

            if (this.GroupChanged != null)
            {
                var groupChangedEventArgs = new GroupChangedEventArgs
                {
                    TargetGroup = navigationIdentity.TargetGroup,
                    AnchoredElementIdentity = navigationIdentity.AnchoredElementIdentity,
                    ScreenType = navigationIdentity.ScreenType
                };
                
                this.GroupChanged(groupChangedEventArgs);
            }
        }
    }

    public delegate void BeforeGroupChanged(BeforeGroupChangedEventArgs eventArgs);

    public delegate void GroupChanged(GroupChangedEventArgs newGroupIdentity);
}