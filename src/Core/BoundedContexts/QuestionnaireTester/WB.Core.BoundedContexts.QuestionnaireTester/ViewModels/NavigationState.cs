using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class NavigationState
    {
        public event GroupChanged OnGroupChanged;
        public string InterviewId { get; private set; }
        public string QuestionnaireId { get; private set; }

        private readonly Queue<Identity> navigationQueue = new Queue<Identity>();

        public void Init(string interviewId, string questionnaireId)
        {
            this.InterviewId = interviewId;
            this.QuestionnaireId = questionnaireId;
        }

        public void NavigateTo(Identity groupIdentity, bool shouldBeAddedToNavigationStack = true)
        {
            if (shouldBeAddedToNavigationStack)
                this.navigationQueue.Enqueue(groupIdentity);

            if (OnGroupChanged != null)
                OnGroupChanged(groupIdentity);
        }

        public void NavigateBack(Action navigateToIfHistoryIsEmpty)
        {
            if (navigateToIfHistoryIsEmpty == null) throw new ArgumentNullException("navigateToIfHistoryIsEmpty");

            if (navigationQueue.Count == 0)
                navigateToIfHistoryIsEmpty();
            else
            {
                var previousGroupIdentity = this.navigationQueue.Dequeue();
                this.NavigateTo(previousGroupIdentity);
            }
        }
    }

    public delegate void GroupChanged(Identity newGroupIdentity);
}