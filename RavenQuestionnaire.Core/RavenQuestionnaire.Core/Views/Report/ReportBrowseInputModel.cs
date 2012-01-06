namespace RavenQuestionnaire.Core.Views.Report
{
    public class ReportBrowseInputModel
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
        private int _pageSize = 10;

    }
}
