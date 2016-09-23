using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto.LookupTables
{
    public class LookupTableUpdated: QuestionnaireActive
    {
        public LookupTableUpdated() { }

        public LookupTableUpdated(Guid lookupTableId, string lookupTableName, string lookupTableFileName, Guid responsibleId)
        {
            this.LookupTableId = lookupTableId;
            this.LookupTableName = lookupTableName;
            this.LookupTableFileName = lookupTableFileName;
            this.ResponsibleId = responsibleId;
        }

        public Guid LookupTableId { get; set; }

        public string LookupTableName { get; set; }

        public string LookupTableFileName { get; set; }
    }
}