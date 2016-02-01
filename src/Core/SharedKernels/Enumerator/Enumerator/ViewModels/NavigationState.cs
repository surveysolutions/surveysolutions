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

        public virtual event GroupChanged ScreenChanged;
        public virtual event BeforeGroupChanged BeforeScreenChanged;

        private bool isNavigationStarted = false;
        public virtual string InterviewId { get; private set; }
        public virtual string QuestionnaireId { get; private set; }
        public virtual Identity CurrentGroup { get; private set; }
        public virtual ScreenType CurrentScreenType { get; private set; }

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
            if (this.isNavigationStarted)
                return;

            try
            {
                this.isNavigationStarted = true;

                await this.WaitPendingOperationsCompletion();

                this.NavigateTo(navigationItem);
            }
            finally
            {
                this.isNavigationStarted = false;
            }
        }

        public async Task NavigateBackAsync(Action navigateToIfHistoryIsEmpty)
        {
            if (this.isNavigationStarted)
                return;

            try
            {
                this.isNavigationStarted = true;

                await this.WaitPendingOperationsCompletion();

                this.NavigateBack(navigateToIfHistoryIsEmpty);
            }
            finally
            {
                this.isNavigationStarted = false;
            }
        }

        private async Task WaitPendingOperationsCompletion()
        {
            await this.userInteractionServiceAwaiter.WaitPendingUserInteractionsAsync().ConfigureAwait(false);
            await this.userInterfaceStateService.WaitWhileUserInterfaceIsRefreshingAsync().ConfigureAwait(false);
            await this.commandService.WaitPendingCommandsAsync().ConfigureAwait(false);
        }

        private void NavigateTo(NavigationIdentity navigationItem)
        {
            if (navigationItem.TargetScreen == ScreenType.Group)
            {
                if (!this.CanNavigateTo(navigationItem.TargetGroup)) return;

                while (this.navigationStack.Any(x => x.TargetGroup!=null && x.TargetGroup.Equals(navigationItem.TargetGroup)))
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
            this.BeforeScreenChanged?.Invoke(new BeforeScreenChangedEventArgs(this.CurrentGroup, navigationIdentity.TargetGroup));

            this.CurrentGroup = navigationIdentity.TargetGroup;
            this.CurrentScreenType = navigationIdentity.TargetScreen;

            if (this.ScreenChanged != null)
            {
                var screenChangedEventArgs = new ScreenChangedEventArgs
                {
                    TargetGroup = navigationIdentity.TargetGroup,
                    AnchoredElementIdentity = navigationIdentity.AnchoredElementIdentity,
                    TargetScreen = navigationIdentity.TargetScreen
                };

                this.ScreenChanged(screenChangedEventArgs);
            }
        }
    }

    public delegate void BeforeGroupChanged(BeforeScreenChangedEventArgs eventArgs);

    public delegate void GroupChanged(ScreenChangedEventArgs eventArgs);
}