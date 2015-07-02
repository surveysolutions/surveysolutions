using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewLevel
    {
        public InterviewLevel()
        {
            this.ScopeVectors = new Dictionary<ValueVector<Guid>, int?>();
            this.DisabledGroups = new HashSet<Guid>();
            this.RosterRowTitles = new Dictionary<Guid, string>();
            this.QuestionsSearchCahche = new Dictionary<Guid, InterviewQuestion>();
        }
        public InterviewLevel(ValueVector<Guid> scopeVector, int? sortIndex, decimal[] vector)
            : this()
        {
            this.ScopeVectors = new Dictionary<ValueVector<Guid>, int?>() { { scopeVector, sortIndex } };
            this.RosterVector = vector;
        }

        public decimal[] RosterVector { get; set; }
        public Dictionary<ValueVector<Guid>, int?> ScopeVectors { get; set; }
        public HashSet<Guid> DisabledGroups { get; set; }
        public Dictionary<Guid, string> RosterRowTitles { set; get; }
        public Dictionary<Guid, InterviewQuestion> QuestionsSearchCahche { set; get; }
        
    }
}
