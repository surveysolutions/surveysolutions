using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Factories
{
    public class InterviewDiagnosticsInfo
    {
        public Guid InterviewId { get; set; }
        public string InterviewKey { get; set; }
        public InterviewStatus Status { get; set; }
        public Guid ResponsibleId { get; set; }
        public string ResponsibleName { get; set; }
        public int NumberOfInterviewers { get; set; }
        public int NumberRejectionsBySupervisor { get; set; }
        public int NumberRejectionsByHq { get; set; }
        public int NumberValidQuestions { get; set; }
        public int NumberInvalidEntities { get; set; }
        public int NumberUnansweredQuestions { get; set; }
        public int NumberCommentedQuestions { get; set; }
        public long? InterviewDuration { get; set; }
    }

    public interface IInterviewDiagnosticsFactory
    {
        InterviewDiagnosticsInfo GetById(Guid interviewId);

        List<InterviewDiagnosticsInfo> GetByBatchIds(IEnumerable<Guid> interviewIds);
    }
}
