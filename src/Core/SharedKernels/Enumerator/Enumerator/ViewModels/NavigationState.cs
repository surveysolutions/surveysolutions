using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class NavigationState
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IViewModelNavigationService viewModelNavigationService;

        protected NavigationState()
        {
        }

        public virtual event GroupChanged ScreenChanged;
        public virtual event BeforeGroupChanged BeforeScreenChanged;
        public virtual string InterviewId { get; private set; }
        public virtual string QuestionnaireId { get; private set; }
        public virtual Identity CurrentGroup { get; private set; }
        public virtual ScreenType CurrentScreenType { get; private set; }

        public NavigationIdentity CurrentNavigationIdentity
        {
            get
            {
                switch (this.CurrentScreenType)
                {
                    case ScreenType.Complete:
                        return NavigationIdentity.CreateForCompleteScreen();
                    case ScreenType.Cover:
                        return NavigationIdentity.CreateForCoverScreen();
                    case ScreenType.Identifying:
                        return NavigationIdentity.CreateForPrefieldScreen();
                    case ScreenType.Overview:
                        return NavigationIdentity.CreateForOverviewScreen();
                    default:
                        return NavigationIdentity.CreateForGroup(this.CurrentGroup);
                }
            }
        }

        private readonly Stack<NavigationIdentity> navigationStack = new Stack<NavigationIdentity>();

        public NavigationState(
            IStatefulInterviewRepository interviewRepository,
            IViewModelNavigationService viewModelNavigationService)
        {
            this.interviewRepository = interviewRepository;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public void Init(string interviewId, string questionnaireId)
        {
            this.InterviewId = interviewId ?? throw new ArgumentNullException(nameof(interviewId));
            this.QuestionnaireId = questionnaireId ?? throw new ArgumentNullException(nameof(questionnaireId));
        }

        public Task NavigateTo(NavigationIdentity navigationItem)
        {
            if (this.viewModelNavigationService.HasPendingOperations)
            {
                this.viewModelNavigationService.ShowWaitMessage();
                return Task.CompletedTask;
            }

            return this.NavigateToImpl(navigationItem);
        }

        public void NavigateBack(Action navigateToIfHistoryIsEmpty)
        {
            if (this.viewModelNavigationService.HasPendingOperations)
                this.viewModelNavigationService.ShowWaitMessage();
            else
                this.NavigateBackImpl(navigateToIfHistoryIsEmpty);
        }

        private async Task NavigateToImpl(NavigationIdentity navigationItem)
        {
            if (navigationItem.TargetScreen == ScreenType.Identifying)
            {
                await viewModelNavigationService.NavigateToPrefilledQuestionsAsync(InterviewId);
            }
            else
            {

                if (navigationItem.TargetScreen == ScreenType.Group)
                {
                    if (!this.CanNavigateTo(navigationItem)) return;

                    while (
                        this.navigationStack.Any(
                            x => x.TargetGroup != null && x.TargetGroup.Equals(navigationItem.TargetGroup)))
                    {
                        this.navigationStack.Pop();
                    }
                }

                this.navigationStack.Push(navigationItem);

                this.ChangeCurrentGroupAndFireEvent(navigationItem);
            }
        }

        private bool CanNavigateTo(NavigationIdentity navigationIdentity)
        {
            var interview = this.interviewRepository.Get(this.InterviewId);

            if (navigationIdentity.TargetScreen == ScreenType.Complete || 
                navigationIdentity.TargetScreen == ScreenType.Cover || 
                navigationIdentity.TargetScreen == ScreenType.Identifying ||
                navigationIdentity.TargetScreen == ScreenType.Overview
                )

                return true;

            return interview.HasGroup(navigationIdentity.TargetGroup) && interview.IsEnabled(navigationIdentity.TargetGroup);
        }

        private void NavigateBackImpl(Action navigateToIfHistoryIsEmpty)
        {
            if (navigateToIfHistoryIsEmpty == null) 
                throw new ArgumentNullException(nameof(navigateToIfHistoryIsEmpty));

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

                while (!this.CanNavigateTo(previousNavigationItem) || (previousNavigationItem.TargetGroup != null && previousNavigationItem.TargetGroup.Equals(this.CurrentGroup)))
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

            var previousStage = this.CurrentScreenType;
            var previousGroup = this.CurrentGroup;

            this.CurrentGroup = navigationIdentity.TargetGroup;
            this.CurrentScreenType = navigationIdentity.TargetScreen;

            this.ScreenChanged?.Invoke(new ScreenChangedEventArgs(
                navigationIdentity.TargetScreen,
                navigationIdentity.TargetGroup,
                navigationIdentity.AnchoredElementIdentity,
                previousStage,
                previousGroup));
        }
    }

    public delegate void BeforeGroupChanged(BeforeScreenChangedEventArgs eventArgs);

    public delegate void GroupChanged(ScreenChangedEventArgs eventArgs);
}
