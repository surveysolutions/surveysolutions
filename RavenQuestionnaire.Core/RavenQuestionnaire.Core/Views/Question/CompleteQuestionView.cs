using System;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.Answer;
using RavenQuestionnaire.Core.Views.Card;

namespace RavenQuestionnaire.Core.Views.Question
{
    public class CompleteQuestionView : QuestionView<CompleteAnswerView,ICompleteGroup, ICompleteQuestion>
    {
        public bool Enabled { get; set; }
        public bool Valid { get; set; }
        public bool Answered { get; set; }

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
            CompleteQuestionnaireDocument questionnaire, ICompleteQuestion doc)
            : base(questionnaire, doc)
        {
            this.QuestionnaireId = questionnaire.Id;
            this.Valid = doc.Valid;
            this.Enabled = doc.Enabled;
            this.Answers = doc.Children.OfType<ICompleteAnswer>().Select(a => new CompleteAnswerView(doc.PublicKey,a)).ToArray();
            this.Answered = this.Answers.Any(a => a.Selected == true);
            if (doc.Cards != null)
            {
                this.Cards = doc.Cards.Select(card => new CardView(doc.PublicKey, card)).OrderBy(a => Guid.NewGuid()).ToArray();
            }
        }
    }
}
