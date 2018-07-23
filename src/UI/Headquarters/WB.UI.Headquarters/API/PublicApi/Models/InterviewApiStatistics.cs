using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class InterviewApiStatistics
    {
        public int Answered { get; set; }
        public int NotAnswered { get; set; }
        public int Flagged { get; set; }
        public int NotFlagged { get; set; }
        public int Valid { get; set; }
        public int Invalid { get; set; }
        public int WithComments { get; set; }
        public int ForInterviewer { get; set; }
        public int ForSupervisor { get; set; }

        public Guid InterviewId { get; set; }
        public string InterviewKey { get; set; }
        public string Status { get; set; }
        public Guid ResponsibleId { get; set; }
        public string ResponsibleName { get; set; }
        public int NumberOfInterviewers { get; set; }
        public int NumberRejectionsBySupervisor { get; set; }
        public int NumberRejectionsByHq { get; set; }
        public TimeSpan? InterviewDuration { get; set; }
    }
}
