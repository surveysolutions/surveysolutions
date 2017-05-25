using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class AssignmentRow
    {
        public DateTime CreatedAtUtc { get; set; }
        public Guid ResponsibleId { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public int? Capacity { get; set; }
        public int InterviewsCount { get; set; }
        public int Id { get; set; }
        public string Responsible { get; set; }

        public Dictionary<string, string> IdentifyingQuestions { get; set; }
    }
}