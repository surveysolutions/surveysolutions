using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewState : StateBase
    {
        public Dictionary<InterviewStateIdentity, bool> Enablement { get; set; } = new Dictionary<InterviewStateIdentity, bool>();
        public Dictionary<InterviewStateIdentity, InterviewStateValidation> Validity { get; set; } = new Dictionary<InterviewStateIdentity, InterviewStateValidation>();
        public Dictionary<InterviewStateIdentity, InterviewStateValidation> Warnings { get; set; } = new Dictionary<InterviewStateIdentity, InterviewStateValidation>();
        public Dictionary<InterviewStateIdentity, InterviewStateAnswer> Answers { get; set; } = new Dictionary<InterviewStateIdentity, InterviewStateAnswer>();
        public List<InterviewStateIdentity> ReadOnly { get; set; } = new List<InterviewStateIdentity>();
        public List<InterviewStateIdentity> Removed { get; set; } = new List<InterviewStateIdentity>();
    }
}