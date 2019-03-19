using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewFlag
    {
        public virtual string InterviewId { get; set; }
        public virtual string Identity { get; set; }

        public override bool Equals(object obj) => obj is InterviewFlag flag &&
                                                   InterviewId == flag.InterviewId &&
                                                   Identity == flag.Identity;

        public override int GetHashCode()
        {
            var hashCode = -1791060676;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(InterviewId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Identity);
            return hashCode;
        }
    }
}
