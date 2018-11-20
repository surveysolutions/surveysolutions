using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;

namespace WB.UI.Headquarters.API.WebInterview.Services.Overview
{
    public class OverviewWebQuestionNode : OverviewQuestion
    {
        public OverviewWebQuestionNode(InterviewTreeQuestion treeQuestion, IStatefulInterview interview) : base(treeQuestion, interview)
        {
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
        
        public string ControlType { get; set; } = "text";
    }
}
