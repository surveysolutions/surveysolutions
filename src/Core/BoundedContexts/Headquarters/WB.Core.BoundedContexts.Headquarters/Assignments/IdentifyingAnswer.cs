using System;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class IdentifyingAnswer
    {
        protected IdentifyingAnswer()
        {
        }

        public IdentifyingAnswer(Assignment assignment)
        {
            this.Assignment = assignment;
        }

        public virtual Guid QuestionId { get; set; }

        public virtual string Answer { get; set; }

        public virtual  Assignment Assignment { get; protected set; }
    }
}