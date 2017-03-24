using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class RosterStructureService : IRosterStructureService
    {
        public Dictionary<ValueVector<Guid>, RosterScopeDescription> GetRosterScopes(QuestionnaireDocument document)
        {
            var rosterScopesFiller = new RosterScopesFiller(document);
            rosterScopesFiller.FillRosterScopes();
            return rosterScopesFiller.Result;
        }
    }
}