using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class RosterScopeDescription
    {
        public RosterScopeDescription(ValueVector<Guid> scopeVector, string scopeName, RosterScopeType scopeType,
            Dictionary<Guid, RosterTitleQuestionDescription> rosterIdToRosterTitleQuestionIdMap)
        {
            this.ScopeVector = scopeVector;
            this.ScopeTriggerName = scopeName;
            this.ScopeType = scopeType;
            this.RosterIdToRosterTitleQuestionIdMap = rosterIdToRosterTitleQuestionIdMap ??
                new Dictionary<Guid, RosterTitleQuestionDescription>();
        }

        public ValueVector<Guid> ScopeVector { get; private set; }
        public string ScopeTriggerName { get; private set; }
        public RosterScopeType ScopeType { get; private set; }
        public Dictionary<Guid, RosterTitleQuestionDescription> RosterIdToRosterTitleQuestionIdMap { get; private set; }
    }
}
