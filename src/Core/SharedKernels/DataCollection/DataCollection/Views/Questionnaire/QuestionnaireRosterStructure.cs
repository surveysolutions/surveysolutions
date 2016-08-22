using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class QuestionnaireRosterStructure : IView
    {
        public QuestionnaireRosterStructure()
        {
            this.RosterScopes = new Dictionary<ValueVector<Guid>, RosterScopeDescription>();
        }

        public Guid QuestionnaireId { get; set; }
        public Dictionary<ValueVector<Guid>, RosterScopeDescription> RosterScopes { get; set; }
        public long Version { get; set; }
    }
}
