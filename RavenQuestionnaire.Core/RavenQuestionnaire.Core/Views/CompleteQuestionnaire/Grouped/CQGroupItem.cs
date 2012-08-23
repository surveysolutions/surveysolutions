using System.Collections.Generic;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped
{
    public class CQGroupItem
    {
           public int PageSize
        {
            get;
            set;
        }

        public int Page
        {
            get;
            set;
        }

        public int TotalCount { get; set; }

        public IList<CompleteQuestionnaireBrowseItem> Items
        {
            get;
            set;
        }

        public string SurveyTitle { get; set; }
        public string SurveyId
        {
            get { return IdUtil.ParseId(_id); }
            set { _id = value; }
        }

        private string _id;

        public CQGroupItem()
        {
            this.Items = new CompleteQuestionnaireBrowseItem[0];
        }

        public CQGroupItem(int page, int pageSize, int totalCount, /*IEnumerable<CompleteQuestionnaireStatisticDocument> items,*/ string title, string id):this()
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
          
     //       this.Items = items.Select(x => new CompleteQuestionnaireBrowseItem(x));
            this.SurveyTitle = title;
            this.SurveyId = id;
        }
    }
}
