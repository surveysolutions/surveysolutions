using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewLevel
    {
        public InterviewLevel()
        {
            this.RosterRowTitles = new Dictionary<Guid, string>();
            this.QuestionsSearchCache = new Dictionary<Guid, InterviewQuestion>();
            this.Variables = new Dictionary<Guid, object>();
            this.DisabledVariables = new HashSet<Guid>();
        }
        public InterviewLevel(ValueVector<Guid> scopeVector, int? sortIndex, decimal[] vector)
            : this()
        {
            this.RosterScope = scopeVector;
            this.RosterVector = vector;
        }

        public decimal[] RosterVector { get; set; }
        public Dictionary<Guid, string> RosterRowTitles { get; set; }
        public Dictionary<Guid, InterviewQuestion> QuestionsSearchCache { get; set; }
        public Dictionary<Guid, object> Variables { get; set; }
        public HashSet<Guid> DisabledVariables { get; set; }

        public ValueVector<Guid> RosterScope { get; set; }
    }
}
