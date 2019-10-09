using System;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    public class ImportQuestionnaireToHq : QuestionnaireCommand
    {
        public ImportQuestionnaireToHq(Guid responsibleId, QuestionnaireChangeRecordMetadata metadata, QuestionnaireDocument source)
            : base(source.PublicKey, responsibleId)
        {
            Metadata = metadata;
            Source = source;            
        }

        public QuestionnaireChangeRecordMetadata Metadata { get; }
        public QuestionnaireDocument Source { get; }
    }
}
