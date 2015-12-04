using System;
using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire.LookupTables
{
    public class LookupTableAdded: QuestionnaireActiveEvent
    {
        public LookupTableAdded() { }

        public LookupTableAdded(Guid lookupTableId, Guid responsibleId)
        {
            this.LookupTableId = lookupTableId;
            this.ResponsibleId = responsibleId;
        }

        public Guid LookupTableId { get; set; }
    }
}