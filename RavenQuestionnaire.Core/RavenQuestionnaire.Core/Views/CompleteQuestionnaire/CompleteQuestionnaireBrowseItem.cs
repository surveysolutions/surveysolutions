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
        public string Id
        {
            get { return IdUtil.ParseId(_id); }
            private set { _id = value; }
        }

        private string _id;
        public string QuestionnaireTitle { get; private set; }
        public DateTime CreationDate { get; private set; }
        public DateTime LastEntryDate { get; private set; }
        public int TotalQuestionCouont { get; private set; }
        public int AnsweredQuestionCouont { get; private set; }
        public UserLight Responsible { get; private set; }
        public QuestionStatisticView[] FeaturedQuestions { get; private set; }
        public SurveyStatus Status { get; private set; }
        public UserLight Creator { get; set; }
        public CompleteQuestionnaireBrowseItem(string id, string questionnaireTitle, DateTime creationDate,
                                               DateTime lastEntryDate, SurveyStatus status,int totalQuestionCount, int answeredQuestionCount, UserLight responsible)
        {
            this.Id = id;
            this.QuestionnaireTitle = questionnaireTitle;
            this.CreationDate = creationDate;
            this.LastEntryDate = lastEntryDate;
            this.Status = status;
            this.TotalQuestionCouont = totalQuestionCount;
            this.AnsweredQuestionCouont = answeredQuestionCount;
            this.Responsible = responsible;
        }
        public CompleteQuestionnaireBrowseItem(CompleteQuestionnaireStatisticDocument doc)
        {
            this.Id = doc.CompleteQuestionnaireId;
            this.QuestionnaireTitle = doc.Title;
            this.CreationDate = doc.StartDate;
            this.LastEntryDate = doc.EndDate ?? DateTime.Now;
            this.Status = doc.Status;
            this.TotalQuestionCouont = doc.TotalQuestionCount;
            this.AnsweredQuestionCouont = doc.AnsweredQuestions.Count;
            this.Creator = doc.Creator;
            this.FeaturedQuestions = doc.FeturedQuestions.Select(q => new QuestionStatisticView(q)).ToArray();
            //this.Responsible = doc.r;
        }
    }
}
