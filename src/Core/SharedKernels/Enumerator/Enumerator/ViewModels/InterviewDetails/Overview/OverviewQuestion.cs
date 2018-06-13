using System.Globalization;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.Properties;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview
{
    public class OverviewQuestion : OverviewNode
    {
        public OverviewQuestion(InterviewTreeQuestion treeQuestion) : base(treeQuestion)
        {
            this.Answer = treeQuestion.GetAnswerAsString(CultureInfo.CurrentCulture);
            
            if (treeQuestion.AnswerComments.Count > 0)
            {
                this.State = OverviewNodeState.Commented;
            }
            else if (!treeQuestion.IsAnswered())
            {
                this.State = OverviewNodeState.Unanswered;
            }
            else if (!treeQuestion.IsValid)
            {
                this.State = OverviewNodeState.Invalid;
            }
            else
            {
                this.State = OverviewNodeState.Answered;
            }

            base.IsAnswered = treeQuestion.IsAnswered();
        }

        public string Answer { get; set; }

        public sealed override OverviewNodeState State { get; set; }
    }
}
