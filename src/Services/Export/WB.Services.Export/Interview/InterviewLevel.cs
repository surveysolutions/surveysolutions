using System;
using System.Collections.Generic;
using WB.Services.Export.Interview.Entities;

namespace WB.Services.Export.Interview
{
    public class InterviewLevel
    {
        public InterviewLevel()
        {
            this.ScopeVectors = new Dictionary<ValueVector<Guid>, int?>();
            this.RosterRowTitles = new Dictionary<Guid, string>();
            this.QuestionsSearchCache = new Dictionary<Guid, InterviewEntity>();
            this.Variables = new Dictionary<Guid, object>();
            this.DisabledVariables = new HashSet<Guid>();
        }
        public InterviewLevel(ValueVector<Guid> scopeVector, int? sortIndex, RosterVector vector)
            : this()
        {
            this.ScopeVectors = new Dictionary<ValueVector<Guid>, int?> { { scopeVector, sortIndex } };
            this.RosterVector = vector;
        }

        public RosterVector RosterVector { get; set; }
        public Dictionary<ValueVector<Guid>, int?> ScopeVectors { get; set; }
        public Dictionary<Guid, string> RosterRowTitles { get; set; }
        public Dictionary<Guid, InterviewEntity> QuestionsSearchCache { get; set; }
        public Dictionary<Guid, object> Variables { get; set; }
        public HashSet<Guid> DisabledVariables { get; set; }
    }
}
