using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class QuestionnaireRosterStructure : IVersionedView
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
