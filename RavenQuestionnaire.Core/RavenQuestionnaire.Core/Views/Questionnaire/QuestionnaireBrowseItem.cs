using System;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    public class QuestionnaireBrowseItem
    {
        public string Id
        {
            get { return IdUtil.ParseId(_id); }
            private set { _id = value; }
        }

        private string _id;
        public string Title
        {
            get;
            private set;
        }

        public DateTime CreationDate
        {
            get;
            private set;
        }
        public DateTime LastEntryDate
        {
            get;
            private set;
        }
        public QuestionnaireBrowseItem(string id, string title, DateTime creationDate, DateTime lastEntryDate)
        {
            this.Id = id;
            this.Title = title;
            this.CreationDate = creationDate;
            this.LastEntryDate = lastEntryDate;
        }
        public static QuestionnaireBrowseItem New()
        {
            return new QuestionnaireBrowseItem(null, null, DateTime.Now, DateTime.Now);
        }
    }
}
