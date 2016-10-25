using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    public class ImportQuestionnaire : QuestionnaireCommand
    {
        public ImportQuestionnaire(Guid responsibleId, QuestionnaireDocument source)
            : base(source.PublicKey, responsibleId)
        {
            Source = source;
        }

        public QuestionnaireDocument Source { get; private set; }
    }
}
