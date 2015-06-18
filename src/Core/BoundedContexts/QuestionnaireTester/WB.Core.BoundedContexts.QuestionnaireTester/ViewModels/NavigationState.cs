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
        public Identity CurrentGroup { get; private set; }

        private readonly Stack<Identity> navigationStack = new Stack<Identity>();

        public void Init(string interviewId, string questionnaireId)
        {
            this.InterviewId = interviewId;
            this.QuestionnaireId = questionnaireId;
        }

        public void NavigateTo(Identity groupIdentity)
        {
            while (this.navigationStack.Contains(groupIdentity))
            {
                this.navigationStack.Pop();
            }

            this.navigationStack.Push(groupIdentity);

            this.CurrentGroup = groupIdentity;

            if (OnGroupChanged != null)
                OnGroupChanged(groupIdentity);
        }

        public void NavigateBack(Action navigateToIfHistoryIsEmpty)
        {
            if (navigateToIfHistoryIsEmpty == null) throw new ArgumentNullException("navigateToIfHistoryIsEmpty");

            // remove current group from stack
            this.navigationStack.Pop();

            if (this.navigationStack.Count == 0)
                navigateToIfHistoryIsEmpty();
            else
            {
                var previousGroupIdentity = this.navigationStack.Pop();
                this.NavigateTo(previousGroupIdentity);
            }
        }
    }

    public delegate void GroupChanged(Identity newGroupIdentity);
}