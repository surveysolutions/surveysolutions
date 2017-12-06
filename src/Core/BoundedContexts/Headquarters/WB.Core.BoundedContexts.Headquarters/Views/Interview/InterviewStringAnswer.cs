using System;
using System.Diagnostics;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    [DebuggerDisplay("{ToString()}")]
    public class InterviewStringAnswer
    {
        public Guid InterviewId { get; set; }
        public string Answer { get; set; }

        public override bool Equals(object obj)
        {
            var target = obj as InterviewStringAnswer;
            if (target == null) return false;

            return this.Equals(target);
        }

        protected bool Equals(InterviewStringAnswer other)
        {
            return InterviewId.Equals(other.InterviewId) && string.Equals(Answer, other.Answer);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (InterviewId.GetHashCode() * 397) ^ (Answer != null ? Answer.GetHashCode() : 0);
            }
        }

        public override string ToString() => $"{InterviewId} => {Answer}";
    }
}