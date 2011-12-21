namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireBrowseInputModel
    {
        public int Page
        {
            get { return _page; }
            set { _page = value; }
        }

        private int _page = 1;
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }
        private int _pageSize = 20;


        public string ResponsibleId
        {
            get { return _responsibleId; }
            set { _responsibleId = value; }
        }

        private string _responsibleId = "";
    }
}
