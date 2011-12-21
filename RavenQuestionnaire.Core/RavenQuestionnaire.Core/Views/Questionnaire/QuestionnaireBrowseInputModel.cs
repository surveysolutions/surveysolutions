namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    public class QuestionnaireBrowseInputModel
    {
        public int Page { get { return _page; }
            set { _page = value; }
        }

        private int _page = 1;
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }
        private int _pageSize = 20;
    }
}
