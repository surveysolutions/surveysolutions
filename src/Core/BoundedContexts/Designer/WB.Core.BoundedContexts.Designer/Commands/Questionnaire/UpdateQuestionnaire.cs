using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    public class UpdateQuestionnaire : QuestionnaireCommand
    {
        public UpdateQuestionnaire(Guid questionnaireId, string title, bool isPublic, Guid responsibleId, bool isResponsibleAdmin)
            : base(questionnaireId, responsibleId, isResponsibleAdmin)
        {
            this.Title = CommandUtils.SanitizeHtml(title);
            this.IsPublic = isPublic;
        }

        public string Title { get; private set; }

        public bool IsPublic { get; private set; }
    }
}