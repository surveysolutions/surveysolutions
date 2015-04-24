using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class NavigationState
    {
        public event GroupChanged OnGroupChanged;
        public string InterviewId { get; private set; }
        public string QuestionnaireId { get; private set; }
        public Identity CurrentGroup { get; private set; }

        public void Init(string interviewId, string questionnaireId, Identity currentGroupIdentity)
        {
            this.InterviewId = interviewId;
            this.QuestionnaireId = questionnaireId;
            this.CurrentGroup = currentGroupIdentity;

            if (OnGroupChanged != null)
                OnGroupChanged(currentGroupIdentity);
        }
    }

    public delegate void GroupChanged(Identity newGroupIdentity);
}