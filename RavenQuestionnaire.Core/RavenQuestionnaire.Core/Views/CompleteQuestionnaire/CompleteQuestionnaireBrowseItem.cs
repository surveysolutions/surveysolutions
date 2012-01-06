using System;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

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

        public UserLight Responsible { get; private set; }

        public SurveyStatus Status { get; private set; }

        public CompleteQuestionnaireBrowseItem(string id, string questionnaireTitle, DateTime creationDate,
                                               DateTime lastEntryDate, SurveyStatus status, UserLight responsible)
        {
            this.Id = id;
            this.QuestionnaireTitle = questionnaireTitle;
            this.CreationDate = creationDate;
            this.LastEntryDate = lastEntryDate;
            this.Status = status;
            this.Responsible = responsible;
        }
    }
}
