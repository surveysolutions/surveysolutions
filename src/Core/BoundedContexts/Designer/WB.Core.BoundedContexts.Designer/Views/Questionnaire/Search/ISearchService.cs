using System.Threading.Tasks;
using Npgsql;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search
{
    public interface ISearchService
    {
        SearchResult PerformSearch(SearchInput search);
    }
}
