using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables
{
    [Serializable]
    public class AddLookupTable : QuestionnaireCommand
    {
        public AddLookupTable(
            Guid questionnaireId, 
            string lookupTableName, 
            string lookupTableFileName,
            Guid lookupTableId,
            Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId)
        {
            this.LookupTableId = lookupTableId;
            this.LookupTableName = lookupTableName;
            this.LookupTableFileName = lookupTableFileName;
        }

        public Guid LookupTableId { get; private set; }
        public string LookupTableName { get; private set; }
        public string LookupTableFileName { get; private set; }
    }
}