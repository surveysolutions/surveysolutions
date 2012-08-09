using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Views.Survey
{
    public class SurveyBrowseView
    {
        public int PageSize { get; private set; }

        public int Page { get; private set;}

        public int TotalCount { get; private set; }

        public List<SurveysBrowseItem> Items { get; set; }

        public SurveyBrowseView()
        {
            this.Items=new List<SurveysBrowseItem>();
        }

        public SurveyBrowseView(int page, int pageSize, int totalCount):this()
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
        }
    }
}
