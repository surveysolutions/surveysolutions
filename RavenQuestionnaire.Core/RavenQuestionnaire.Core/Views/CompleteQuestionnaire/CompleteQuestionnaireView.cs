using System;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Questionnaire;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireView
    {
        public string Id { get; set; }
        public string Title
        {
            get { return Questionnaire.Title; }
        }
        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate{ get; set; }

        public string Status { get; set; }

        public string ResponsibleId { set; get; }

        public CompleteQuestionView[] Questions
        {
            get { return _questions; }
        }

        private CompleteQuestionView[] _questions;

        private QuestionnaireView Questionnaire { get; set; }

        protected CompleteAnswer[] CompleteAnswers { get; set; }

        public CompleteQuestionnaireView( CompleteQuestionnaireDocument doc)
        {
            this.Id = IdUtil.ParseId(doc.Id);
            this.Questionnaire = new QuestionnaireView(doc.Questionnaire);
            this.CompleteAnswers = doc.CompletedAnswers.ToArray();
            this.CreationDate = doc.CreationDate;
            this.LastEntryDate = doc.LastEntryDate;
            this.Status = doc.Status;
            this.ResponsibleId = doc.ResponsibleId;
            //TODO _question may be redundant
            _questions =
                doc.Questionnaire.Questions.Select(q => new CompleteQuestionView(q, doc.Questionnaire.Id)).ToArray();
            MerdgeAnswersWithResults();

        }
        public CompleteQuestionnaireView(QuestionnaireDocument template)
        {
            this.Questionnaire = new QuestionnaireView(template);
            CompleteAnswers = new CompleteAnswer[0];
            _questions = template.Questions.Select(q => new CompleteQuestionView(q, template.Id)).ToArray();
        }

        protected void MerdgeAnswersWithResults()
        {
            foreach (var answer in Questions.SelectMany(q=>q.Answers))
            {
                var completeAnswer = CompleteAnswers.Where(a => a.PublicKey.Equals(answer.PublicKey)).FirstOrDefault();
                if(completeAnswer!=null)
                {
                    answer.Selected = true;
                    answer.CustomAnswer = completeAnswer.CustomAnswer;
                }
            }
        }
    }
}
