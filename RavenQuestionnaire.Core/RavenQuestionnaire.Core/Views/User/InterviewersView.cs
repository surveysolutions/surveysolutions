using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewersView
    {
        public int PageSize { get; private set; }

        public int Page { get; private set; }

        public string Order 
        {
            get { return _order; }
            set { _order = value; }
        }
        private string _order = string.Empty;

        public string SupervisorId { get; private set; }

        public string SupervisorName { get; private set; }

        public int TotalCount { get; private set; }

        public IEnumerable<InterviewersItem> Items { get; private set; }

        public InterviewersView(int page, int pageSize, int totalCount, IEnumerable<InterviewersItem> items, string supervisorId, string supervisorName)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
            this.SupervisorId = supervisorId;
            this.SupervisorName = supervisorName;
        }
    }
}
