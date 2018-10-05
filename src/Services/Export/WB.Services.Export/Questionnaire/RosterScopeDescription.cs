using System;
using System.Collections.Generic;
using WB.Services.Export.Interview;

namespace WB.Services.Export.Questionnaire
{
    public class RosterScopeDescription
    {
        public RosterScopeDescription(ValueVector<Guid> scopeVector, string sizeQuestionTitle, RosterScopeType scopeType,
            Dictionary<Guid, RosterTitleQuestionDescription> rosterIdToRosterTitleQuestionIdMap)
        {
            this.ScopeVector = scopeVector;
            this.SizeQuestionTitle = sizeQuestionTitle;
            this.Type = scopeType;
            this.RosterIdToRosterTitleQuestionIdMap = rosterIdToRosterTitleQuestionIdMap ??
                                                      new Dictionary<Guid, RosterTitleQuestionDescription>();
        }

        public ValueVector<Guid> ScopeVector { get; private set; }
        public string SizeQuestionTitle { get; private set; }
        public RosterScopeType Type { get; private set; }
        public Dictionary<Guid, RosterTitleQuestionDescription> RosterIdToRosterTitleQuestionIdMap { get; private set; }
    }
}