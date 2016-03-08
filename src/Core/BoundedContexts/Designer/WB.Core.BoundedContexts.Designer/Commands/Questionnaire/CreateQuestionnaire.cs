using System;
using Ncqrs.Commanding;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    public class CreateQuestionnaire : CommandBase
    {
        public CreateQuestionnaire(Guid questionnaireId, string text, Guid? createdBy = null, bool isPublic = false)
            : base(questionnaireId)
        {
            this.PublicKey = questionnaireId;
            this.Title = CommandUtils.SanitizeHtml(text);
            this.CreatedBy = createdBy;
            this.IsPublic = isPublic;
        }

        public Guid PublicKey { get; private set; }

        public string Title { get; private set; }

        public Guid? CreatedBy { get; private set; }

        public bool IsPublic { get; private set; }
    }
}