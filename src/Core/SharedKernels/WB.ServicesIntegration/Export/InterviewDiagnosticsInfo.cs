using System;

namespace WB.ServicesIntegration.Export
{
    public class InterviewDiagnosticsInfo
    {
        public Guid InterviewId { get; set; }
        public string InterviewKey { get; set; } = null!;
        public InterviewStatus Status { get; set; }
        public Guid ResponsibleId { get; set; }
        public string ResponsibleName { get; set; } = null!;
        public int NumberOfInterviewers { get; set; }
        public int NumberRejectionsBySupervisor { get; set; }
        public int NumberRejectionsByHq { get; set; }
        public int NumberValidQuestions { get; set; }
        public int NumberInvalidEntities { get; set; }
        public int NumberUnansweredQuestions { get; set; }
        public int NumberCommentedQuestions { get; set; }
        public long? InterviewDuration { get; set; }
        public int? NotAnsweredCount { get; set; }
    }
}
