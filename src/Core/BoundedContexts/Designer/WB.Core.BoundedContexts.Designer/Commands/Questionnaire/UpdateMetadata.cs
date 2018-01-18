using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    public class UpdateMetadata : QuestionnaireCommand
    {
        public UpdateMetadata(Guid questionnaireId, string title, QuestionnaireMetaInfo metadata, Guid responsibleId)
            : base(questionnaireId, responsibleId)
        {
            this.ResponsibleId = responsibleId;
            this.Title = CommandUtils.SanitizeHtml(title);
            this.Metadata = metadata;
        }

        public string Title { get; private set; }

        public QuestionnaireMetaInfo Metadata { get; private set; }
    }
}