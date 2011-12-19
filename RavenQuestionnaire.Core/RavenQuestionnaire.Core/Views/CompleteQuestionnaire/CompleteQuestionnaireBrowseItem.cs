using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Questionnaire;

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

        public string ResponsibleId { get; private set; }

        public string StatusId { get; private set; }

        public CompleteQuestionnaireBrowseItem(string id, string questionnaireTitle, DateTime creationDate,
                                               DateTime lastEntryDate, string statusId, string responsibleId)
        {
            this.Id = id;
            this.QuestionnaireTitle = questionnaireTitle;
            this.CreationDate = creationDate;
            this.LastEntryDate = lastEntryDate;
            this.StatusId = statusId;
            this.ResponsibleId = responsibleId;
        }
    }
}
