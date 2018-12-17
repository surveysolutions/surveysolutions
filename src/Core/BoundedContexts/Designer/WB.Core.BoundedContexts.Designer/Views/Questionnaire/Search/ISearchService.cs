using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search
{
    public interface ISearchService
    {
        SearchResult PerformSearch(SearchInput search);
    }

    public class SearchService : ISearchService
    {
        private const string ScriptName = "WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search.SearchQuestions.sql";

        private readonly IUnitOfWork unitOfWork;

        public SearchService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public SearchResult PerformSearch(SearchInput search)
        {
            var searchResult = new SearchResult();

            searchResult.TotalCount = ExecuteCountQuery(search);
            searchResult.Items = ExecuteSearchQuery(search).ToList();

            return searchResult;
        }

        private int ExecuteCountQuery(SearchInput input)
        {
            return 0;
        }

        private IEnumerable<SearchResultEntity> ExecuteSearchQuery(SearchInput input)
        {
            string query = GetSqlQueryForInterviews(ScriptName);

            var items = unitOfWork.Session.Connection.Query<SearchResultEntity>(query, new
            {

            });
            return items;
        }

        private static string GetSqlQueryForInterviews(string scriptName)
        {
            var assembly = typeof(SearchService).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(scriptName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string query = reader.ReadToEnd();
                return query;
            }
        }
    }
}
