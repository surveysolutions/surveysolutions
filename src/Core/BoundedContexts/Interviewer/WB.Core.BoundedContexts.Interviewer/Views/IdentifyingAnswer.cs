using System;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class IdentifyingAnswer
    {
        public virtual Guid QuestionId { get; set; }
        public virtual string Answer { get; set; }
    }
}