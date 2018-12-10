using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewState : StateBase
    {
        public Dictionary<InterviewStateIdentity, bool> Enablement { get; set; } = new Dictionary<InterviewStateIdentity, bool>();
        public Dictionary<InterviewStateIdentity, InterviewStateValidation> Validity { get; set; } = new Dictionary<InterviewStateIdentity, InterviewStateValidation>();
        public Dictionary<InterviewStateIdentity, InterviewStateValidation> Warnings { get; set; } = new Dictionary<InterviewStateIdentity, InterviewStateValidation>();
        public Dictionary<InterviewStateIdentity, InterviewStateAnswer> Answers { get; set; } = new Dictionary<InterviewStateIdentity, InterviewStateAnswer>();
        public HashSet<InterviewStateIdentity> ReadOnly { get; set; } = new HashSet<InterviewStateIdentity>();
        public HashSet<InterviewStateIdentity> Removed { get; set; } = new HashSet<InterviewStateIdentity>();
    }
}
