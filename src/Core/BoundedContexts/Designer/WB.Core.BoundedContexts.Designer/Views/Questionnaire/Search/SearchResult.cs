using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search
{
    public class SearchResult
    {
        public IList<SearchResultEntity> Items { get; set; } = new List<SearchResultEntity>();
        public int TotalCount { get; set; }
    }
}
