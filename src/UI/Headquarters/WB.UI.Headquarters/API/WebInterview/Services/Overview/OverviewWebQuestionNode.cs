using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;

namespace WB.UI.Headquarters.API.WebInterview.Services.Overview
{
    public class OverviewWebQuestionNode : OverviewQuestion
    {
        public OverviewWebQuestionNode(InterviewTreeQuestion treeQuestion, IStatefulInterview interview) : base(treeQuestion, interview)
        {
            this.Comments = treeQuestion.AnswerComments;
        }

        public List<AnswerComment> Comments { get; set; }
    }
}
