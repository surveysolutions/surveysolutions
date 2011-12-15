using System;
using System.Linq;
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

        public int Status { get; set; }

        public CompleteQuestionView[] Questions
        {
            get { return _questions; }
        /*    set
            {
                _questions = value;
                for (int i = 0; i < this._questions.Length; i++)
                {
                    this._questions[i].Index = i + 1;
                }

            }*/
        }

        private CompleteQuestionView[] _questions;

        private QuestionnaireView Questionnaire { get; set; }

        protected CompleteAnswer[] CompleteAnswers { get; set; }

        public CompleteQuestionnaireView(string id, QuestionnaireView template, CompleteAnswer[] answers, DateTime creationDate,
                                               DateTime lastEntryDate, int status):this(template)
        {
            this.Id = IdUtil.ParseId(id);
            this.CompleteAnswers = answers;
            this.CreationDate = creationDate;
            this.LastEntryDate = lastEntryDate;
            this.Status = status;
            MerdgeAnswersWithResults();

        }
        public CompleteQuestionnaireView(QuestionnaireView template)
        {
            this.Questionnaire = template;
            CompleteAnswers = new CompleteAnswer[0];
            _questions = this.Questionnaire.Questions.Select(q => new CompleteQuestionView(q)).ToArray();
        }

        public static CompleteQuestionnaireView New(QuestionnaireView template)
        {
            return new CompleteQuestionnaireView(template);
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
