using System;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    public class CloneQuestionnaire : QuestionnaireCommand
    {
        public CloneQuestionnaire(Guid questionnaireId, string title, Guid responsibleId, bool isPublic, QuestionnaireDocument doc)
            : base(questionnaireId, responsibleId)
        {
            this.ResponsibleId = responsibleId;
            this.Title = CommandUtils.SanitizeHtml(title);
            this.IsPublic = isPublic;
            this.Source = doc;
        }

        public string Title { get; private set; }

        public bool IsPublic { get; private set; }

        public QuestionnaireDocument Source { get; private set; }
    }
}