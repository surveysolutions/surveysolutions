using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto.LookupTables
{
    public class LookupTableAdded: QuestionnaireActive
    {
        public LookupTableAdded() { }

        public LookupTableAdded(Guid lookupTableId, string lookupTableName, string lookupTableFileName, Guid responsibleId)
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