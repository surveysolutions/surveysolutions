using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IRostrerStructureService
    {
        Dictionary<ValueVector<Guid>, RosterScopeDescription> GetRosterScopes(QuestionnaireDocument document);
    }
}
