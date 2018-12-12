using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search
{
    public interface ISearchService
    {
        SearchResult PerformSearch(SearchInput search);
    }

    class SearchService : ISearchService
    {
        public SearchResult PerformSearch(SearchInput search)
        {
            throw new System.NotImplementedException();
        }
    }
}
