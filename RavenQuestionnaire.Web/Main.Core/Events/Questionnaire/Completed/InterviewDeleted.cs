using System;

namespace Main.Core.Events.Questionnaire.Completed
{
    public class InterviewDeleted
    {
        public Guid InterviewId { get; set; }

        public Guid DeletedBy { get; set; }
    }
}
