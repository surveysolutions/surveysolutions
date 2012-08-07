using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewersInputModel
    {
        private int _page = 1;

        private int _pageSize = 20;

        public int Page
        {
            get { return _page; }
            set { _page = value; }
        }

        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        public UserLight Supervisor { get; set; }
        public bool AllSubordinateUsers { get; set; }
    }
}