using System;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.Views.Question
{
    public class CompleteQuestionView : QuestionView<CompleteGroup, CompleteQuestion, CompleteAnswer>
    {
        public bool Enabled { get; set; }

        public CompleteQuestionView()
        {
        }
        public CompleteQuestionView(string questionnaireId, Guid? groupPublicKey)
            : base(questionnaireId, groupPublicKey)
        {
        }
        protected CompleteQuestionView(IQuestionnaireDocument questionnaire, IQuestion doc)
            : base(questionnaire, doc)
        {
        }
        public CompleteQuestionView(
            CompleteQuestionnaireDocument questionnaire, CompleteQuestion doc)
            : base(questionnaire, doc)
        {
            this.QuestionnaireId = questionnaire.Id;
            this.Enabled = doc.Enabled;
            this.Answers = doc.Answers.Select(a => new CompleteAnswerView(a)).ToArray();
        }
    }
}
