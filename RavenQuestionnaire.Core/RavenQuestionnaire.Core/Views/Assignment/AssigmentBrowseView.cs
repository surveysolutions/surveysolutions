using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Views.Assignment
{
    public class AssigmentBrowseView
    {
        public int PageSize { get; private set; }

        public int Page { get; private set;}

        public int TotalCount { get; private set; }

        public List<AssigmentBrowseItem> Items { get; set; }

        public string TemplateId { set; get; }

        public AssigmentBrowseView()
        {
        }

        public AssigmentBrowseView(int page, int pageSize, int totalCount, string templateId)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.TemplateId = templateId;
        }
    }
}
