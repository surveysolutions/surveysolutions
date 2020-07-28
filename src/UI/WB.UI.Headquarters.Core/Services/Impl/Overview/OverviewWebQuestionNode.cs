using System;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;

namespace WB.UI.Headquarters.API.WebInterview.Services.Overview
{
    public class OverviewWebQuestionNode : OverviewQuestion
    {
        public OverviewWebQuestionNode(InterviewTreeQuestion treeQuestion, IStatefulInterview interview, Guid currentUserId) : base(treeQuestion, interview, currentUserId)
        {
            if (treeQuestion.IsAudio)
                ControlType = "audio";
            if (treeQuestion.IsArea)
                ControlType = "area";
            if (treeQuestion.IsGps)
            {
                ControlType = "map";
                var gps = treeQuestion.GetAsInterviewTreeGpsQuestion()?.GetAnswer()?.Value;
                if (gps != null)
                {
                    this.Answer = $@"{{ ""latitude"": {gps.Latitude}, ""longitude"": {gps.Longitude} }}";
                }
            }

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
