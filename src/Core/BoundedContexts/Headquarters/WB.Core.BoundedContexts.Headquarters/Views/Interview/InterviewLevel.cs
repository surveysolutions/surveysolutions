using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewLevel
    {
        public InterviewLevel()
        {
            this.ScopeVectors = new Dictionary<ValueVector<Guid>, int?>();
            this.DisabledGroups = new HashSet<Guid>();
            this.RosterRowTitles = new Dictionary<Guid, string>();
            this.QuestionsSearchCache = new Dictionary<Guid, InterviewQuestion>();
            this.Variables = new Dictionary<Guid, object>();
            this.StaticTexts = new Dictionary<Guid, InterviewStaticText>();
            this.DisabledVariables = new HashSet<Guid>();
        }
        public InterviewLevel(ValueVector<Guid> scopeVector, int? sortIndex, decimal[] vector)
            : this()
        {
            this.ScopeVectors = new Dictionary<ValueVector<Guid>, int?> { { scopeVector, sortIndex } };
            this.RosterVector = vector;
        }

        public decimal[] RosterVector { get; set; }
        public Dictionary<ValueVector<Guid>, int?> ScopeVectors { get; set; }
        public HashSet<Guid> DisabledGroups { get; set; }
        public Dictionary<Guid, string> RosterRowTitles { set; get; }
        public Dictionary<Guid, InterviewQuestion> QuestionsSearchCache { set; get; }
        public Dictionary<Guid, object> Variables { set; get; }
        public HashSet<Guid> DisabledVariables { get; set; }
        public Dictionary<Guid, InterviewStaticText> StaticTexts { get; set; }
    }
}
