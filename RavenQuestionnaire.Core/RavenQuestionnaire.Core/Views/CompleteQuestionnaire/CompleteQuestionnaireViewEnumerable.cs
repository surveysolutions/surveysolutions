using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Iterators;
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

        public CompleteQuestionnaireViewEnumerable(CompleteQuestionnaireDocument doc, RavenQuestionnaire.Core.Entities.SubEntities.Question currentQuestion)
        {
            this.Id = IdUtil.ParseId(doc.Id);
            this.Title = doc.Questionnaire.Title;
            this.CompleteAnswers = doc.CompletedAnswers.ToArray();
            this.CreationDate = doc.CreationDate;
            this.LastEntryDate = doc.LastEntryDate;
            this.Status = doc.Status;
            this.ResponsibleId = doc.ResponsibleId;
            if(currentQuestion!=null)
                this.CurrentQuestion = new CompleteQuestionView(currentQuestion, doc.Questionnaire.Id);
            if (CurrentQuestion != null)
                MerdgeAnswersWithResults();

        }
        public CompleteQuestionnaireViewEnumerable(QuestionnaireDocument template)
        {
            this.Title = template.Title;
            this.CurrentQuestion = new CompleteQuestionView(template.Questions[0], template.Id);
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
