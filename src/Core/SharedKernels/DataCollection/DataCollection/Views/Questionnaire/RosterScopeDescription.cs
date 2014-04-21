using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class RosterScopeDescription
    {
        public RosterScopeDescription(Guid scopeId, string scopeName, bool isTextListRoster, Dictionary<Guid, RosterTitleQuestionDescription> rosterIdToRosterTitleQuestionIdMap, Dictionary<Guid, Guid[]> rosterIdToRosterVectorMap)
        {
            this.RosterIdToRosterVectorMap = rosterIdToRosterVectorMap;
            this.ScopeId = scopeId;
            this.ScopeTriggerName = scopeName;
            this.IsTextListRoster = isTextListRoster;
            this.RosterIdToRosterTitleQuestionIdMap = rosterIdToRosterTitleQuestionIdMap ?? new Dictionary<Guid, RosterTitleQuestionDescription>();
        }

        public Guid ScopeId { get; private set; }
        public string ScopeTriggerName { get; private set; }
        public bool IsTextListRoster { get; private set; }
        public Dictionary<Guid, RosterTitleQuestionDescription> RosterIdToRosterTitleQuestionIdMap { get; private set; }
        public Dictionary<Guid, Guid[]> RosterIdToRosterVectorMap { get; private set; }
    }
}
