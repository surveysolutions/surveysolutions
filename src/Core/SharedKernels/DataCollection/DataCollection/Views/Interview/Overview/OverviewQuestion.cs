using System;
using System.Globalization;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.SharedKernels.DataCollection.Views.Interview.Overview
{
    public class OverviewQuestion : OverviewNode
    {
        public OverviewQuestion(InterviewTreeQuestion treeQuestion, IStatefulInterview interview) : base(treeQuestion)
        {
            this.Answer = treeQuestion.GetAnswerAsString(CultureInfo.CurrentCulture);
            
            if (!treeQuestion.IsAnswered())
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
            this.HasErrors = interview.GetFailedValidationMessages(treeQuestion.Identity, null).Any();

            this.HasWarnings = interview.GetFailedWarningMessages(treeQuestion.Identity, string.Empty).Any();
            HasComment = treeQuestion.AnswerComments.Count > 0;
            this.AnswerTimeUtc = treeQuestion.AnswerTimeUtc;
            this.SupportsComments = true;

            if (treeQuestion.IsAudio)
                ControlType = "audio";
            if (treeQuestion.IsArea)
                ControlType = "area";
            if (treeQuestion.IsGps)
                ControlType = "map";
            if (treeQuestion.IsMultimedia)
            {
                ControlType = "image";
                if (!string.IsNullOrWhiteSpace(this.Answer))
                {
                    this.Answer = $@"?interviewId={interview.Id}&questionId={treeQuestion.Identity}&filename={Answer}";
                }
            }
        }

        public bool HasErrors { get; set; }

        public string Answer { get; set; }

        public DateTime? AnswerTimeUtc { get; set; }

        public bool HasComment { get; set; }

        public bool HasWarnings { get; set; }

        public string ControlType { get; set; } = "text";

        public sealed override OverviewNodeState State { get; set; }
    }
}
