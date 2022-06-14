using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public class QuestionnaireRevision
    {
        public QuestionnaireRevision(Guid questionnaireId, Guid? revision = null, int? version = null)
        {
            this.QuestionnaireId = questionnaireId;
            this.Revision = revision;
            this.Version = version;
        }

        public QuestionnaireRevision(string questionnaireId)
        {
            this.QuestionnaireId = Guid.Parse(questionnaireId);
        }

        public Guid QuestionnaireId { get; }
        public Guid? OriginalQuestionnaireId { get; private set; }
        public Guid? Revision { get; }
        public int? Version { get; }

        public void MarkAsAnonymousQuestionnaire(Guid originalQuestionnaireId)
        {
            OriginalQuestionnaireId = originalQuestionnaireId;
        }

        public override string ToString() => Revision.HasValue ? $"{QuestionnaireId:N}${Revision:N}" : $"{QuestionnaireId:N}";
    }
}
