using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewState
    {
        public InterviewState(Guid id)
        {
            this.Id = id;
        }

        public Guid Id { get; }
        public Dictionary<InterviewStateIdentity, bool> Enablement { get; set; } = new Dictionary<InterviewStateIdentity, bool>();
        public Dictionary<InterviewStateIdentity, InterviewStateValidation> Validity { get; set; } = new Dictionary<InterviewStateIdentity, InterviewStateValidation>();
        public Dictionary<InterviewStateIdentity, InterviewStateAnswer> Answers { get; set; } = new Dictionary<InterviewStateIdentity, InterviewStateAnswer>();
        public List<InterviewStateIdentity> ReadOnly { get; set; } = new List<InterviewStateIdentity>();
        public List<InterviewStateIdentity> Removed { get; set; } = new List<InterviewStateIdentity>();

    }
}