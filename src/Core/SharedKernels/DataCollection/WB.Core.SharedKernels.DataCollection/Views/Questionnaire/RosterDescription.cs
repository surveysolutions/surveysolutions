using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class RosterDescription
    {
        public RosterDescription(Guid scopeId, Dictionary<Guid, Guid> rosterGroupsWithTitleQuestionPairs)
        {
            this.ScopeId = scopeId;
            this.RosterGroupsWithTitleQuestionPairs = rosterGroupsWithTitleQuestionPairs;
            this.RosterGroupsId = new HashSet<Guid>();
        }

        public Guid ScopeId { get; private set; }
        public Dictionary<Guid, Guid> RosterGroupsWithTitleQuestionPairs { get; private set; }
        public HashSet<Guid> RosterGroupsId { get; private set; }
    }
}
