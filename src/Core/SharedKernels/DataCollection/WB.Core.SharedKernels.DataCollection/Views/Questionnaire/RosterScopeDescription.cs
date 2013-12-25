using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class RosterScopeDescription
    {
        public RosterScopeDescription(Guid scopeId, Dictionary<Guid, RosterTitleQuestionDescription> rosterIdToRosterTitleQuestionIdMap)
        {
            this.ScopeId = scopeId;
            this.RosterIdToRosterTitleQuestionIdMap = rosterIdToRosterTitleQuestionIdMap ?? new Dictionary<Guid, RosterTitleQuestionDescription>();
        }

        public Guid ScopeId { get; private set; }
        public Dictionary<Guid, RosterTitleQuestionDescription> RosterIdToRosterTitleQuestionIdMap { get; private set; }
    }
}
