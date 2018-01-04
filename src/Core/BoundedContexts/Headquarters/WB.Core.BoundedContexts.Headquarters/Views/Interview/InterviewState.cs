using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewState
    {
        public InterviewState(Guid id)
        {
            this.Id = id;
        }

        public Guid Id { get; }
        public Dictionary<Identity, bool> Enablement { get; set; } = new Dictionary<Identity, bool>();
        public Dictionary<Identity, int[]> Validity { get; set; } = new Dictionary<Identity, int[]>();
        public Dictionary<Identity, object> Answers { get; set; } = new Dictionary<Identity, object>();
        public List<Identity> ReadOnly { get; set; } = new List<Identity>();
        public List<Identity> Removed { get; set; } = new List<Identity>();

    }
}