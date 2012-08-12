using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Views.Survey
{
    public class SurveyBrowseView
    {
        public int PageSize { get; private set; }

        public int Page { get; private set;}

        public int TotalCount { get; private set; }

        public List<SurveyBrowseItem> Items { get; set; }

        public List<string> Headers { get; set; }

        public SurveyBrowseView()
        {
            this.Items = new List<SurveyBrowseItem>();
            this.Headers=new List<string>();
        }

        public SurveyBrowseView(int page, int pageSize, int totalCount):this()
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
        }
    }
}
