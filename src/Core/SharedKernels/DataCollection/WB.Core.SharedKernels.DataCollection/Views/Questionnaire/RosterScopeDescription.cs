using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class RosterScopeDescription
    {
        public RosterScopeDescription(Guid scopeId, Dictionary<Guid, Guid?> rosterIdToRosterTitleQuestionIdMap)
        {
            this.ScopeId = scopeId;
            this.RosterIdToRosterTitleQuestionIdMap = rosterIdToRosterTitleQuestionIdMap ?? new Dictionary<Guid, Guid?>();
        }

        public Guid ScopeId { get; private set; }
        public Dictionary<Guid, Guid?> RosterIdToRosterTitleQuestionIdMap { get; private set; }
    }
}
