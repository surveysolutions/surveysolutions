using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    public class CreateQuestionnaire : QuestionnaireCommand
    {
        public CreateQuestionnaire(Guid questionnaireId, string text, Guid responsibleId, bool isPublic = false)
            : base(questionnaireId, responsibleId)
        {
            this.Title = CommandUtils.SanitizeHtml(text);
            this.ResponsibleId = responsibleId;
            this.IsPublic = isPublic;
        }

        public string Title { get; private set; }

        public bool IsPublic { get; private set; }
    }
}