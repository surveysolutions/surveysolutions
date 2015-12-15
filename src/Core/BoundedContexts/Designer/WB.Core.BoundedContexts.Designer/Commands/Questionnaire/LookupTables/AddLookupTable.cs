using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables
{
    [Serializable]
    public class AddLookupTable : QuestionnaireCommand
    {
        public AddLookupTable(
            Guid questionnaireId, 
            Guid lookupTableId, 
            Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId)
        {
            this.LookupTableId = lookupTableId;
        }

        public Guid LookupTableId { get; private set; }
        public string LookupTableName { get; set; }
        public string LookupTableFileName { get; set; }
    }
}