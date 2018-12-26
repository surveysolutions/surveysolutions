using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search
{
    public class SearchResult
    {
        public IList<SearchResultEntity> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
