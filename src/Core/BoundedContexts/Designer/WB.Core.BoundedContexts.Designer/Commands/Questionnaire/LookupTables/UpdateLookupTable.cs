using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables
{
    [Serializable]
    public class UpdateLookupTable : QuestionnaireCommand
    {
        public UpdateLookupTable(
            Guid questionnaireId,
            Guid lookupTableId,
            Guid responsibleId,
            string lookupTableName,
            string lookupTableFileName,
            Guid? oldLookupTableId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId)
        {
            this.LookupTableId = lookupTableId;
            this.LookupTableName = lookupTableName;
            this.LookupTableFileName = lookupTableFileName;
            this.OldLookupTableId = oldLookupTableId;
        }

        public Guid? OldLookupTableId { get; set; }
        public Guid LookupTableId { get; private set; }
        public string LookupTableName { get; private set; }
        public string LookupTableFileName { get; set; }
    }
}