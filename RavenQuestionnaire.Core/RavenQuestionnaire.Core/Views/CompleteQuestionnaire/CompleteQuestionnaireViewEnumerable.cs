using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Questionnaire;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireViewEnumerable
    {
        public string Id { get; set; }
        public string Title { get;  set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate{ get; set; }

        public string Status { get; set; }

        public string ResponsibleId { set; get; }
        public CompleteQuestionView CurrentQuestion { get; set; }


        protected CompleteAnswer[] CompleteAnswers { get; set; }

        public CompleteQuestionnaireViewEnumerable(string id,string title, CompleteAnswer[] answers, DateTime creationDate,
                                               DateTime lastEntryDate, string status, string responsibleId, CompleteQuestionView currentQuestion)
        {
            this.Id = IdUtil.ParseId(id);
            this.Title = title;
            this.CompleteAnswers = answers;
            this.CreationDate = creationDate;
            this.LastEntryDate = lastEntryDate;
            this.Status = status;
            this.ResponsibleId = responsibleId;
            this.CurrentQuestion = currentQuestion;
            if (CurrentQuestion != null)
                MerdgeAnswersWithResults();

        }
        public CompleteQuestionnaireViewEnumerable(QuestionnaireView template)
        {
            this.Title = template.Title;
            this.CurrentQuestion = new CompleteQuestionView(template.Questions[0]);
            CompleteAnswers = new CompleteAnswer[0];
        }
        protected void MerdgeAnswersWithResults()
        {
            foreach (var answer in CurrentQuestion.Answers)
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
