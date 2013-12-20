using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class RosterScopeDescription
    {
        public RosterScopeDescription(Guid scopeId, Dictionary<Guid, Guid?> rosterGroupsWithTitleQuestionPairs)
        {
            this.ScopeId = scopeId;
            this.RosterIdMappedOfRosterTitleQuestionId = rosterGroupsWithTitleQuestionPairs ?? new Dictionary<Guid, Guid?>();
        }

        public Guid ScopeId { get; private set; }
        public Dictionary<Guid, Guid?> RosterIdMappedOfRosterTitleQuestionId { get; private set; }
    }
}
