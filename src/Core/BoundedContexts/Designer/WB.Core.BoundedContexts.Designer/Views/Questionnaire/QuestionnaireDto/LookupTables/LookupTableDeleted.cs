using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto.LookupTables
{
    public class LookupTableDeleted : QuestionnaireActive
    {
        public LookupTableDeleted() { }

        public LookupTableDeleted(Guid lookupTableId, Guid responsibleId)
        {
            this.LookupTableId = lookupTableId;
            this.ResponsibleId = responsibleId;
        }

        public Guid LookupTableId { get; set; }
    }
}