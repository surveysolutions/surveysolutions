namespace RavenQuestionnaire.Core.Views.Status
{
    public class StatusBrowseInputModel
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


        public string QId { get; set; }
    }
}
