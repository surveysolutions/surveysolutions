using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;

namespace WB.UI.Headquarters.API.WebInterview.Services.Overview
{
    public class OverviewWebQuestionNode : OverviewQuestion
    {
        public OverviewWebQuestionNode(InterviewTreeQuestion treeQuestion) : base(treeQuestion)
        {
            this.Comments = treeQuestion.AnswerComments;
        }

        public List<AnswerComment> Comments { get; set; }
    }
}
