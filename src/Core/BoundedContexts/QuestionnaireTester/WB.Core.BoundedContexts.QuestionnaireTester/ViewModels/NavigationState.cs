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

        private readonly Stack<Identity> navigationQueue = new Stack<Identity>();

        public void Init(string interviewId, string questionnaireId)
        {
            this.InterviewId = interviewId;
            this.QuestionnaireId = questionnaireId;
        }

        public void NavigateTo(Identity groupIdentity)
        {
            this.navigationQueue.Push(groupIdentity);

            if (OnGroupChanged != null)
                OnGroupChanged(groupIdentity);
        }

        public void NavigateBack(Action navigateToIfHistoryIsEmpty)
        {
            if (navigateToIfHistoryIsEmpty == null) throw new ArgumentNullException("navigateToIfHistoryIsEmpty");

            // remove current group from stack
            this.navigationQueue.Pop();

            if (navigationQueue.Count == 0)
                navigateToIfHistoryIsEmpty();
            else
            {
                var previousGroupIdentity = this.navigationQueue.Pop();
                this.NavigateTo(previousGroupIdentity);
            }
        }
    }

    public delegate void GroupChanged(Identity newGroupIdentity);
}