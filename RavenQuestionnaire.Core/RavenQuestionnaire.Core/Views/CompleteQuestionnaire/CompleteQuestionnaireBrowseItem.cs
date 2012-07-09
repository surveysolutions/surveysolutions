using System;
using System.Linq;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Statistics;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireBrowseItem
    {
        public string CompleteQuestionnaireId
        {
            get { return IdUtil.ParseId(_id); }
             set { _id = value; }
        }

        private string _id;
        public string QuestionnaireTitle { get;  set; }
        public string TemplateId { get;  set; }
        public DateTime CreationDate { get;  set; }
        public DateTime LastEntryDate { get;  set; }
        public int TotalQuestionCount { get;  set; }
        public int AnsweredQuestionCount { get;  set; }
        public UserLight Responsible { get;  set; }
        public QuestionStatisticView[] FeaturedQuestions { get;  set; }
        public SurveyStatus Status { get;  set; }
        public UserLight Creator { get; set; }

        protected CompleteQuestionnaireBrowseItem()
        {
            this.FeaturedQuestions = new QuestionStatisticView[0];
        }

        public CompleteQuestionnaireBrowseItem(string completeQuestionnaireId, string templateId, string questionnaireTitle, DateTime creationDate,
                                               DateTime lastEntryDate, SurveyStatus status,int totalQuestionCount, int answeredQuestionCount, UserLight responsible):this()
        {
            this.CompleteQuestionnaireId = completeQuestionnaireId;
            this.TemplateId = templateId;
            this.QuestionnaireTitle = questionnaireTitle;
            this.CreationDate = creationDate;
            this.LastEntryDate = lastEntryDate;
            this.Status = status;
            this.TotalQuestionCount = totalQuestionCount;
            this.AnsweredQuestionCount = answeredQuestionCount;
            this.Responsible = responsible;
           
            
        }
        public CompleteQuestionnaireBrowseItem(CompleteQuestionnaireStatisticDocument doc)
            : this()
        {
            this.CompleteQuestionnaireId = doc.CompleteQuestionnaireId;
            this.QuestionnaireTitle = doc.Title;
            this.CreationDate = doc.StartDate;
            this.LastEntryDate = doc.EndDate ?? DateTime.Now;
            this.Status = doc.Status;
            this.TotalQuestionCount = doc.TotalQuestionCount;
            this.AnsweredQuestionCount = doc.AnsweredQuestions.Count;
            this.Creator = doc.Creator;
         //   this.FeaturedQuestions = doc.FeturedQuestions.Select(q => new QuestionStatisticView(q)).ToArray();
            //this.Responsible = doc.r;
        }
    }
}
